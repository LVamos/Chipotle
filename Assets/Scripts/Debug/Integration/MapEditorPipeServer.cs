using Game.UI;

using System;
using System.Collections.Concurrent;
using System.Globalization;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading;

using UnityEngine;

namespace Game.Debug.integration
{
	public class MapEditorPipeServer : MonoBehaviour, IDisposable
	{

		private const string FromEditorPipeName = "EditorToChipotle";
		private const string ToEditorPipeName = "ChipotleToEditor";

		private NamedPipeServerStream _fromEditorPipe;
		private NamedPipeServerStream _toEditorPipe;

		private StreamReader _reader;
		private StreamWriter _writer;

		private Thread _listenThread;
		private volatile bool _isRunning;

		private readonly ConcurrentQueue<Action> _mainThreadActions = new();

		private readonly object _writeLock = new();

		public void StartServer()
		{
			if (_isRunning)
				return;

			_isRunning = true;

			// Start server thread
			_listenThread = new Thread(ServerLoop)
			{
				IsBackground = true,
				Name = "MapEditorPipeServer"
			};
			_listenThread.Start();
		}

		public void StopServer()
		{
			_isRunning = false;

			try
			{
				_reader?.Dispose();
				_writer?.Dispose();
				_fromEditorPipe?.Dispose();
				_toEditorPipe?.Dispose();
			}
			catch { }

			_reader = null;
			_writer = null;
			_fromEditorPipe = null;
			_toEditorPipe = null;

			if (_listenThread != null && _listenThread.IsAlive) _listenThread.Join(500);
			_listenThread = null;
		}

		private void OnDestroy() => StopServer();

		private void Update()
		{
			while (_mainThreadActions.TryDequeue(out var action))
			{
				try { action(); }
				catch { }
			}
		}

		private void ServerLoop()
		{
			try
			{
				_fromEditorPipe = new NamedPipeServerStream(
					FromEditorPipeName, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

				_toEditorPipe = new NamedPipeServerStream(
					ToEditorPipeName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

				_fromEditorPipe.WaitForConnection();
				_toEditorPipe.WaitForConnection();

				_reader = new StreamReader(_fromEditorPipe, new UTF8Encoding(false));
				_writer = new StreamWriter(_toEditorPipe, new UTF8Encoding(false)) { AutoFlush = true };

				while (_isRunning && _fromEditorPipe.IsConnected && _toEditorPipe.IsConnected)
				{
					string line = _reader.ReadLine();
					if (line != null) ProcessLine(line);
					else Thread.Sleep(20);
				}
			}
			catch (Exception)
			{
				// Swallow exceptions to keep server alive
			}
		}

		private void ProcessLine(string line)
		{
			if (string.IsNullOrWhiteSpace(line)) return;

			string trimmed = line.Trim();
			Vector2 coords = default;
			if (TryParseJumpToCoordinates(trimmed, out coords))
				EnqueueMainThread(() => InvokeGoToCoords(coords));
			else if (TryParseMoveTuttleToCoordinates(trimmed, out coords))
				EnqueueMainThread(() => InvokeMoveTuttleToCoords(coords));

		}

		private void InvokeMoveTuttleToCoords(Vector2 coordinates)
		{
			try
			{
				WindowHandler.DebugManager.MoveTuttleToCoords(coordinates);
				WindowHandler.FocusGameWindow();
			}
			catch { }
		}

		private void EnqueueMainThread(Action action) => _mainThreadActions.Enqueue(action);

		private bool TryParseJumpToCoordinates(string message, out Vector2 coordinates)
		{
			coordinates = default;

			if (!message.StartsWith("JumpToCoordinates", StringComparison.OrdinalIgnoreCase))
				return false;

			string[] parts = message.Split(';');
			if (parts.Length < 3) return false;

			if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float x)) return false;
			if (!float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float y)) return false;

			coordinates = new Vector2(x, y);
			return true;
		}

		private bool TryParseMoveTuttleToCoordinates(string message, out Vector2 coordinates)
		{
			coordinates = default;

			if (!message.StartsWith("MoveTuttleToCoords", StringComparison.OrdinalIgnoreCase))
				return false;

			string[] parts = message.Split(';');
			if (parts.Length < 3) return false;

			if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float x)) return false;
			if (!float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float y)) return false;

			coordinates = new Vector2(x, y);
			return true;
		}

		private void InvokeGoToCoords(Vector2 coordinates)
		{
			try
			{
				WindowHandler.DebugManager.GoToCoords(coordinates);
				WindowHandler.FocusGameWindow();
			}
			catch { }
		}

		public void SendJumpToCoordinates(Vector2 coordinates)
		{
			string message = $"JumpToCoordinates;{coordinates.x.ToString(CultureInfo.InvariantCulture)};{coordinates.y.ToString(CultureInfo.InvariantCulture)}";
			SendCommand(message);
		}

		private void SendCommand(string command)
		{
			if (!_isRunning || _writer == null) return;

			lock (_writeLock)
			{
				try
				{
					_writer.WriteLine(command);
					_writer.Flush();
				}
				catch
				{
					// Swallow errors, příště se pokusíme znovu
				}
			}
		}

		public void Dispose() => StopServer();
	}
}
