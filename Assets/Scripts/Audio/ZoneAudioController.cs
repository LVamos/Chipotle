using Assets.Scripts.Models;

using Game.Messaging;
using Game.Messaging.Events.GameManagement;
using Game.Messaging.Events.Movement;
using Game.Messaging.Events.Physics;
using Game.Terrain;

using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEditor.Animations;

using UnityEngine;

using static Game.Terrain.Zone;
using static UnityEngine.RectTransform;

namespace Game.Audio
{
	public class ZoneAudioController : GameComponent<Zone>
	{
		/// <summary>
		/// A reference to the parent NPC
		/// </summary>
		[ProtoIgnore]
		public override Zone Owner
		{
			get
			{
				if (_owner == null)
					_owner = World.GetZone(_ownerName)
							 ?? throw new InvalidOperationException(nameof(_ownerName));

				return _owner;
			}
		}

		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case ChipotlesCarMoved m:
					OnChipotlesCarMoved(m); break;
				case CharacterMoved em: OnCharacterMoved(em); break;
				case DoorUsed dm: OnDoorUsed(dm); break;
				case Reloaded:
					OnGameReloaded(); break;
				case CharacterCameToZone m: OnCharacterCameToZone(m); break;
				default: base.HandleMessage(message); break;
			}
		}


		private Zone _owner;

		public void OnDoorUsed(DoorUsed message)
		{
			List<ReadyPortalModel> preparedPortals = PreparePassageLoops();

			if (_portals.ContainsKey(message.Sender))
			{
				ReadyPortalModel preparedPortal = preparedPortals.First(l => l.Passage == message.Sender);
				PortalModel portal = _portals[message.Sender];
				SetPortalVolume(preparedPortal.Passage, portal);
				SetPortalOcclusion(preparedPortal, portal);
			}

			foreach (ReadyPortalModel portal in preparedPortals.Where(p => p.Passage is not Door))
			{
				if (_portals.ContainsKey(portal.Passage))
					SetPortalOcclusion(portal, _portals[portal.Passage]);
			}
		}


		public void Initialize(Zone owner, ZoneLoopInfo loop, ZoneMaterials materials = null)
		{
			_owner = owner ?? throw new ArgumentNullException(nameof(owner));
			_ambientSource = null;
			_portals = new();
			_soundMode = default;
			_materials = materials;

			if (loop != null)
			{
				AmbientSound = loop.Sound;
				_defaultVolume = loop.Volume;
			}
		}

		protected ReadyPortalModel GetPortalByPassage(List<ReadyPortalModel> portals, Passage passage)
		{
			ReadyPortalModel result = portals
				.First(loop => loop.Passage == passage);
			return result;
		}

		private List<Passage> ReadyPortalsToPassages(List<ReadyPortalModel> portals)
		{
			List<Passage> result = portals
				.Select(loop => loop.Passage)
				.ToList();
			return result;
		}


		protected ReadyPortalModel RemoveReadyPortalNearPlayer(List<ReadyPortalModel> portals)
		{
			List<Passage> passages = ReadyPortalsToPassages(portals);
			Passage closestPassage = World.GetClosestElement(passages, World.Player) as Passage;
			ReadyPortalModel closestPortal = GetPortalByPassage(portals, closestPassage);
			portals.Remove(closestPortal);
			return closestPortal;
		}


		public AudioSource ReleaseAmbientSource()
		{
			AudioSource source = _ambientSource;
			_ambientSource = null;
			return source;
		}

		private bool PlayerInSoundRadius
		{ get => _owner.GetDistanceToPlayer() <= Settings.ZoneSoundRadius; }


		/// <summary>
		/// Surfacematerials for walls, floor and ceiling
		/// </summary>
		private ZoneMaterials _materials;

		[ProtoIgnore]
		private AudioSource _ambientSource;


		/// <summary>
		/// Updates position of passage sound loops.
		/// </summary>
		private void UpdatePortals()
		{
			foreach (Passage passage in _portals.Keys)
			{
				Vector2 player = World.Player.Area.Value.Center;
				AudioSource source = _portals[passage].AudioSource;

				// If the player is standing right in the passage locate the sound right on his position.
				if (passage.Area.Value.Contains(player))
				{
					source.transform.position = player.ToVector3(4);
					continue;
				}

				Vector2? point = passage.Area.Value.GetAlignedPoint(player)
				?? passage.Area.Value.GetClosestPoint(player);
				source.transform.position = point.Value.ToVector3(2);
				SetPortalSpatialBlend(passage, source);
				if (source.volume <= 0)
					SetPortalVolume(passage, _portals[passage]);
			}
		}

		private void SetPortalSpatialBlend(Passage passage, AudioSource source)
		{
			float oldSpatialBlend = source.spatialBlend;
			if (passage is Door)
			{
				source.spatialBlend = 1;
				source.outputAudioMixerGroup = Sounds.ResonanceGroup;
				source.spatialize = true;
				return;
			}

			int distance = (int)passage.Area.Value.GetDistanceFrom(World.Player.Area.Value);
			source.spatialBlend = distance > 10 ? 1 : distance * .1f;

			// Turn off spatialization if spatial blend was set to 1.
			if (oldSpatialBlend >= 1)
			{
				source.spatialize = false;
				source.outputAudioMixerGroup = null;
			}
		}

		/// <summary>
		/// Name of background sound played in loop.
		/// </summary>
		public string AmbientSound;
		private float _defaultVolume;

		protected bool TryStealAmbient(Zone previousZone)
		{
			// Coming from another zone.
			if (previousZone == null
								|| !previousZone.SameAmbients(AmbientSound))
				return false;

			if (_owner.PlayerInHere())
			{
				_ambientSource = previousZone.ReleaseAmbientSource();
				StopPortals();
				return true;
			}
			return false;
		}

		public bool SameAmbients(string soundName) => string.Equals(soundName, AmbientSound, StringComparison.OrdinalIgnoreCase);

		protected bool IsSameAmbientNearBy()
=> _owner.Neighbours.Any(n => n.SameAmbients(AmbientSound));

		/// <summary>
		/// Plays the background sound of this zone in a loop.
		/// </summary>
		/// <param name="playerMoved">Specifies if the player just moved from one zone to another one.</param>
		public void UpdateAmbientSounds(Zone previousZone = null)
		{
			if (string.IsNullOrEmpty(AmbientSound))
				return;

			if (previousZone != null && TryStealAmbient(previousZone))
				return;

			bool playerHere = _owner.PlayerInHere();
			if (IsSameAmbientNearBy())
			{
				if (playerHere)
					Play2dAmbient();
				else if (World.Player.Zone.SameAmbients(AmbientSound))
					StopPortals();
				else
					PlayPortals(previousZone);
			}
			else
			{
				if (playerHere)
					Play2dAmbient();
				else PlayPortals(previousZone);
			}
		}

		/// <summary>
		/// Playback mode for background sounds
		/// </summary>
		private ZoneSoundMode _soundMode;

		private void StopLoops()
		{
			Stop(ref _ambientSource);
			foreach (PortalModel loop in _portals.Values)
				Stop(ref loop.AudioSource);
			_portals = new();

			void Stop(ref AudioSource source)
			{
				if (source != null && source.isPlaying)
				{
					source.Stop();
					source = null;
				}
			}
		}


		/// <summary>
		/// Stores the identifiers of location audio loops played in passages.
		/// </summary>
		[ProtoIgnore]
		private Dictionary<Passage, PortalModel> _portals;

		private void PlayPortals(Zone previousZone = null)
		{
			// Portal ambients already playing
			if (previousZone != null && _portals != null && _portals.Any(p => p.Value.AudioSource.isPlaying))
				return;

			Zone playersZone = World.Player.Zone;
			List<ReadyPortalModel> readyPortals = PreparePassageLoops();

			/* 
			 * Player leaved this zone.
			 */
			if (previousZone == this && _ambientSource != null && _ambientSource.isPlaying)
			{
				// Change 2D ambient sound to 3D and place it in the nearest passage between this and new zone. Start playing 3D ambient sounds from other passages between this and the new zone.
				ReadyPortalModel closestPortal = RemoveReadyPortalNearPlayer(readyPortals);

				if (!playersZone.SameAmbients(AmbientSound))
				{
					Action action = () => MoveAmbientToPassage(closestPortal, _defaultVolume, _ambientSource);
					if (!playersZone.IsAccessible(_owner))
						Sounds.SlideVolume(_ambientSource, Settings.Ambient2dFadeDuration, 0, false, false, action);
					else action();
				}
			}

			// Start playback in The remaining exits.
			foreach (ReadyPortalModel loop in readyPortals)
				PlayPortal(loop, _defaultVolume);
		}

		private List<ReadyPortalModel> PreparePassageLoops()
		{
			Zone playersZone = World.Player.Zone;
			List<ReadyPortalModel> result = new();

			foreach (Passage exit in _owner.Exits)
			{
				Vector2 position = default;
				Vector2 player = World.Player.Area.Value.Center;

				// Player stands in the passage
				if (exit.Area.Value.Contains(player))
				{
					Zone other = exit.AnotherZone(playersZone);
					position = other.Area.Value.GetClosestPoint(player);
				}
				else
				{
					// Is the player standing in opposit to the passage?
					Vector2? tmp = exit.Area.Value.GetAlignedPoint(player);
					position = tmp != null ? tmp.Value : exit.Area.Value.GetClosestPoint(player);
				}

				if (exit.LeadsTo(playersZone))
					position = playersZone.Area.Value.GetAlignedPoint(position).Value;
				Vector3 position3d = position.ToVector3(2);

				// Make it quieter if the player is in a inadjecting zone behind a closed door.
				Zone between = playersZone.GetZonesBehindDoor().FirstOrDefault(a => a.IsBehindDoor(_owner));
				bool doubleAttenuation = between != null && playersZone.GetOpenExits(between).IsNullOrEmpty();
				float volume = exit.State == PassageState.Closed ? _defaultVolume : Sounds.GetOverClosedDoorVolume(_defaultVolume);
				if (doubleAttenuation)
					volume *= .01f;

				result.Add(new(exit, position3d, doubleAttenuation));
			}
			return result;
		}

		private void Play2dAmbient(Zone previousZone = null)
		{
			string description = $"2d ambient; {_owner.Name.Indexed}";
			if (_portals.IsNullOrEmpty())
			{
				_ambientSource = Sounds.Play2d(AmbientSound, 0, true, false, description: description);
				Sounds.SlideVolume(_ambientSource, Settings.Ambient2dFadeDuration, _defaultVolume);
				return;
			}

			/*
			 * Find a passage sound that is closest to the player, 
			] * change it to full stereo, disable Low pass and stop the rest of the passage loops.
			 */
			PortalModel portalAmbient = TakeClosestPassageLoop();
			Sounds.ConvertTo2d(portalAmbient.AudioSource, true);
			_ambientSource = portalAmbient.AudioSource;
			_ambientSource.name = description;
			Sounds.SlideVolume(_ambientSource, Settings.Ambient2dFadeDuration, _defaultVolume);

			StopPortals();
		}

		private PortalModel TakeClosestPassageLoop()
		{
			Passage closest = _portals.Keys
				.OrderBy(p => p.Area.Value.GetDistanceFrom(World.Player.Area.Value))
				.FirstOrDefault();
			if (closest == null)
				return null;

			PortalModel loop = _portals[closest];
			_portals.Remove(closest);
			return loop;
		}

		private void PlayPortal(ReadyPortalModel preparedPortal, float volume)
		{
			string description = GetPortalAmbientDescription(preparedPortal.Passage);
			PortalModel newPortal = new();
			AudioSource source = Sounds.Play(AmbientSound, preparedPortal.Position, 0, true, description: description);
			newPortal.AudioSource = source;
			SetDistanceAttenuation(preparedPortal, newPortal);
			SetPortalSpatialBlend(preparedPortal.Passage, newPortal.AudioSource);
			SetPortalVolume(preparedPortal.Passage, newPortal);
			SetPortalOcclusion(preparedPortal, newPortal);
			_portals[preparedPortal.Passage] = newPortal;
		}

		private void SetDistanceAttenuation(ReadyPortalModel preparedPortalAmbient, PortalModel portalAmbient)
		{
			portalAmbient.AudioSource.rolloffMode = AudioRolloffMode.Linear;
			if (preparedPortalAmbient.Passage is Door door)
			{
				if (door.State == PassageState.Open)
					portalAmbient.AudioSource.maxDistance = Settings.PortalAmbientOpenDoorMaxDistance;
				else
					portalAmbient.AudioSource.maxDistance = Settings.PortalAmbientClosedDoorMaxDistance;
			}
			else portalAmbient.AudioSource.maxDistance = Settings.PortalAmbientMaxDistance;

			portalAmbient.AudioSource.minDistance = .5f; // 3D sound
		}

		private float GetPortalVolume(Passage passage, PortalModel portalAmbient)
		{
			float targetVolume = _defaultVolume;
			if (!passage.Open)
			{
				float defaultVolume = Sounds.GetOverClosedDoorVolume(_defaultVolume);
				targetVolume = Sounds.GetLinearRolloffAttenuation(portalAmbient.AudioSource, defaultVolume);
			}
			return targetVolume;
		}

		private void SetPortalVolume(Passage passage, PortalModel portalAmbient)
		{
			float targetVolume = GetPortalVolume(passage, portalAmbient);
			float duration = Settings.Ambient2dFadeDuration;
			if (passage is Door)
			{
				if (!passage.Open)
					duration = Settings.DoorOpeningOcclusionDuration;
				else duration = Settings.DoorClosingOcclusionDuration;
			}

			Sounds.SlideVolume(portalAmbient.AudioSource, duration, targetVolume);
		}

		protected string GetPortalAmbientDescription(Passage passage)
		{
			Zone[] zones = passage.Zones.ToArray();
			string description = $"3d portal ambient for {_owner.Name.Indexed}; passage between {zones[0].Name.Indexed} and {zones[1].Name.Indexed}";
			return description;
		}

		private void MoveAmbientToPassage(ReadyPortalModel preparedPortalAmbient, float volume, AudioSource stereoAmbientSound)
		{
			stereoAmbientSound.transform.position = preparedPortalAmbient.Position;
			PortalModel newPortalAmbient = new()
			{
				AudioSource = stereoAmbientSound
			};
			newPortalAmbient.AudioSource.maxDistance = Settings.PortalAmbientMaxDistance;
			string description = GetPortalAmbientDescription(preparedPortalAmbient.Passage);
			newPortalAmbient.AudioSource.name = description;
			_portals[preparedPortalAmbient.Passage] = newPortalAmbient;
			SetPortalSpatialBlend(preparedPortalAmbient.Passage, newPortalAmbient.AudioSource);
			SetPortalVolume(preparedPortalAmbient.Passage, newPortalAmbient);
			SetPortalOcclusion(preparedPortalAmbient, newPortalAmbient);
			newPortalAmbient.AudioSource.rolloffMode = AudioRolloffMode.Linear;
		}

		private void SetPortalOcclusion(ReadyPortalModel preparedPortal, PortalModel portal)
		{
			Passage passage = preparedPortal.Passage;

			if (passage is Passage)
			{
				if (passage.GetDistanceToPlayer() > Settings.PortalAttenuationDistanceLimit)
					return;

				Zone startZone = passage.AnotherZone(_owner);
				Zone targetZone = World.Player.Zone;
				if (startZone == targetZone || this == targetZone)
				{
					Open(false);
					return;
				}

				if (!startZone.HasPath(targetZone))
				{
					Closed(false);
					return;
				}

				Open(true);
				return;
			}

			// Door
			if (!passage.Open)
				Open(true);
			else Closed(preparedPortal.DoubleAttenuation);

			void Closed(bool doubleAttenuation)
			{
				float frequency = doubleAttenuation ? Sounds.OverWallLowpass : Sounds.OverClosedDoorLowpass;
				Sounds.SlideLowPass(portal.AudioSource, Settings.DoorClosingOcclusionDuration, frequency);
			}

			void Open(bool attenuate)
			{
				if (attenuate)
					Sounds.SlideLowPass(portal.AudioSource, Settings.DoorOpeningOcclusionDuration, Sounds.OverOpenDoorLowpass);
				else Sounds.SlideLowPass(portal.AudioSource, Settings.DoorOpeningOcclusionDuration, 22000, true);
			}
		}


		/// <summary>
		/// Stops all isntances of background sound.
		/// </summary>
		/// <param name="fadeOut">Specifies if the loop is faded out</param>
		private void StopAmbientSounds()
		{
			if (_ambientSource != null && _ambientSource.isPlaying)
			{
				Sounds.SlideVolume(_ambientSource, Settings.Ambient2dFadeDuration, 0);
				_ambientSource = null;
			}

			StopPortals();
		}

		private void StopPortals()
		{
			foreach (PortalModel loop in _portals.Values)
				Sounds.SlideVolume(loop.AudioSource, .5f, 0);

			_portals = new();
		}

		private void OnGameReloaded()
		{
			_portals = new();
			UpdateAmbientSounds();
		}

		private void OnCharacterMoved(CharacterMoved message)
		{
			if (message.Sender == World.Player)
				UpdatePortals();
		}

		private void OnChipotlesCarMoved(ChipotlesCarMoved message)
		{
			StopAmbientSounds();
			throw new NotImplementedException();
		}

		private void OnCharacterCameToZone(CharacterCameToZone message)
		{
			if (message.Character != World.Player)
				return;
			Sounds.SetRoomParameters(_owner, _materials);
			UpdateAmbientSounds(message.PreviousZone);
		}
	}
}
