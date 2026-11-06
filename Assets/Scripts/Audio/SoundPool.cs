
using Game;
using Game.Audio;

using SteamAudio;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEditor;

using UnityEngine;

using Vector3 = UnityEngine.Vector3;

namespace Assets.Scripts.Audio
{
	public class SoundPool : MonoBehaviour
	{
		public void SoundStartedPlaying(AudioSource source)
		{
			if (source == null)
				throw new ArgumentNullException(nameof(source));

			if (!source.isActiveAndEnabled)
				source.gameObject.SetActive(true);

			_pool.Remove(source);
			_playingSources.Add(source);
		}

		public AudioLowPassFilter GetLowPass(AudioSource source)
		{
			return _lowPasses[source];
		}

		public int GetGeneration(AudioSource source)
		{
			return _generations.TryGetValue(source, out int generation) ? generation : -1;
		}

		private Dictionary<AudioSource, AudioLowPassFilter> _lowPasses = new();
		private Dictionary<AudioSource, int> _generations = new();

		private void LogPlayingSounds()
		{
			if (!Settings.LogPlayingSounds)
				return;

			(AudioSource[] sounds, string[] names) sounds = GetPlayingSounds();
			string[] soundDescriptions = sounds.sounds.Select(s => s.name).ToArray();
			StringBuilder builder = new("Playing sounds");
			builder.AppendLine();
			for (int i = 0; i < sounds.names.Length; i++)
			{
				builder.AppendLine($"{sounds.names[i]}; {soundDescriptions[i]}");
			}

			string output = builder.ToString();
			System.Diagnostics.Debug.WriteLine(output);
		}

		/// <summary>
		/// Returns currently playing sounds.
		/// </summary>
		/// <returns>(AudioSource[] sounds, string[] names)</returns>
		public (AudioSource[] sounds, string[] names) GetPlayingSounds()
		{
			AudioSource[] playingSounds = _playingSources.ToArray();
			string[] playingSoundNames = playingSounds.Select(s => s.clip.name).ToArray();
			return (playingSounds, playingSoundNames);
		}

		public void EnableLowPass(AudioSource source)
		{
			AudioLowPassFilter lowPass = GetLowPass(source);
			lowPass.enabled = true;
			source.spatializePostEffects = true;
		}

		public void DisableLowPass(AudioSource source)
		{
			AudioLowPassFilter lowPass = GetLowPass(source);
			if (!lowPass.enabled)
				return;

			lowPass.enabled = false;
		}

		private HashSet<AudioSource> _playingSources = new(_poolSize);
		public AudioSource GetSource()
		{
			AudioSource source = _pool.FirstOrDefault();
			if (source == null)
				source = AddSource();

			// Init settings
			source.panStereo = 0;
			source.pitch = 1;
			source.rolloffMode = AudioRolloffMode.Logarithmic;
			source.minDistance = 1;
			source.maxDistance = 500;
			_steamSources[source].reflectionsMixLevel = 1;
			_transmissionFilters[source].enabled = false;
			_steamSources[source].transmissionInput = TransmissionInput.SimulationDefined;

			return source;
		}

		public SteamAudioSource GetSteamAudio(AudioSource source) => _steamSources[source];

		public TransmissionFilter GetTransmissionFilter(AudioSource source) => _transmissionFilters[source];

		private readonly Dictionary<AudioSource, TransmissionFilter> _transmissionFilters = new();

		private DoorTransmission _doorTransmission;

		public bool Owns(AudioSource source)
			=> _steamSources.ContainsKey(source);

		/// <summary>
		/// Lets Steam Audio simulate reflections of the source once more at its current position.
		/// </summary>
		public void Simulate(AudioSource source)
		{
			SpatialAudio.Simulate(_steamSources[source]);
			_positionsAtSimulation[source] = (source.transform.position, ListenerPosition ?? source.transform.position);
		}

		/// <summary>
		/// Starts a sound after its reflections were simulated where it plays, otherwise its early reflections would come from where the source was before.
		/// </summary>
		public void PlayWhenSimulated(AudioSource source, Action play)
		{
			Simulate(source);
			_pendingPlays[source] = (play, Time.realtimeSinceStartup);
		}

		/// <summary>
		/// Sounds waiting for the simulation of their reflections with the time they were requested.
		/// </summary>
		private readonly Dictionary<AudioSource, (Action Play, float Requested)> _pendingPlays = new();

		/// <summary>
		/// Positions of each source and of the listener at the last simulation of its reflections.
		/// </summary>
		private readonly Dictionary<AudioSource, (Vector3 Source, Vector3 Listener)> _positionsAtSimulation = new();

		private readonly Dictionary<AudioSource, SteamAudioSource> _steamSources = new();

		/// <summary>
		/// Longest wait for a simulation in seconds; a sound plays even if Steam Audio does not deliver.
		/// </summary>
		/// <remarks>A sound started earlier gets its reverb only in the middle. In a closed room a simulation takes up to about 0.1 s
		/// and a sound waits for two or three of them.</remarks>
		private const float _maxSimulationWait = 1;

		/// <summary>
		/// How far the listener or a playing loop may move before the reflections of the loop are simulated again, in meters.
		/// </summary>
		private const float _loopRefreshDistance = 1.5f;

		/// <summary>
		/// How far the listener may move within one frame, in meters; a longer move is a teleport.
		/// </summary>
		private const float _listenerJump = 2;

		private Vector3? _lastListenerPosition;

		/// <summary>
		/// Loops whose reverb was simulated for the listener before a teleport; they stay dry until they are simulated again.
		/// </summary>
		private readonly HashSet<AudioSource> _staleLoops = new();

		private static Vector3? ListenerPosition => Camera.main != null ? Camera.main.transform.position : null;

		private void Awake()
		{
			SpatialAudio.Simulated += OnSimulated;
			SpatialAudio.GeometryChanged += OnGeometryChanged;
			_doorTransmission = new(this);
		}

		private void OnDestroy()
		{
			SpatialAudio.Simulated -= OnSimulated;
			SpatialAudio.GeometryChanged -= OnGeometryChanged;
			_doorTransmission.Dispose();
		}

		/// <summary>
		/// Makes the playing loops simulate their reflections again in the next update.
		/// </summary>
		private void OnGeometryChanged()
			=> _positionsAtSimulation.Clear();

		private void OnSimulated(SteamAudioSource steam)
		{
			AudioSource source = _steamSources.FirstOrDefault(s => s.Value == steam).Key;
			if (source == null)
				return;

			if (_staleLoops.Remove(source))
				steam.reflectionsMixLevel = SpatialAudio.GetReflectionsMixLevel(source.transform.position);
			StartPendingPlay(source);
		}

		/// <summary>
		/// The frozen reverb of the playing loops belongs to the place the listener jumped from, e.g. a cellar heard from the street.
		/// The loops go dry and simulate again.
		/// </summary>
		private void OnListenerJumped()
		{
			SpatialAudio.RestartSimulations();

			foreach (AudioSource source in _playingSources.Where(s => s.loop && _steamSources[s].reflections))
			{
				_steamSources[source].reflectionsMixLevel = 0;
				_staleLoops.Add(source);
				_positionsAtSimulation.Remove(source);
			}
		}

		private void StartPendingPlay(AudioSource source)
		{
			if (!_pendingPlays.Remove(source, out var pending))
				return;

			pending.Play();
			LogSound(source, Time.realtimeSinceStartup - pending.Requested);
		}

		/// <summary>
		/// Writes a played 3D sound to the Unity log as one line starting with [SND].
		/// </summary>
		private static void LogSound(AudioSource source, float delay)
		{
			Vector3 position = source.transform.position;
			float distance = ListenerPosition is Vector3 listener ? Vector3.Distance(listener, position) : -1;
			Debug.LogFormat(LogType.Log, LogOption.NoStacktrace, null,
				"[SND] {0:0.00} s | {1} | pos {2:0.00}, {3:0.00}, {4:0.00} | distance {5:0.00} m | delay {6:0} ms",
				Time.time, source.clip != null ? source.clip.name : "none", position.x, position.y, position.z, distance, delay * 1000);
		}

		private HashSet<AudioSource> _pool = new(_poolSize);
		private const int _poolSize = 20;

		private void Start()
		{
			for (int i = 0; i < _poolSize; i++)
				AddSource();
		}

		private void Update()
		{
			Vector3? listener = ListenerPosition;
			if (listener != null && _lastListenerPosition != null && Vector3.Distance(listener.Value, _lastListenerPosition.Value) > _listenerJump)
				OnListenerJumped();
			_lastListenerPosition = listener;

			HashSet<AudioSource> sourcesToRemove = new();
			bool refreshLoop = _pendingPlays.Count == 0 && !_playingSources.Any(s => s.loop && SpatialAudio.IsSimulating(_steamSources[s]));
			foreach (AudioSource source in _playingSources)
			{
				StopForbiddenSound(source);
				if (_pendingPlays.TryGetValue(source, out var pending))
				{
					if (Time.realtimeSinceStartup - pending.Requested > _maxSimulationWait)
						StartPendingPlay(source);
					continue;
				}

				if (!source.isPlaying)
				{
					Sleep(source);
					sourcesToRemove.Add(source);
					_pool.Add(source);
					continue;
				}

				SpatialAudio.Sync(source, _steamSources[source]);
			}

			foreach (AudioSource source in sourcesToRemove)
			{
				_playingSources.Remove(source);
			}

			if (refreshLoop && listener != null)
				RefreshNearestLoop(listener.Value);

			_doorTransmission.Update(_playingSources);
			LogPlayingSounds();
		}

		/// <summary>
		/// Simulates reflections of the nearest playing loop that it or the listener moved away from where they were simulated.
		/// </summary>
		/// <remarks>The caller lets one loop at a time simulate and only while no sound waits for its reflections: each simulated source
		/// slows the whole simulation down, and a starting sound must not wait for loops.</remarks>
		private void RefreshNearestLoop(Vector3 listener)
		{
			AudioSource nearest = _playingSources
				.Where(s => IsOutOfDate(s, listener))
				.OrderBy(s => Vector3.Distance(s.transform.position, listener))
				.FirstOrDefault();
			if (nearest != null)
				Simulate(nearest);
		}

		private bool IsOutOfDate(AudioSource source, Vector3 listener)
		{
			SteamAudioSource steam = _steamSources[source];
			if (!source.loop || !source.isPlaying || !steam.reflections || SpatialAudio.IsSimulating(steam))
				return false;

			return !_positionsAtSimulation.TryGetValue(source, out var last)
				|| Vector3.Distance(last.Listener, listener) > _loopRefreshDistance
				|| Vector3.Distance(last.Source, source.transform.position) > _loopRefreshDistance;
		}

		private void StopForbiddenSound(AudioSource source)
		{
			if (source.clip == null)
				return;

			if (!string.IsNullOrEmpty(Settings.AllowedSound) && source.clip.name != Settings.AllowedSound)
				source.Stop();
		}

		private void Sleep(AudioSource source)
		{
			source.name = "inactive sound";
			source.transform.SetParent(null);
			SpatialAudio.Forget(_steamSources[source]);
			_positionsAtSimulation.Remove(source);
			_staleLoops.Remove(source);
			source.gameObject.SetActive(false);
			_generations[source]++;
		}


		private AudioSource AddSource()
		{
			GameObject o = new("Sound");
			AudioSource source = o.AddComponent<AudioSource>();
			source.playOnAwake = false;
			SteamAudioSource steam = o.AddComponent<SteamAudioSource>();
			SpatialAudio.Configure(steam);
			_steamSources[source] = steam;
			AudioLowPassFilter lowPass = o.AddComponent<AudioLowPassFilter>();
			o.SetActive(false);
			_pool.Add(source);
			_lowPasses[source] = lowPass;
			TransmissionFilter transmission = o.AddComponent<TransmissionFilter>();
			transmission.enabled = false;
			_transmissionFilters[source] = transmission;
			_generations[source] = 0;
			return source;
		}
	}
}
