using SteamAudio;

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;

using UnityEngine;

using Vector3 = UnityEngine.Vector3;

namespace Game.Audio
{
	/// <summary>
	/// Simulates reflections of the footsteps apart from the other sources, with the listener where the ears will be when the step sounds.
	/// Steam Audio simulates all sources with the listener where the camera is now, but the camera glides behind the player,
	/// and with a step close to the ears the reflections depend strongly on the distance between them.
	/// </summary>
	/// <remarks>
	/// The sources are moved from the simulator of <see cref="SteamAudioManager"/> to an own one, which also simulates their direct sound.
	/// Reflections are simulated on an own thread, one source at a time; a source is left out of the simulation in between, which freezes its reflections.
	/// </remarks>
	public static class StepReflections
	{
		/// <summary>
		/// Raised on the main thread when reflections of a source were simulated for the listener it was asked for.
		/// </summary>
		public static event Action<SteamAudioSource> Simulated;

		private static Simulator _simulator;
		private static Simulator _managerSimulator;
		private static Thread _thread;
		private static readonly AutoResetEvent _start = new(false);
		private static readonly ManualResetEvent _idle = new(true);
		private static volatile bool _stop;

		/// <summary>
		/// Sources simulated here with whether they were already moved from the simulator of <see cref="SteamAudioManager"/>.
		/// </summary>
		private static readonly Dictionary<SteamAudioSource, bool> _sources = new();

		private static readonly List<Request> _requests = new();
		private static Request _running;
		private static bool _commitRequired;
		private static bool _nudged;
		private static FieldInfo _sceneCommitRequired;

		private class Request
		{
			public SteamAudioSource Source;
			public Vector3 Listener;
			public Quaternion Rotation;
		}

		/// <summary>
		/// Takes the source over from Steam Audio. Call it after every new <see cref="SteamAudioSource"/>.
		/// </summary>
		public static void Adopt(SteamAudioSource steam)
		{
			if (steam == null || _sources.ContainsKey(steam))
				return;

			steam.pathing = false;
			SpatialAudio.Freeze(steam);
			_sources[steam] = false;
		}

		/// <summary>
		/// Gives up the source before it is destroyed.
		/// </summary>
		public static void Release(SteamAudioSource steam)
		{
			if (steam == null || !_sources.TryGetValue(steam, out bool moved))
				return;

			_requests.RemoveAll(r => r.Source == steam);
			if (_running != null && _running.Source == steam)
			{
				_idle.WaitOne();
				_running = null;
			}

			_sources.Remove(steam);
			if (moved && _simulator != null)
			{
				steam.GetSource().RemoveFromSimulator(_simulator);
				_commitRequired = true;
			}
		}

		/// <summary>
		/// Simulates reflections of the source at its current position for a listener at the given place.
		/// </summary>
		public static void Simulate(SteamAudioSource steam, Vector3 listener, Quaternion rotation)
		{
			if (steam == null || !_sources.ContainsKey(steam))
				return;

			_requests.RemoveAll(r => r.Source == steam);
			_requests.Add(new() { Source = steam, Listener = listener, Rotation = rotation });
		}

		public static bool IsSimulating(SteamAudioSource steam)
			=> (_running != null && _running.Source == steam) || _requests.Exists(r => r.Source == steam);

		/// <summary>
		/// Runs once a frame before <see cref="SteamAudioManager"/> commits the scene: simulates the direct sound, takes finished reflections
		/// and starts the next simulation.
		/// </summary>
		public static void Tick(Transform camera)
		{
			if (camera == null || !EnsureSimulator())
				return;

			MoveSources();
			SimulateDirect(camera);

			bool sceneChanging = SceneCommitPending();
			if (!_idle.WaitOne(0))
			{
				// A scene must not change while a simulation traces rays in it.
				if (!sceneChanging)
				{
					HoldManager(true);
					return;
				}
				_idle.WaitOne();
			}

			FinishSimulation();
			if (sceneChanging)
			{
				HoldManager(false);
				return;
			}

			if (_commitRequired)
			{
				_simulator.SetScene(SteamAudioManager.CurrentScene);
				_simulator.Commit();
				_commitRequired = false;
			}

			_requests.RemoveAll(r => r.Source == null || !_sources.TryGetValue(r.Source, out bool moved) || !moved);
			bool waiting = _requests.Count > 0;
			if (waiting && !ManagerSimulating())
				StartSimulation();
			HoldManager(waiting);
		}

		/// <summary>
		/// Whether the simulation thread of <see cref="SteamAudioManager"/> traces rays now.
		/// </summary>
		private static bool ManagerSimulating()
		{
			SteamAudioManager manager = SteamAudioManager.Singleton;
			if (manager == null)
				return false;

			_managerThread ??= typeof(SteamAudioManager).GetField("mSimulationThread", BindingFlags.NonPublic | BindingFlags.Instance);
			return _managerThread?.GetValue(manager) is Thread thread && thread.ThreadState != ThreadState.WaitSleepJoin;
		}

		/// <summary>
		/// Keeps <see cref="SteamAudioManager"/> from starting a simulation while the steps simulate or wait for it.
		/// Each simulator takes the share of CPU cores set in Steam Audio; two at once leave the audio thread too little time and the sound breaks up.
		/// </summary>
		private static void HoldManager(bool hold)
		{
			SteamAudioManager manager = SteamAudioManager.Singleton;
			if (manager == null || (!hold && !_holding))
				return;

			_managerElapsed ??= typeof(SteamAudioManager).GetField("mSimulationUpdateTimeElapsed", BindingFlags.NonPublic | BindingFlags.Instance);
			_managerElapsed?.SetValue(manager, hold ? float.NegativeInfinity : SteamAudioSettings.Singleton.simulationUpdateInterval);
			_holding = hold;
		}

		private static FieldInfo _managerThread, _managerElapsed;
		private static bool _holding;

		private static bool EnsureSimulator()
		{
			if (SteamAudioManager.Simulator == null || SteamAudioManager.CurrentScene == null)
				return false;
			if (_simulator != null)
				return true;

			_simulator = new Simulator(SteamAudioManager.Context, SteamAudioManager.GetSimulationSettings(false));
			_managerSimulator = SteamAudioManager.Simulator;
			_stop = false;
			_idle.Set();
			_thread = new Thread(RunSimulations) { Name = "Step reflections", IsBackground = true };
			_thread.Start();
			return true;
		}

		private static void MoveSources()
		{
			foreach (SteamAudioSource steam in new List<SteamAudioSource>(_sources.Keys))
			{
				if (steam == null)
				{
					_sources.Remove(steam);
					continue;
				}
				if (_sources[steam])
					continue;

				SteamAudioManager.RemoveSource(steam);
				steam.GetSource().RemoveFromSimulator(_managerSimulator);
				steam.GetSource().AddToSimulator(_simulator);
				_sources[steam] = true;
				_commitRequired = true;
			}
		}

		private static void SimulateDirect(Transform camera)
		{
			_simulator.SetSharedInputs(SimulationFlags.Direct, SharedInputs(camera.position, camera.rotation));
			foreach (var (steam, moved) in _sources)
			{
				if (moved && steam != null)
					steam.SetInputs(SimulationFlags.Direct);
			}

			_simulator.RunDirect();

			foreach (var (steam, moved) in _sources)
			{
				if (moved && steam != null)
					steam.UpdateOutputs(SimulationFlags.Direct);
			}
		}

		private static void FinishSimulation()
		{
			if (_running == null)
				return;

			SteamAudioSource steam = _running.Source;
			_running = null;
			if (steam == null)
				return;

			SpatialAudio.Freeze(steam);
			steam.SetInputs(SimulationFlags.Reflections);
			if (!_requests.Exists(r => r.Source == steam))
				Simulated?.Invoke(steam);
		}

		private static void StartSimulation()
		{
			_running = _requests[0];
			_requests.RemoveAt(0);

			_simulator.SetScene(SteamAudioManager.CurrentScene);
			_simulator.Commit();
			_commitRequired = false;

			// A listener that moved is a new simulation; Steam Audio would average it with the previous one if nothing moved.
			_nudged = !_nudged;
			Vector3 listener = _running.Listener + (_nudged ? .001f : 0) * Vector3.up;
			_simulator.SetSharedInputs(SimulationFlags.Reflections, SharedInputs(listener, _running.Rotation));

			foreach (var (steam, moved) in _sources)
			{
				if (!moved || steam == null)
					continue;

				if (steam == _running.Source)
					SpatialAudio.Unfreeze(steam);
				else SpatialAudio.Freeze(steam);
				steam.SetInputs(SimulationFlags.Reflections);
			}

			_idle.Reset();
			_start.Set();
		}

		private static SimulationSharedInputs SharedInputs(Vector3 listener, Quaternion rotation)
		{
			SteamAudioSettings settings = SteamAudioSettings.Singleton;
			var inputs = new SimulationSharedInputs { };
			inputs.listener.origin = Common.ConvertVector(listener);
			inputs.listener.ahead = Common.ConvertVector(rotation * Vector3.forward);
			inputs.listener.up = Common.ConvertVector(rotation * Vector3.up);
			inputs.listener.right = Common.ConvertVector(rotation * Vector3.right);
			inputs.numRays = settings.realTimeRays;
			inputs.numBounces = settings.realTimeBounces;
			inputs.duration = settings.realTimeDuration;
			inputs.order = settings.realTimeAmbisonicOrder;
			inputs.irradianceMinDistance = settings.realTimeIrradianceMinDistance;
			inputs.pathingVisualizationCallback = null;
			inputs.pathingUserData = IntPtr.Zero;
			return inputs;
		}

		/// <summary>
		/// Whether <see cref="SteamAudioManager"/> will change the scene in this frame, e.g. after a door opened.
		/// </summary>
		private static bool SceneCommitPending()
		{
			SteamAudioManager manager = SteamAudioManager.Singleton;
			if (manager == null)
				return false;

			_sceneCommitRequired ??= typeof(SteamAudioManager).GetField("mSceneCommitRequired", BindingFlags.NonPublic | BindingFlags.Instance);
			return _sceneCommitRequired != null && (bool)_sceneCommitRequired.GetValue(manager);
		}

		private static void RunSimulations()
		{
			while (true)
			{
				_start.WaitOne();
				if (_stop)
					break;

				try
				{
					_simulator.RunReflections();
				}
				catch (Exception e)
				{
					UnityEngine.Debug.LogException(e);
				}
				finally
				{
					_idle.Set();
				}
			}
			_idle.Set();
		}

		/// <summary>
		/// Stops the simulation before Steam Audio frees its context.
		/// </summary>
		public static void Shutdown()
		{
			if (_thread != null)
			{
				_stop = true;
				_start.Set();
				_thread.Join();
				_thread = null;
			}

			foreach (var (steam, moved) in _sources)
			{
				if (moved && steam != null && _simulator != null)
					steam.GetSource().RemoveFromSimulator(_simulator);
			}

			_simulator?.Release();
			_simulator = null;
			_managerSimulator = null;
			_sources.Clear();
			_requests.Clear();
			_running = null;
			_commitRequired = false;
			HoldManager(false);
		}
	}
}
