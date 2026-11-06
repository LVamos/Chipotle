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
			steam.maxTransmissionSurfaces = 16;
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
			Nudge(steam);
			Unfreeze(steam);
			_simulating[steam] = SteamAudioManager.SimulationsStarted;
		}

		/// <summary>
		/// Sources lifted by <see cref="ResetDistance"/>.
		/// </summary>
		private static readonly HashSet<SteamAudioSource> _nudged = new();

		/// <summary>
		/// How far a source moves to make Steam Audio forget its reflections, in meters; Steam Audio averages them while the source and the listener stay within 0.1 mm.
		/// </summary>
		private const float ResetDistance = .001f;

		/// <summary>
		/// Moves the source by a millimeter up or back down, so a closed door does not keep the reverb averaged while it was open.
		/// </summary>
		private static void Nudge(SteamAudioSource steam)
		{
			bool down = _nudged.Remove(steam);
			if (!down)
				_nudged.Add(steam);
			steam.transform.position += (down ? -ResetDistance : ResetDistance) * Vector3.up;
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
			_nudged.Remove(steam);
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

		private static readonly Dictionary<AudioClip, AudioClip> _seamlessLoops = new();

		private const float LoopCrossfade = .02f;

		/// <summary>
		/// How far before the end a loop may be cut so that its tone continues in phase, in seconds; covers one period down to 40 Hz.
		/// </summary>
		private const float LoopCutSearch = .025f;

		/// <summary>
		/// Makes a loop continue in phase where it wraps. A hum whose phase jumps there cancels against its own reverb tail and flutters.
		/// </summary>
		public static AudioClip MakeSeamless(AudioClip clip)
		{
			if (clip == null)
				return null;
			if (_seamlessLoops.TryGetValue(clip, out AudioClip seamless))
				return seamless;

			_seamlessLoops[clip] = clip;
			if (clip.loadType != AudioClipLoadType.DecompressOnLoad)
				return clip;
			if (clip.loadState != AudioDataLoadState.Loaded)
				clip.LoadAudioData();

			int channels = clip.channels;
			float[] samples = new float[clip.samples * channels];
			int fade = Mathf.RoundToInt(LoopCrossfade * clip.frequency);
			int search = Mathf.RoundToInt(LoopCutSearch * clip.frequency);
			if (!clip.GetData(samples, 0) || clip.samples < (fade + search) * 4)
				return clip;

			(int length, float similarity) = FindLoopCut(samples, channels, fade, search);
			bool tonal = similarity >= .5f;
			if (!tonal)
			{
				if (!HasSeam(samples, channels))
					return clip;
				length = clip.samples - fade;
			}

			float[] result = new float[length * channels];
			Array.Copy(samples, result, result.Length);
			for (int i = 0; i < fade; i++)
			{
				float t = (i + .5f) / fade;
				float fadeIn = tonal ? t : Mathf.Sin(t * Mathf.PI / 2);
				float fadeOut = tonal ? 1 - t : Mathf.Cos(t * Mathf.PI / 2);
				for (int c = 0; c < channels; c++)
					result[i * channels + c] = samples[i * channels + c] * fadeIn + samples[(length + i) * channels + c] * fadeOut;
			}

			seamless = AudioClip.Create(clip.name, length, channels, clip.frequency, false);
			seamless.SetData(result, 0);
			_seamlessLoops[clip] = seamless;
			return seamless;
		}

		/// <summary>
		/// Finds the length near the end of a clip after which its beginning follows most smoothly, with the correlation of the two.
		/// </summary>
		private static (int Length, float Similarity) FindLoopCut(float[] samples, int channels, int window, int search)
		{
			int frames = samples.Length / channels;
			float[] mono = new float[frames];
			for (int i = 0; i < frames; i++)
				for (int c = 0; c < channels; c++)
					mono[i] += samples[i * channels + c];

			double startEnergy = 0;
			for (int i = 0; i < window; i++)
				startEnergy += mono[i] * mono[i];

			(int Length, float Similarity) best = (frames - window, -1);
			for (int length = frames - window - search; length <= frames - window; length++)
			{
				double product = 0, energy = 0;
				for (int i = 0; i < window; i++)
				{
					product += mono[i] * mono[length + i];
					energy += mono[length + i] * mono[length + i];
				}

				float similarity = (float)(product / Math.Sqrt(startEnergy * energy + 1e-20));
				if (similarity > best.Similarity)
					best = (length, similarity);
			}
			return best;
		}

		/// <summary>
		/// Whether the jump from the last sample to the first is bigger than 99 % of the steps inside the clip.
		/// </summary>
		private static bool HasSeam(float[] samples, int channels)
		{
			int frames = samples.Length / channels;
			float[] steps = new float[frames - 1];
			for (int i = 1; i < frames; i++)
				for (int c = 0; c < channels; c++)
					steps[i - 1] = Mathf.Max(steps[i - 1], Mathf.Abs(samples[i * channels + c] - samples[(i - 1) * channels + c]));
			Array.Sort(steps);
			float typical = steps[(int)(steps.Length * .99f)];

			float seam = 0;
			for (int c = 0; c < channels; c++)
				seam = Mathf.Max(seam, Mathf.Abs(samples[c] - samples[(frames - 1) * channels + c]));
			return seam > typical;
		}
	}
}
