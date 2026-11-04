using SteamAudio;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Vector3 = UnityEngine.Vector3;

namespace Game.Audio
{
	/// <summary>
	/// Steam Audio settings shared by all 3D sources: HRTF, occlusion, transmission and reflections simulated from the geometry of the map.
	/// </summary>
	public static class SpatialAudio
	{
		/// <summary>
		/// Sets up a source. Its reflections start frozen.
		/// </summary>
		public static void Configure(SteamAudioSource steam)
		{
			steam.directBinaural = true;
			steam.interpolation = HRTFInterpolation.Bilinear;
			steam.distanceAttenuation = true;
			steam.distanceAttenuationInput = DistanceAttenuationInput.PhysicsBased;
			steam.airAbsorption = true;
			steam.directivity = false;
			steam.occlusion = true;
			steam.occlusionInput = OcclusionInput.SimulationDefined;
			steam.occlusionType = OcclusionType.Raycast;
			steam.transmission = true;
			steam.transmissionType = TransmissionType.FrequencyDependent;
			steam.transmissionInput = TransmissionInput.SimulationDefined;
			steam.reflections = true;
			steam.applyHRTFToReflections = true;
			steam.useDistanceCurveForReflections = false;
			steam.pathing = false;
			steam.applyHRTFToPathing = false;
			steam.findAlternatePaths = false;
			Freeze(steam);
		}

		/// <summary>
		/// Keeps the reflections in step with the source: none for 2D sounds, and distance attenuation from the game's rolloff curve
		/// where the game shapes it (portals, ambient loops, beacons), physically based otherwise.
		/// </summary>
		public static void Sync(AudioSource source, SteamAudioSource steam)
		{
			bool reflections = source.spatialize && !_placedByGame.Contains(steam);
			if (steam.reflections != reflections)
				steam.reflections = reflections;

			bool physical = source.rolloffMode == AudioRolloffMode.Logarithmic && source.minDistance <= 1;
			DistanceAttenuationInput input = physical ? DistanceAttenuationInput.PhysicsBased : DistanceAttenuationInput.CurveDriven;
			if (steam.distanceAttenuationInput != input)
				steam.distanceAttenuationInput = input;
		}

		/// <summary>
		/// Raised when reflections of a source were simulated and frozen, before the next simulation reads the inputs.
		/// </summary>
		public static event Action<SteamAudioSource> Simulated;

		/// <summary>
		/// Sources being simulated with the number of simulations started before; a later simulation includes their current position.
		/// </summary>
		private static readonly Dictionary<SteamAudioSource, int> _simulating = new();

		static SpatialAudio() => SteamAudioManager.SimulationApplied += OnSimulationApplied;

		/// <summary>
		/// Lets Steam Audio simulate reflections of the source once more at its current position and freezes them as soon as they arrive.
		/// </summary>
		public static void Simulate(SteamAudioSource steam)
		{
			Unfreeze(steam);
			_simulating[steam] = SteamAudioManager.SimulationsStarted;
		}

		public static bool IsSimulating(SteamAudioSource steam)
			=> _simulating.ContainsKey(steam);

		/// <summary>
		/// Raised when doors opened or closed; frozen reflections of sources that will still play are out of date.
		/// </summary>
		public static event Action GeometryChanged;

		/// <summary>
		/// Lets sources being simulated wait for a simulation of the new geometry and asks the owners of frozen sources to simulate them again.
		/// </summary>
		public static void NotifyGeometryChanged()
		{
			RestartSimulations();
			GeometryChanged?.Invoke();
		}

		/// <summary>
		/// Lets sources being simulated wait for a simulation that starts from now on, e.g. after the listener jumped elsewhere.
		/// </summary>
		public static void RestartSimulations()
		{
			foreach (SteamAudioSource steam in _simulating.Keys.ToList())
				_simulating[steam] = SteamAudioManager.SimulationsStarted;
		}

		/// <summary>
		/// Stops waiting for the reflections of a source that is no longer used.
		/// </summary>
		public static void Forget(SteamAudioSource steam)
		{
			_simulating.Remove(steam);
			if (_placedByGame.Remove(steam))
				Configure(steam);
			Freeze(steam);
		}

		/// <summary>
		/// Sources the game places and muffles itself, such as portals at doors. Steam Audio only gives them direction,
		/// otherwise a portal sitting in a closed door or above the player would be muffled twice.
		/// </summary>
		private static readonly HashSet<SteamAudioSource> _placedByGame = new();

		public static void LeaveAcousticsToGame(SteamAudioSource steam)
		{
			_placedByGame.Add(steam);
			_simulating.Remove(steam);
			steam.occlusion = false;
			steam.transmission = false;
			steam.reflections = false;
			Freeze(steam);
		}

		public static bool IsPlacedByGame(SteamAudioSource steam)
			=> _placedByGame.Contains(steam);

		public static void UseGeometricAcoustics(SteamAudioSource steam)
		{
			if (_placedByGame.Remove(steam))
				Configure(steam);
		}

		private static void OnSimulationApplied()
		{
			List<SteamAudioSource> done = _simulating
				.Where(s => s.Key == null || SteamAudioManager.SimulationsApplied > s.Value)
				.Select(s => s.Key)
				.ToList();

			foreach (SteamAudioSource steam in done)
			{
				_simulating.Remove(steam);
				if (steam == null)
					continue;

				Freeze(steam);
				Simulated?.Invoke(steam);
			}
		}

		/// <summary>
		/// Lets Steam Audio simulate reflections of the source at its current position.
		/// </summary>
		public static void Unfreeze(SteamAudioSource steam)
			=> steam.reflectionsType = ReflectionsType.Realtime;

		/// <summary>
		/// Keeps the last simulated reflections. A new impulse response in the middle of a sound makes its reverb flicker and crackle.
		/// </summary>
		/// <remarks>A baked static source without baked data is left out of the simulation but keeps applying its last impulse response.</remarks>
		public static void Freeze(SteamAudioSource steam)
			=> steam.reflectionsType = ReflectionsType.BakedStaticSource;

		/// <summary>
		/// Layer of the boxes of the acoustic geometry; their colliders let the game find walls and doors between two points.
		/// </summary>
		public const int GeometryLayer = 2;

		/// <summary>
		/// Distance kept between a source and the geometry in front of it, in meters.
		/// </summary>
		private const float _geometryClearance = .05f;

		/// <summary>
		/// Moves a point that lies inside or behind a wall or a closed door, as seen from <paramref name="from"/>, just in front of it.
		/// A source inside a box is occluded and gets the reflections of the inside of the box.
		/// </summary>
		public static Vector3 InFrontOfGeometry(Vector3 from, Vector3 position)
		{
			Vector3 direction = position - from;
			float distance = direction.magnitude;
			if (distance <= 0 || !Physics.Raycast(from, direction / distance, out RaycastHit hit, distance, 1 << GeometryLayer, QueryTriggerInteraction.Ignore))
				return position;

			return from + direction / distance * Mathf.Max(0, hit.distance - _geometryClearance);
		}

		/// <summary>
		/// The thickest box a source can end up in, in meters.
		/// </summary>
		private const float _maxGeometryDepth = .5f;

		/// <summary>
		/// Moves a point that lies inside a wall, a closed door or an obstacle toward the listener, just out of it.
		/// Unlike <see cref="InFrontOfGeometry"/> it leaves a point behind a wall where it is, e.g. a door in another room.
		/// </summary>
		public static Vector3 OutOfGeometry(Vector3 listener, Vector3 position)
		{
			if (!Physics.CheckSphere(position, .01f, 1 << GeometryLayer, QueryTriggerInteraction.Ignore))
				return position;

			Vector3 toListener = listener - position;
			float distance = toListener.magnitude;
			if (distance <= 0)
				return position;

			Vector3 outside = position + toListener / distance * Mathf.Min(distance, _maxGeometryDepth);
			return InFrontOfGeometry(outside, position);
		}

		/// <summary>
		/// Steam Audio clamps the distance attenuation of the direct sound at 1 m, so a source 0.4 m away is as loud as at 1 m
		/// while its reflections are not. Scaling the reflections by the distance restores the real ratio of direct and reflected sound.
		/// </summary>
		public static float GetReflectionsMixLevel(Vector3 position)
		{
			if (Camera.main == null)
				return 1;

			return GetReflectionsMixLevel(Vector3.Distance(Camera.main.transform.position, position));
		}

		/// <param name="distance">Distance of the source from the listener in meters</param>
		public static float GetReflectionsMixLevel(float distance)
			=> Mathf.Clamp(distance, .2f, 1);

		/// <summary>
		/// Copies of clips extended with silence, indexed by the original clips.
		/// </summary>
		private static readonly Dictionary<AudioClip, AudioClip> _clipsWithTail = new();

		/// <summary>
		/// Returns a copy of the clip extended with silence as long as the simulated reflections.
		/// </summary>
		/// <remarks>Unity stops the spatializer as soon as the clip ends, which would cut off the reflections.</remarks>
		public static AudioClip AddReflectionTail(AudioClip clip)
		{
			if (clip == null)
				return null;
			if (_clipsWithTail.TryGetValue(clip, out AudioClip extended))
				return extended;

			if (clip.loadType != AudioClipLoadType.DecompressOnLoad)
			{
				// Samples of streamed and compressed clips cannot be read.
				_clipsWithTail[clip] = clip;
				return clip;
			}

			if (clip.loadState != AudioDataLoadState.Loaded)
				clip.LoadAudioData();

			float[] samples = new float[clip.samples * clip.channels];
			if (!clip.GetData(samples, 0))
				return clip;

			int tailSamples = Mathf.CeilToInt(SteamAudioSettings.Singleton.realTimeDuration * clip.frequency);
			extended = AudioClip.Create(clip.name, clip.samples + tailSamples, clip.channels, clip.frequency, false);
			extended.SetData(samples, 0);
			_clipsWithTail[clip] = extended;
			return extended;
		}
	}
}
