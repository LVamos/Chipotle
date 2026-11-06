using Game;
using Game.Audio;
using Game.Terrain;

using SteamAudio;

using System;
using System.Collections.Generic;
using System.Linq;

using Unity.Collections;
using Unity.Jobs;

using UnityEngine;

using Random = System.Random;
using Vector3 = UnityEngine.Vector3;

namespace Assets.Scripts.Audio
{
	/// <summary>
	/// Lets sound through closed doors. Steam Audio passes only the direct sound through geometry and never the reflections,
	/// yet through a door one hears mostly the reverberant sound of the room behind it.
	/// A closed door is driven by the sound falling on its far side and radiates the part given by the transmission of its material
	/// into the listener's room. A source on the listener's side of the door plays that part and Steam Audio simulates it like any other.
	/// </summary>
	/// <remarks>The sound falling on each door is traced with rays from the source through the acoustic geometry with the materials
	/// Steam Audio uses, so it includes the direct sound and the reverberation of the room behind the door.</remarks>
	public class DoorTransmission
	{
		public DoorTransmission(SoundPool pool)
		{
			_pool = pool;
			SpatialAudio.Simulated += OnSimulated;
		}

		public void Dispose() => SpatialAudio.Simulated -= OnSimulated;

		private readonly SoundPool _pool;

		/// <summary>
		/// Updates the sources in the doors and traces the sound of the sources behind them.
		/// </summary>
		public void Update(IReadOnlyCollection<AudioSource> playingSources)
		{
			if (World.Player == null || Camera.main == null || !FindDoors())
				return;

			Vector3 listener = Camera.main.transform.position;
			Zone zone = World.GetZone(new Vector2(listener.x, listener.z));
			List<Door> doors = zone == null ? new() : _doors.Where(d => d.Collider.enabled && d.Passage.Zones.Contains(zone)).ToList();
			_doorStates = string.Concat(_doors.Select(d => d.Collider.enabled ? '1' : '0'));

			List<AudioSource> origins = playingSources.Where(s => IsOrigin(s, listener, zone)).ToList();
			TraceOrigins(origins);
			UpdateRadiators(origins, doors, listener);
			UpdateOrigins(origins, doors, listener);
		}

		#region Doors

		private class Door
		{
			public Transform Transform;
			public Passage Passage;
			public Collider Collider;
			public int ColliderId;
			public float LastOpenness = -1;
			public Vector3 Center;

			/// <summary>
			/// Horizontal unit vector across the door.
			/// </summary>
			public Vector3 Normal;

			public float HalfThickness;

			/// <summary>
			/// Fraction of the sound energy the door lets through in the low, middle and high band.
			/// </summary>
			public float[] Transmission;
		}

		private List<Door> _doors;
		private Dictionary<int, int> _doorIndices;

		private bool FindDoors()
		{
			if (_doors != null)
				return true;

			GameObject root = GameObject.Find(AcousticDoors.ObjectName);
			if (root == null)
				return false;

			_doors = new();
			_doorIndices = new();
			foreach (Transform transform in root.transform)
			{
				if (World.GetPassage(transform.name) is not Passage passage || !transform.TryGetComponent(out Collider collider))
					continue;

				SteamAudioMaterial material = transform.GetComponent<SteamAudioGeometry>()?.material;
				float[] transmission = material == null ? new[] { .03f, .003f, .001f }
					: new[] { material.lowFreqTransmission, material.midFreqTransmission, material.highFreqTransmission }.Select(t => t * t).ToArray();
				Vector3 scale = transform.lossyScale;
				bool thinAlongX = scale.x < scale.z;
				_doorIndices[collider.GetInstanceID()] = _doors.Count;
				_doors.Add(new Door
				{
					Transform = transform,
					Passage = passage,
					Collider = collider,
					ColliderId = collider.GetInstanceID(),
					Center = transform.position,
					Normal = thinAlongX ? Vector3.right : Vector3.forward,
					HalfThickness = (thinAlongX ? scale.x : scale.z) / 2,
					Transmission = transmission
				});
			}
			return true;
		}

		#endregion

		#region Tracing

		/// <summary>
		/// Rays sent from each source.
		/// </summary>
		private const int _rayCount = 1024;

		/// <summary>
		/// Reflections traced; the rest of the reverberation is extrapolated.
		/// </summary>
		private const int _generations = 30;

		/// <summary>
		/// Last reflections whose rates of hitting the doors and of losing energy extrapolate the rest of the reverberation.
		/// </summary>
		private const int _tailGenerations = 10;

		/// <summary>
		/// Rays traced per frame over all sources.
		/// </summary>
		private const int _raysPerFrame = _rayCount;

		private const float _maxRayLength = 300;

		/// <summary>
		/// How far a source may move before it is traced again, in meters.
		/// </summary>
		private const float _retraceDistance = .5f;

		/// <summary>
		/// Farthest source from the listener that is traced, in meters.
		/// </summary>
		private const float _maxOriginDistance = 60;

		private const int _bands = 3;

		private class Trace
		{
			public Vector3 Origin;

			/// <summary>
			/// Which doors were closed, see <see cref="_doorStates"/>.
			/// </summary>
			public string Doors;
			public Vector3[] Positions;
			public Vector3[] Directions;
			public float[] Energy;
			public bool[] Alive;
			public int Generation;

			/// <summary>
			/// Energy falling on each door per side it passes to (0 toward -Normal, 1 toward +Normal) and band, as a fraction of the source power.
			/// </summary>
			public float[,,] Incident;

			public float[,,] TailIncident;
			public double[] TailLoss = new double[_bands];

			public bool Done;
		}

		/// <summary>
		/// The last finished trace and the one in progress of each source.
		/// </summary>
		private readonly Dictionary<AudioSource, (int Generation, Trace Finished, Trace Running)> _traces = new();

		/// <summary>
		/// Finished traces of each source by the doors that were closed, so a door closed again does not wait for a new trace.
		/// </summary>
		private readonly Dictionary<AudioSource, Dictionary<string, Trace>> _knownTraces = new();

		/// <summary>
		/// One character per door, 1 for a door that blocks the doorway at least partly and 0 for an open one.
		/// </summary>
		private string _doorStates;

		private readonly Dictionary<int, (float[] Absorption, float Scattering)> _materials = new();

		private readonly Random _random = new();

		private readonly int[] _alive = new int[_rayCount];

		private bool IsOrigin(AudioSource source, Vector3 listener, Zone listenerZone)
		{
			if (_radiatorSources.Contains(source) || source.clip == null || !source.isPlaying || !source.spatialize || source.spatialBlend < .99f)
				return false;

			SteamAudioSource steam = _pool.GetSteamAudio(source);
			if (!steam.reflections || SpatialAudio.IsPlacedByGame(steam))
				return false;

			Vector3 position = source.transform.position;
			if (Vector3.Distance(position, listener) > _maxOriginDistance)
				return false;

			return listenerZone == null || World.GetZone(new Vector2(position.x, position.z)) != listenerZone;
		}

		private void TraceOrigins(List<AudioSource> origins)
		{
			foreach (AudioSource origin in _traces.Keys.ToList())
			{
				if (!origins.Contains(origin) || _traces[origin].Generation != _pool.GetGeneration(origin))
				{
					_traces.Remove(origin);
					_knownTraces.Remove(origin);
				}
			}

			foreach (AudioSource origin in origins)
			{
				int generation = _pool.GetGeneration(origin);
				Vector3 position = origin.transform.position;
				if (!_traces.TryGetValue(origin, out var traces))
				{
					traces = (generation, null, null);
					_knownTraces[origin] = new();
				}

				Trace latest = traces.Running ?? traces.Finished;
				bool moved = latest != null && Vector3.Distance(latest.Origin, position) > _retraceDistance;
				if (moved)
					_knownTraces[origin].Clear();

				if (latest == null || moved || latest.Doors != _doorStates)
				{
					traces = _knownTraces[origin].TryGetValue(_doorStates, out Trace known) ? (generation, known, null)
						: (generation, traces.Finished, StartTrace(position));
				}
				_traces[origin] = traces;
			}

			int budget = _raysPerFrame;
			foreach (AudioSource origin in origins.OrderBy(o => Vector3.Distance(o.transform.position, Camera.main.transform.position)))
			{
				var traces = _traces[origin];
				while (budget > 0 && traces.Running != null)
				{
					budget -= Advance(traces.Running);
					if (traces.Running.Done)
					{
						_knownTraces[origin][traces.Running.Doors] = traces.Running;
						_traces[origin] = traces = (traces.Generation, traces.Running, null);
					}
				}
			}
		}

		private Trace StartTrace(Vector3 origin) => new() { Origin = origin, Doors = _doorStates };

		/// <summary>
		/// Sends the rays of a trace from its source when it is traced for the first time.
		/// </summary>
		private void Launch(Trace trace)
		{
			if (trace.Incident != null)
				return;

			if (_syncedStates != _doorStates)
			{
				Physics.SyncTransforms();
				_syncedStates = _doorStates;
			}

			trace.Positions = new Vector3[_rayCount];
			trace.Directions = new Vector3[_rayCount];
			trace.Energy = new float[_rayCount * _bands];
			trace.Alive = new bool[_rayCount];
			trace.Incident = new float[_doors.Count, 2, _bands];
			trace.TailIncident = new float[_doors.Count, 2, _bands];
			Vector3 origin = trace.Origin;
			for (int i = 0; i < _rayCount; i++)
			{
				trace.Positions[i] = origin;
				trace.Directions[i] = UnityEngine.Random.onUnitSphere;
				trace.Alive[i] = true;
				for (int b = 0; b < _bands; b++)
					trace.Energy[i * _bands + b] = 1f / _rayCount;
			}
		}

		private string _syncedStates;

		/// <summary>
		/// Traces one reflection of all rays of a source.
		/// </summary>
		/// <returns>Number of rays cast</returns>
		private int Advance(Trace trace)
		{
			Launch(trace);
			int[] alive = _alive;
			int count = 0;
			for (int i = 0; i < _rayCount; i++)
			{
				if (trace.Alive[i])
					alive[count++] = i;
			}

			if (count == 0 || trace.Generation >= _generations)
			{
				Finish(trace);
				return 0;
			}

			bool tail = trace.Generation >= _generations - _tailGenerations;
			QueryParameters query = new(1 << SpatialAudio.GeometryLayer, false, QueryTriggerInteraction.Ignore, false);
			NativeArray<RaycastCommand> commands = new(count, Allocator.TempJob);
			NativeArray<RaycastHit> hits = new(count, Allocator.TempJob);
			for (int k = 0; k < count; k++)
				commands[k] = new RaycastCommand(trace.Positions[alive[k]], trace.Directions[alive[k]], query, _maxRayLength);
			RaycastCommand.ScheduleBatch(commands, hits, 64, 1).Complete();
			commands.Dispose();

			for (int k = 0; k < count; k++)
			{
				int i = alive[k];
				RaycastHit hit = hits[k];
				int colliderId = hit.colliderInstanceID;
				if (colliderId == 0)
				{
					// The ray left the map, e.g. to the sky.
					trace.Alive[i] = false;
					if (tail)
						for (int b = 0; b < _bands; b++)
							trace.TailLoss[b] += trace.Energy[i * _bands + b];
					continue;
				}

				Vector3 direction = trace.Directions[i];
				if (_doorIndices.TryGetValue(colliderId, out int door))
				{
					int side = Vector3.Dot(direction, _doors[door].Normal) > 0 ? 1 : 0;
					for (int b = 0; b < _bands; b++)
					{
						float energy = trace.Energy[i * _bands + b];
						trace.Incident[door, side, b] += energy;
						if (tail)
							trace.TailIncident[door, side, b] += energy;
					}
				}

				(float[] absorption, float scattering) = GetMaterial(colliderId);
				float strongest = 0;
				for (int b = 0; b < _bands; b++)
				{
					float energy = trace.Energy[i * _bands + b];
					if (tail)
						trace.TailLoss[b] += energy * absorption[b];
					energy *= 1 - absorption[b];
					trace.Energy[i * _bands + b] = energy;
					strongest = Mathf.Max(strongest, energy);
				}

				Vector3 normal = hit.normal;
				if (Vector3.Dot(normal, direction) > 0)
					normal = -normal;
				trace.Directions[i] = _random.NextDouble() < scattering ? Diffuse(normal) : Vector3.Reflect(direction, normal);
				trace.Positions[i] = hit.point + normal * .002f;
				if (strongest < 1e-9f)
					trace.Alive[i] = false;
			}

			hits.Dispose();
			trace.Generation++;
			return count;
		}

		/// <summary>
		/// Adds the reverberation after the last traced reflection. Each further reflection brings the doors the same share of the energy left
		/// and loses the same share as the last traced ones did, so the doors receive the energy left times the ratio of the two shares.
		/// </summary>
		private void Finish(Trace trace)
		{
			float[] remaining = new float[_bands];
			for (int i = 0; i < _rayCount; i++)
			{
				if (trace.Alive[i])
					for (int b = 0; b < _bands; b++)
						remaining[b] += trace.Energy[i * _bands + b];
			}

			for (int b = 0; b < _bands; b++)
			{
				if (trace.TailLoss[b] <= 0)
					continue;

				for (int d = 0; d < _doors.Count; d++)
					for (int side = 0; side < 2; side++)
						trace.Incident[d, side, b] += remaining[b] * (float)(trace.TailIncident[d, side, b] / trace.TailLoss[b]);
			}

			trace.Positions = null;
			trace.Directions = null;
			trace.Energy = null;
			trace.Alive = null;
			trace.TailIncident = null;
			trace.Done = true;
		}

		/// <summary>
		/// A direction scattered by a rough surface: cosine-weighted around the normal.
		/// </summary>
		private Vector3 Diffuse(Vector3 normal)
		{
			double u = _random.NextDouble(), v = _random.NextDouble();
			float radius = (float)Math.Sqrt(u);
			float angle = (float)(2 * Math.PI * v);
			Vector3 tangent = Vector3.Cross(normal, Mathf.Abs(normal.y) < .9f ? Vector3.up : Vector3.right).normalized;
			Vector3 bitangent = Vector3.Cross(normal, tangent);
			return (tangent * (radius * Mathf.Cos(angle)) + bitangent * (radius * Mathf.Sin(angle)) + normal * (float)Math.Sqrt(1 - u)).normalized;
		}

		private (float[] Absorption, float Scattering) GetMaterial(int id)
		{
			if (_materials.TryGetValue(id, out var material))
				return material;

			SteamAudioMaterial steam = (Resources.InstanceIDToObject(id) as Collider)?.GetComponent<SteamAudioGeometry>()?.material;
			material = steam == null ? (new[] { .1f, .1f, .1f }, .5f)
				: (new[] { steam.lowFreqAbsorption, steam.midFreqAbsorption, steam.highFreqAbsorption }, steam.scattering);
			_materials[id] = material;
			return material;
		}

		#endregion

		#region Radiators

		private class Radiator
		{
			public AudioSource Source;
			public int Generation;
			public AudioSource Origin;
			public int OriginGeneration;
			public TransmissionFilter Filter;
			public float[] Gains = new float[_bands];

			/// <summary>
			/// Fades the source in when the door closes and out when it opens, between 0 and 1.
			/// </summary>
			public float Level;

			public bool Wanted;
		}

		private readonly Dictionary<(AudioSource Origin, Door Door), Radiator> _radiators = new();
		private readonly HashSet<AudioSource> _radiatorSources = new();

		/// <summary>
		/// Quietest door source played, as an amplitude gain against the source behind the door.
		/// </summary>
		private const float _minGain = 1e-4f;

		/// <summary>
		/// Fade of a door source in seconds.
		/// </summary>
		private const float _fadeDuration = .1f;

		/// <summary>
		/// Distance of a door source in front of the door, in meters.
		/// </summary>
		private const float _radiatorDistance = .1f;

		private void UpdateRadiators(List<AudioSource> origins, List<Door> doors, Vector3 listener)
		{
			foreach (Radiator radiator in _radiators.Values)
				radiator.Wanted = false;
			HashSet<AudioSource> directThroughDoors = new();

			foreach (AudioSource origin in origins)
			{
				Trace trace = _traces[origin].Finished;
				if (trace == null)
					continue;

				foreach (Door door in doors)
				{
					int index = _doorIndices[door.ColliderId];
					int side = Vector3.Dot(listener - door.Center, door.Normal) > 0 ? 1 : 0;

					// A door swinging open lets through less and less of its area.
					float closed = 1 - AcousticDoors.GetOpenness(door.Transform);
					float[] gains = new float[_bands];
					for (int b = 0; b < _bands; b++)
						gains[b] = Mathf.Sqrt(trace.Incident[index, side, b] * door.Transmission[b] * closed);

					if (gains.Max() >= _minGain && Physics.Linecast(listener, origin.transform.position, out RaycastHit hit, 1 << SpatialAudio.GeometryLayer, QueryTriggerInteraction.Ignore)
						&& hit.colliderInstanceID == door.ColliderId)
						directThroughDoors.Add(origin);

					if (gains.Max() < _minGain && !_radiators.ContainsKey((origin, door)))
						continue;

					if (!_radiators.TryGetValue((origin, door), out Radiator radiator))
					{
						radiator = CreateRadiator(origin, door, side, listener);
						if (radiator == null)
							continue;
						_radiators[(origin, door)] = radiator;
					}

					radiator.Wanted = gains.Max() >= _minGain;
					radiator.Gains = gains;
				}
			}

			PassDirectSound(directThroughDoors);

			float step = Time.deltaTime / _fadeDuration;
			foreach (var (key, radiator) in _radiators.ToList())
			{
				AudioSource source = radiator.Source;
				bool lost = _pool.GetGeneration(source) != radiator.Generation || _pool.GetGeneration(radiator.Origin) != radiator.OriginGeneration || !radiator.Origin.isPlaying;
				radiator.Level = Mathf.Clamp01(radiator.Level + (radiator.Wanted ? step : -step));
				if (lost || radiator.Level <= 0)
				{
					RemoveRadiator(key, radiator);
					continue;
				}

				float loudest = radiator.Gains.Max();
				if (loudest > 0)
					radiator.Filter.SetGains(radiator.Gains[0] / loudest, radiator.Gains[1] / loudest, radiator.Gains[2] / loudest);
				source.volume = radiator.Level * loudest * radiator.Origin.volume;
				source.pitch = radiator.Origin.pitch;
			}
		}

		/// <summary>
		/// Sources whose direct sound Steam Audio no longer lets through a door.
		/// </summary>
		private readonly HashSet<AudioSource> _withoutTransmission = new();

		/// <summary>
		/// The door source carries the direct sound falling on the door too. Were Steam Audio to let it through as well,
		/// the two copies of the same signal would add up or cancel out by chance.
		/// </summary>
		private void PassDirectSound(HashSet<AudioSource> directThroughDoors)
		{
			foreach (AudioSource origin in _withoutTransmission.Except(directThroughDoors).ToList())
			{
				_withoutTransmission.Remove(origin);
				if (origin != null)
					_pool.GetSteamAudio(origin).transmissionInput = TransmissionInput.SimulationDefined;
			}

			foreach (AudioSource origin in directThroughDoors)
			{
				SteamAudioSource steam = _pool.GetSteamAudio(origin);
				steam.transmissionInput = TransmissionInput.UserDefined;
				steam.transmissionLow = steam.transmissionMid = steam.transmissionHigh = 0;
				_withoutTransmission.Add(origin);
			}
		}

		#region Sources behind doors

		private class Origin
		{
			public int Generation;
			public float Gain = 1;

			/// <summary>
			/// Free area of the doorway the source is heard through, from 0 to 1. It follows the door, but grows only once Steam Audio
			/// simulated the reflections through the opened doorway, otherwise they would come in at once at the size reached meanwhile.
			/// </summary>
			public float Opening = 1;

			/// <summary>
			/// A door closed in front of the source and its reflections still come through the doorway until Steam Audio simulates them again.
			/// </summary>
			public bool Waiting;
		}

		private readonly Dictionary<AudioSource, Origin> _origins = new();
		private readonly Dictionary<SteamAudioSource, AudioSource> _waiting = new();

		/// <summary>
		/// Lets the sound of the sources behind a moving door through the doorway in proportion to the area the door leaves free.
		/// </summary>
		/// <remarks>Steam Audio sees a door either closed or open; a swinging one is open for it, and the sources behind it are turned down
		/// to the free area. The door source takes over the part that passes the door itself.</remarks>
		private void UpdateOrigins(List<AudioSource> origins, List<Door> doors, Vector3 listener)
		{
			foreach (AudioSource origin in _origins.Keys.ToList())
			{
				if (!origins.Contains(origin) || _origins[origin].Generation != _pool.GetGeneration(origin))
					ForgetOrigin(origin);
			}

			foreach (AudioSource origin in origins)
			{
				if (!_origins.TryGetValue(origin, out Origin state))
					_origins[origin] = state = new Origin { Generation = _pool.GetGeneration(origin) };

				Trace trace = _traces[origin].Finished;
				float openingTarget = 1;
				foreach (Door door in doors)
				{
					if (trace == null || !IsBehind(trace, door, listener))
						continue;

					float openness = AcousticDoors.GetOpenness(door.Transform);
					bool started = door.LastOpenness == 0 && openness > 0;
					bool shut = door.LastOpenness > 0 && openness == 0;
					if (started || shut)
					{
						// Steam Audio got the door removed or added; the frozen reflections are those of the old doorway.
						_pool.Simulate(origin);
						state.Waiting = true;
						_waiting[_pool.GetSteamAudio(origin)] = origin;
					}

					if (openness > 0 && openness < 1)
						openingTarget = Mathf.Min(openingTarget, openness);
				}

				state.Opening = state.Waiting ? 0
					: Mathf.Min(openingTarget, state.Opening + Time.deltaTime / Settings.DoorOpeningOcclusionDuration);
				float gain = Mathf.Sqrt(state.Opening);
				if (Mathf.Approximately(gain, state.Gain))
					continue;

				state.Gain = gain;
				TransmissionFilter filter = _pool.GetTransmissionFilter(origin);
				filter.enabled = gain < 1;
				origin.spatializePostEffects = true;
				filter.SetGains(gain, gain, gain);
			}

			foreach (Door door in _doors)
				door.LastOpenness = AcousticDoors.GetOpenness(door.Transform);
		}

		/// <summary>
		/// Whether a meaningful part of the sound of a source falls on the far side of a door, as seen from the listener.
		/// </summary>
		private bool IsBehind(Trace trace, Door door, Vector3 listener)
		{
			int index = _doorIndices[door.ColliderId];
			int side = Vector3.Dot(listener - door.Center, door.Normal) > 0 ? 1 : 0;
			for (int b = 0; b < _bands; b++)
			{
				if (trace.Incident[index, side, b] > _minIncident)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Least energy falling on a door, as a fraction of the source power, for the source to count as being behind the door.
		/// </summary>
		private const float _minIncident = 1e-4f;

		private void OnSimulated(SteamAudioSource steam)
		{
			if (_waiting.Remove(steam, out AudioSource origin) && _origins.TryGetValue(origin, out Origin state))
				state.Waiting = false;
		}

		private void ForgetOrigin(AudioSource origin)
		{
			Origin state = _origins[origin];
			_origins.Remove(origin);
			if (origin != null && _pool.GetGeneration(origin) == state.Generation)
			{
				_pool.GetTransmissionFilter(origin).enabled = false;
				_waiting.Remove(_pool.GetSteamAudio(origin));
			}
		}

		#endregion

		private Radiator CreateRadiator(AudioSource origin, Door door, int side, Vector3 listener)
		{
			if (origin.timeSamples >= origin.clip.samples - 1)
				return null;

			Vector3 position = door.Center + (side == 1 ? 1 : -1) * (door.HalfThickness + _radiatorDistance) * door.Normal;
			position = SpatialAudio.OutOfGeometry(listener, position);

			AudioSource source = _pool.GetSource();
			source.name = "door " + origin.name;
			source.transform.position = position;
			source.clip = origin.clip;
			source.loop = origin.loop;
			source.pitch = origin.pitch;
			source.spatialBlend = 1;
			source.spatialize = true;
			source.spatializePostEffects = true;
			source.outputAudioMixerGroup = origin.outputAudioMixerGroup;
			source.dopplerLevel = 0;
			source.volume = 0;

			SteamAudioSource steam = _pool.GetSteamAudio(source);
			steam.reflections = true;
			steam.reflectionsMixLevel = SpatialAudio.GetReflectionsMixLevel(position);

			TransmissionFilter filter = _pool.GetTransmissionFilter(source);
			filter.enabled = true;

			_pool.SoundStartedPlaying(source);
			source.Play();
			source.timeSamples = origin.timeSamples;
			_pool.Simulate(source);
			_radiatorSources.Add(source);

			return new Radiator
			{
				Source = source,
				Generation = _pool.GetGeneration(source),
				Origin = origin,
				OriginGeneration = _pool.GetGeneration(origin),
				Filter = filter
			};
		}

		private void RemoveRadiator((AudioSource, Door) key, Radiator radiator)
		{
			_radiators.Remove(key);
			_radiatorSources.Remove(radiator.Source);
			if (_pool.GetGeneration(radiator.Source) == radiator.Generation)
				radiator.Source.Stop();
		}

		#endregion
	}
}
