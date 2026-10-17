using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Game
{
	/// <summary>
	/// Outputs text through a screen reader using the native Prism speech library (prism.dll).
	/// </summary>
	public static class ScreenReader
	{
		private const ulong BackendSapi = 2120622734136241766;
		private const ulong BackendAvSpeech = 2946271790952897572;
		private const ulong BackendSpeechDispatcher = 16417582814105037822;
		private const ulong BackendOneCore = 7464667106434960564;
		private const ulong BackendAndroidTts = 13553398574290888140;
		private const ulong BackendWebSpeech = 3851232497565452943;

		private const ulong FeatureIsSupportedAtRuntime = 1;
		private const int ErrorNone = 0;
		private const int ErrorNotInitialized = 1;
		private const int ErrorAlreadyInitialized = 15;

		/// <summary>
		/// Minimal time between two attempts to find a screen reader.
		/// </summary>
		private const double RetrySeconds = 2;

		/// <summary>
		/// Backends that synthesize speech themselves. They're skipped so the game talks only through a screen reader.
		/// </summary>
		private static readonly HashSet<ulong> _synthesizers = new()
		{
			BackendSapi,
			BackendOneCore,
			BackendAvSpeech,
			BackendSpeechDispatcher,
			BackendAndroidTts,
			BackendWebSpeech
		};

		private static readonly object _lock = new();
		private static IntPtr _context;
		private static IntPtr _backend;
		private static long _lastAttempt;

		/// <summary>
		/// Initializes the library.
		/// </summary>
		public static void Load()
		{
			lock (_lock)
			{
				if (_context != IntPtr.Zero)
					return;

				try
				{
					NativeConfig config = Native.prism_config_init();
					_context = Native.prism_init(ref config);
					if (_context == IntPtr.Zero)
						Logger.LogError("Čtečka obrazovky", "Knihovnu Prism se nepodařilo inicializovat.");
					else _backend = AcquireBackend();
				}
				catch (Exception ex) when (ex is DllNotFoundException or EntryPointNotFoundException or BadImageFormatException)
				{
					Logger.LogError("Čtečka obrazovky", ex.ToString());
				}
			}
		}

		/// <summary>
		/// Releases the library.
		/// </summary>
		public static void Unload()
		{
			lock (_lock)
			{
				ReleaseBackend();
				if (_context != IntPtr.Zero)
					Native.prism_shutdown(_context);
				_context = IntPtr.Zero;
			}
		}

		/// <summary>
		/// Speaks the text and, if the screen reader supports it, sends it to a braille display too.
		/// </summary>
		public static bool Output(string text, bool interrupt = false)
			=> Run(backend => Native.prism_backend_output(backend, text, interrupt));

		/// <summary>
		/// Speaks the text.
		/// </summary>
		public static bool Speak(string text, bool interrupt = true)
			=> Run(backend => Native.prism_backend_speak(backend, text, interrupt));

		/// <summary>
		/// Interrupts the current speech.
		/// </summary>
		public static bool Silence()
			=> Run(Native.prism_backend_stop);

		/// <summary>
		/// Indicates if the screen reader is speaking. It's false when the screen reader can't tell.
		/// </summary>
		public static bool IsSpeaking
		{
			get
			{
				lock (_lock)
				{
					IntPtr backend = GetBackend();
					return backend != IntPtr.Zero
						&& Native.prism_backend_is_speaking(backend, out bool speaking) == ErrorNone
						&& speaking;
				}
			}
		}

		private static bool Run(Func<IntPtr, int> action)
		{
			lock (_lock)
			{
				IntPtr backend = GetBackend();
				if (backend == IntPtr.Zero)
					return false;

				int error = action(backend);
				if (error == ErrorNotInitialized)
				{
					Native.prism_backend_initialize(backend);
					error = action(backend);
				}

				if (error != ErrorNone)
					ReleaseBackend();
				return error == ErrorNone;
			}
		}

		/// <summary>
		/// A screen reader can be started or closed while the game is running, so the backend is looked up
		/// again when the previous one is gone.
		/// </summary>
		private static IntPtr GetBackend()
		{
			if (_backend != IntPtr.Zero)
				return _backend;

			double elapsed = (Stopwatch.GetTimestamp() - _lastAttempt) / (double)Stopwatch.Frequency;
			if (_lastAttempt != 0 && elapsed < RetrySeconds)
				return IntPtr.Zero;

			if (_context == IntPtr.Zero)
			{
				Load();
				_lastAttempt = Stopwatch.GetTimestamp();
				return _backend;
			}

			_backend = AcquireBackend();
			return _backend;
		}

		private static void ReleaseBackend()
		{
			if (_backend != IntPtr.Zero)
				Native.prism_backend_free(_backend);
			_backend = IntPtr.Zero;
		}

		private static IntPtr AcquireBackend()
		{
			_lastAttempt = Stopwatch.GetTimestamp();

			List<(ulong id, int priority)> backends = new();
			ulong count = Native.prism_registry_count(_context).ToUInt64();
			for (ulong i = 0; i < count; i++)
			{
				ulong id = Native.prism_registry_id_at(_context, new UIntPtr(i));
				if (!_synthesizers.Contains(id))
					backends.Add((id, Native.prism_registry_priority(_context, id)));
			}
			backends.Sort((a, b) => b.priority.CompareTo(a.priority));

			foreach ((ulong id, _) in backends)
			{
				IntPtr backend = Native.prism_registry_acquire(_context, id);
				if (backend == IntPtr.Zero)
					continue;

				if ((Native.prism_backend_get_features(backend) & FeatureIsSupportedAtRuntime) != 0)
				{
					int error = Native.prism_backend_initialize(backend);
					if (error == ErrorNone || error == ErrorAlreadyInitialized)
						return backend;
				}
				Native.prism_backend_free(backend);
			}
			return IntPtr.Zero;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct NativeConfig
		{
			public byte Version;
			public IntPtr Registry;
			public IntPtr AvailabilityCallback;
			public IntPtr AvailabilityUserdata;
			public uint AvailabilityPollIntervalMs;
			public uint AvailabilityDebounceSamples;
			public uint AvailabilityBackoffMaxMs;
			public byte AvailabilityAutoPowerManage;
		}

		private static class Native
		{
			private const string Library = "prism";

			[DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
			public static extern NativeConfig prism_config_init();

			[DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
			public static extern IntPtr prism_init(ref NativeConfig config);

			[DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
			public static extern void prism_shutdown(IntPtr context);

			[DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
			public static extern UIntPtr prism_registry_count(IntPtr context);

			[DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
			public static extern ulong prism_registry_id_at(IntPtr context, UIntPtr index);

			[DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
			public static extern int prism_registry_priority(IntPtr context, ulong id);

			[DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
			public static extern IntPtr prism_registry_acquire(IntPtr context, ulong id);

			[DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
			public static extern void prism_backend_free(IntPtr backend);

			[DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
			public static extern ulong prism_backend_get_features(IntPtr backend);

			[DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
			public static extern int prism_backend_initialize(IntPtr backend);

			[DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
			public static extern int prism_backend_speak(
				IntPtr backend,
				[MarshalAs(UnmanagedType.LPUTF8Str)] string text,
				[MarshalAs(UnmanagedType.I1)] bool interrupt);

			[DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
			public static extern int prism_backend_output(
				IntPtr backend,
				[MarshalAs(UnmanagedType.LPUTF8Str)] string text,
				[MarshalAs(UnmanagedType.I1)] bool interrupt);

			[DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
			public static extern int prism_backend_stop(IntPtr backend);

			[DllImport(Library, CallingConvention = CallingConvention.Cdecl)]
			public static extern int prism_backend_is_speaking(
				IntPtr backend,
				[MarshalAs(UnmanagedType.I1)] out bool speaking);
		}
	}
}
