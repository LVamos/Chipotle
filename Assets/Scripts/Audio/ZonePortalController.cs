using Assets.Scripts.Models;
using Assets.Scripts.Terrain;

using Game;
using Game.Audio;
using Game.Messaging;
using Game.Messaging.Events.GameManagement;
using Game.Messaging.Events.Movement;
using Game.Messaging.Events.Physics;
using Game.Terrain;

using Microsoft.VisualBasic.Devices;

using ProtoBuf;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using UnityEngine.InputSystem.Composites;

using YamlDotNet.Core.Tokens;

namespace Assets.Scripts.Audio
{
	public class ZonePortalController : GameComponent<Zone>
	{
		private float _portalMaxDistance;
		private float _closedDoorMaxDistance;
		private List<string> _portalAudibleZones;
		private Zone PlayersZone => World.Player.Zone;

		private Zone _owner;

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

		public void Initialize(Zone owner, ZoneLoopInfo loop)
		{
			_owner = owner ?? throw new ArgumentNullException(nameof(owner));
			_portals = new();

			if (loop != null)
			{
				AmbientSound = loop.Sound;
				_defaultVolume = loop.Volume;
				_portalAudibleZones = loop.PortalAudibleZones;
				_portalMaxDistance = loop.PortalMaxDistance ?? Settings.PortalMaxDistance;
				_closedDoorMaxDistance = loop.ClosedDoorMaxDistance ?? Settings.ClosedDoorMaxDistance;
				_openDoorMaxDistance = loop.OpenDoorMaxDistance ?? Settings.OpenDoorMaxDistance;
			}
		}

		public void OnDoorUsed(DoorUsed message)
		{
			List<PortalAnchor> anchors = GetAnchors();

			if (_portals.ContainsKey(message.Sender))
			{
				PortalAnchor anchor = anchors.First(l => l.Passage == message.Sender);
				AudioSource portal = _portals[message.Sender];
				SetVolume(anchor.Passage, portal);
				SetOcclusion(anchor, portal);
			}

			foreach (PortalAnchor portal in anchors.Where(p => p.Passage is not Door))
			{
				if (_portals.ContainsKey(portal.Passage))
					SetOcclusion(portal, _portals[portal.Passage]);
			}
		}

		private void OnGameReloaded()
		{
			_portals = new();
			UpdatePortals();
		}

		private void OnCharacterCameToZone(CharacterCameToZone message)
		{
			//test
			if (_owner.Name.Indexed == "výčep h1")
				Console.WriteLine("");
			if (message.Character != World.Player)
				return;

			if (_owner.PlayerInHere())
				StopUnusedPortals();
			else UpdatePortals();
		}

		private void StopUnusedPortals()
		{
			AudioSource ambient2d = AmbientRegistry.TryGet2D(AmbientSound);
			foreach (var portal in _portals.Values)
			{
				if (portal != ambient2d)
					Sounds.SlideVolume(portal, Settings.Ambient3dFadeDuration, 0);
			}
			AmbientRegistry.UnregisterPortal(AmbientSound);
			_portals = new();
		}

		private void OnCharacterMoved(CharacterMoved message)
		{
			if (message.Sender == World.Player)
				UpdatePortals();
		}

		private void FadeAmbientTo3d(PortalAnchor anchor, float volume, AudioSource ambient2d)
		{
			//test
			if (Owner.Name.Indexed == "výčep h1" && Owner.PlayerInHere())
				Console.WriteLine("");
			AmbientRegistry.RegisterPortal(AmbientSound, ambient2d);
			ambient2d.transform.position = anchor.Position;
			AudioSource newPortal = ambient2d;
			newPortal.maxDistance = _portalMaxDistance;
			newPortal.name = GetDescription(anchor.Passage);
			_portals[anchor.Passage] = newPortal;
			SetSpatialBlend(anchor.Passage, newPortal);
			SetVolume(anchor.Passage, newPortal);
			SetOcclusion(anchor, newPortal);
			newPortal.rolloffMode = AudioRolloffMode.Linear;
		}

		private void OnChipotlesCarMoved(ChipotlesCarMoved message)
		{
			StopPortals();
			throw new NotImplementedException();
		}

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

		/// <summary>
		/// Stores the identifiers of location audio loops played in passages.
		/// </summary>
		[ProtoIgnore]
		private Dictionary<Passage, AudioSource> _portals;

		private float _defaultVolume;

		protected string GetDescription(Passage passage)
		{
			//test
			if (_owner.Name.Indexed == "výčep h1")
				Console.WriteLine("");

			Zone[] zones = passage.Zones.ToArray();
			string description = $"3d portal ambient for {_owner.Name.Indexed}; passage between {zones[0].Name.Indexed} and {zones[1].Name.Indexed}";
			return description;
		}

		private float GetVolume(Passage passage, AudioSource portal)
		{
			float targetVolume = _defaultVolume;

			if (!passage.Open)
			{
				float defaultVolume = Sounds.GetOverClosedDoorVolume(_defaultVolume);
				targetVolume = Sounds.GetLinearRolloffAttenuation(portal, defaultVolume);
			}
			return targetVolume;
		}

		/// <summary>
		/// Name of background sound played in loop.
		/// </summary>
		public string AmbientSound;
		private float _openDoorMaxDistance;

		private void PlayPortal(PortalAnchor anchor, float volume)
		{
			string description = GetDescription(anchor.Passage);
			AudioSource newPortal = Sounds.Play(AmbientSound, anchor.Position, 0, true, description: description);
			AmbientRegistry.RegisterPortal(AmbientSound, newPortal);
			SetAttenuation(anchor, newPortal);
			SetSpatialBlend(anchor.Passage, newPortal);
			SetVolume(anchor.Passage, newPortal);
			SetOcclusion(anchor, newPortal);
			_portals[anchor.Passage] = newPortal;
		}

		private IEnumerator FadeAmbientTo3dDelayed(PortalAnchor anchor)
		{
			yield return new WaitForSeconds(Settings.Ambient3dFadeDuration);
			AudioSource portal = AmbientRegistry.TryGet2D(AmbientSound);
			if (portal != null)
				FadeAmbientTo3d(anchor, _defaultVolume, portal); FadeAmbientTo3d(anchor, _defaultVolume, portal);
		}

		private void PlayPortals(Zone previousZone = null)
		{
			// Portal ambients already playing
			if (previousZone != null && PortalsPlaying())
				return;

			Zone playersZone = World.Player.Zone;
			List<PortalAnchor> anchors = GetAnchors();

			/* 
			 * Player leaved this zone.
			 */
			if (previousZone == Owner)
			{
				// Change 2D ambient sound to 3D and place it in the nearest passage between this and new zone. Start playing 3D ambient sounds from other passages between this and the new zone.
				PortalAnchor closestPortal = RemoveAnchorNearPlayer(anchors);

				if (!playersZone.SameAmbients(AmbientSound))
				{
					if (!playersZone.IsAccessible(_owner))
						StartCoroutine(FadeAmbientTo3dDelayed(closestPortal));
					else FadeAmbientTo3dDelayed(closestPortal);
				}
			}

			// Start playback in The remaining exits.
			foreach (PortalAnchor loop in anchors)
				PlayPortal(loop, _defaultVolume);
		}

		private bool PortalsPlaying()
		{
			return _portals != null && _portals.Any(p => p.Value.isPlaying);
		}

		private List<PortalAnchor> GetAnchors()
		{
			Zone playersZone = World.Player.Zone;
			Dictionary<Passage, Vector3> positions = GetPortalPositions();
			List<PortalAnchor> anchors = new List<PortalAnchor>();

			foreach (Passage exit in _owner.Exits)
			{
				float volume = CalculatePortalVolume(exit, playersZone);
				anchors.Add(new PortalAnchor(exit, positions[exit], volume < _defaultVolume * .02f));
			}

			return anchors;
		}

		private float CalculatePortalVolume(Passage passage, Zone playerZone)
		{
			if (playerZone.GetZonesBehindDoor().FirstOrDefault(a => a.IsBehindDoor(_owner)) is Zone between)
				if (playerZone.GetOpenExits(between).IsNullOrEmpty())
					return _defaultVolume * .01f;

			return passage.State == PassageState.Closed
				? Sounds.GetOverClosedDoorVolume(_defaultVolume)
				: _defaultVolume;
		}

		private Dictionary<Passage, Vector3> GetPortalPositions()
		{
			Dictionary<Passage, Vector3> positions = new Dictionary<Passage, Vector3>();
			Vector2 player = World.Player.Center;
			Zone playersZone = World.Player.Zone;

			foreach (Passage exit in _owner.Exits)
			{
				Vector2 position = exit.Area.Value.Contains(player)
					? exit.AnotherZone(playersZone).Area.Value.GetClosestPoint(player)
					: exit.Area.Value.GetAlignedPoint(player) ?? exit.Area.Value.GetClosestPoint(player);

				if (exit.LeadsTo(playersZone))
					position = playersZone.Area.Value.GetAlignedPoint(position).Value;

				positions[exit] = position.ToVector3(2);
			}

			return positions;
		}

		protected PortalAnchor RemoveAnchorNearPlayer(List<PortalAnchor> anchors)
		{
			List<Passage> passages =
				anchors.Select(loop => loop.Passage)
				.ToList();

			Passage closestPassage = World.GetClosestElement(passages, World.Player) as Passage;
			PortalAnchor closestAnchor = anchors
				.First(loop => loop.Passage == closestPassage);

			anchors.Remove(closestAnchor);
			return closestAnchor;
		}

		private void SetAttenuation(PortalAnchor anchor, AudioSource portal)
		{
			portal.rolloffMode = AudioRolloffMode.Linear;
			if (anchor.Passage is Door door)
				portal.maxDistance = door.Open ? _openDoorMaxDistance : _closedDoorMaxDistance;
			else portal.maxDistance = _portalMaxDistance;

			portal.minDistance = .5f; // 3D sound
		}

		private void SetOcclusion(PortalAnchor anchor, AudioSource portal)
		{
			Passage passage = anchor.Passage;

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
			else Closed(anchor.DoubleAttenuation);

			void Closed(bool doubleAttenuation)
			{
				float frequency = doubleAttenuation ? Sounds.OverWallLowpass : Sounds.OverClosedDoorLowpass;
				Sounds.SlideLowPass(portal, Settings.DoorClosingOcclusionDuration, frequency);
			}

			void Open(bool attenuate)
			{
				if (attenuate)
					Sounds.SlideLowPass(portal, Settings.DoorOpeningOcclusionDuration, Sounds.OverOpenDoorLowpass);
				else Sounds.SlideLowPass(portal, Settings.DoorOpeningOcclusionDuration, 22000, true);
			}
		}

		private void SetSpatialBlend(Passage passage, AudioSource portal)
		{
			float oldSpatialBlend = portal.spatialBlend;
			if (passage is Door)
			{
				portal.spatialBlend = 1;
				portal.outputAudioMixerGroup = Sounds.ResonanceGroup;
				portal.spatialize = true;
				return;
			}

			int distance = (int)passage.Area.Value.GetDistanceFrom(World.Player.Area.Value);
			portal.spatialBlend = distance > 10 ? 1 : distance * .1f;

			// Turn off spatialization if spatial blend was set to 1.
			if (oldSpatialBlend >= 1)
			{
				portal.spatialize = false;
				portal.outputAudioMixerGroup = null;
			}
		}

		private void SetVolume(Passage passage, AudioSource portal)
		{
			float targetVolume = GetVolume(passage, portal);
			float duration = Settings.Ambient2dFadeDuration;
			if (passage is Door)
			{
				if (!passage.Open)
					duration = Settings.DoorOpeningOcclusionDuration;
				else duration = Settings.DoorClosingOcclusionDuration;
			}

			Sounds.SlideVolume(portal, duration, targetVolume);
		}

		private void StopPortals()
		{
			foreach (AudioSource loop in _portals.Values)
				Sounds.SlideVolume(loop, .5f, 0);

			_portals = new();
		}

		/// <summary>
		/// Updates position of passage sound loops.
		/// </summary>
		private void UpdatePortals()
		{
			if (_portals.IsNullOrEmpty())
			{
				PlayPortals();
				return;
			}

			IEnumerable<PortalAnchor> readyPortals = GetAnchors()
				.Where(p => _portals.ContainsKey(p.Passage));
			foreach (PortalAnchor portal in readyPortals)
			{
				Vector2 player = World.Player.Center;
				AudioSource source = _portals[portal.Passage];

				// If the player is standing right in the passage locate the sound right on his position.
				if (portal.Passage.Area.Value.Contains(player))
				{
					source.transform.position = player.ToVector3(4);
					continue;
				}

				Vector2? point = portal.Passage.Area.Value.GetAlignedPoint(player)
				?? portal.Passage.Area.Value.GetClosestPoint(player);
				source.transform.position = point.Value.ToVector3(2);
				SetSpatialBlend(portal.Passage, source);

				// Update volume
				// Mute it if player is in a inadjecting zone behind a closed door.
				if (!PlayersZone.IsAccessible(_owner)
					&& !_portalAudibleZones.Contains(PlayersZone.Name.Indexed))
				{
					Sounds.SlideVolume(source, Settings.Ambient3dFadeDuration, 0, false);
					source.volume = 0;
					continue;
				}

				if (source.volume <= 0)
					SetVolume(portal.Passage, source);
				SetOcclusion(portal, source);
			}
		}
	}
}
