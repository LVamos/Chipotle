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
		private List<Zone> _multipleExitZOnes;

		private void CollectMultipleExitZones()
		{
			_multipleExitZOnes = new();
			Dictionary<Zone, int> _zoneConnections = new();
			foreach (Passage exit in _owner.Exits)
			{
				_zoneConnections[exit.Zones.First()]++;
				_zoneConnections[exit.Zones.Last()]++;
			}

			foreach (KeyValuePair<Zone, int> record in _zoneConnections)
			{
				if (record.Value > 0)
					_multipleExitZOnes.Add(record.Key);
			}
		}

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
				ApplyLoopSettings(loop);
			CollectMultipleExitZones();
		}

		private void ApplyLoopSettings(ZoneLoopInfo loop)
		{
			_loop = new()
			{
				Sound = loop.Sound,
				Volume = loop.Volume,
				PortalAudibleZones = loop.PortalAudibleZones,
				PortalMaxDistance = loop.PortalMaxDistance ?? Settings.PortalMaxDistance,
				ClosedDoorMaxDistance = loop.ClosedDoorMaxDistance ?? Settings.ClosedDoorMaxDistance,
				OpenDoorMaxDistance = loop.OpenDoorMaxDistance ?? Settings.OpenDoorMaxDistance
			};
		}

		private ZoneLoopInfo _loop;

		public void OnDoorUsed(DoorUsed message)
		{
			List<PortalAnchor> anchors = ZonePortalHelper.GetAnchors(_owner, _loop);

			if (_portals.ContainsKey(message.Sender))
			{
				PortalAnchor anchor = anchors.First(l => l.Passage == message.Sender);
				AudioSource portal = _portals[message.Sender];
				SetPortalParameters(anchor, portal, false, true, true, false);
			}

			foreach (PortalAnchor anchor in anchors.Where(p => p.Passage is not Door))
			{
				if (_portals.ContainsKey(anchor.Passage))
					SetPortalParameters(anchor, _portals[anchor.Passage], false, false, true, false);
			}
		}

		private void OnGameReloaded()
		{
			_portals = new();
			UpdatePortals();
		}

		private void OnCharacterCameToZone(CharacterCameToZone message)
		{
			if (message.Character != World.Player)
				return;

			if (_owner.PlayerInHere())
				StopUnusedPortals();
			else UpdatePortals(message.PreviousZone);
		}

		private void StopUnusedPortals()
		{
			AudioSource ambient2d = AmbientRegistry.TryGet2D(_owner.Name.Indexed);
			foreach (AudioSource portal in _portals.Values)
			{
				if (portal != ambient2d)
				{
					AmbientRegistry.UnregisterPortal(_owner.Name.Indexed, portal);
					Sounds.SlideVolume(portal, Settings.Ambient3dFadeDuration, 0);
				}
			}
			_portals = new();
		}

		private void OnCharacterMoved(CharacterMoved message)
		{
			if (message.Sender == World.Player)
				UpdatePortals();
		}

		private void FadeAmbientTo3d(PortalAnchor anchor)
		{
			AudioSource portal = AmbientRegistry.TryGet2D(_owner.Name.Indexed);
			AmbientRegistry.Unregister2D(_owner.Name.Indexed);
			AmbientRegistry.RegisterPortal(_owner.Name.Indexed, portal);
			portal.transform.position = anchor.Position;
			portal.maxDistance = _loop.PortalMaxDistance.Value;
			portal.name = ZonePortalHelper.GetDescription(anchor.Passage, _owner.Name.Indexed);
			_portals[anchor.Passage] = portal;
			SetPortalParameters(anchor, portal, false, true, true, true);
			portal.rolloffMode = AudioRolloffMode.Linear;
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

		private float GetVolume(Passage passage, AudioSource portal)
		{
			float targetVolume = _loop.Volume;

			if (!passage.Open)
				return Sounds.GetOverClosedDoorVolume(_loop.Volume);
			return _loop.Volume;
		}


		private void SetPortalParameters(PortalAnchor anchor, AudioSource portal, bool attenuation = true, bool volume = true, bool occlusion = true, bool spatialBlend = true)
		{
			if (attenuation)
				SetAttenuation(anchor, portal);
			if (volume)
				SetVolume(anchor.Passage, portal);
			if (occlusion)
				SetOcclusion(anchor, portal);
			if (spatialBlend)
				SetSpatialBlend(anchor.Passage, portal);
		}

		private void PlayPortal(PortalAnchor anchor)
		{
			string description = ZonePortalHelper.GetDescription(anchor.Passage, _owner.Name.Indexed);
			AudioSource portal = Sounds.Play(_loop.Sound, anchor.Position, 0, true, description: description);
			AmbientRegistry.RegisterPortal(_owner.Name.Indexed, portal);
			SetPortalParameters(anchor, portal);
			_portals[anchor.Passage] = portal;
		}

		private IEnumerator FadeAmbientTo3dDelayed(PortalAnchor anchor)
		{
			AudioSource portal = AmbientRegistry.TryGet2D(_owner.Name.Indexed);
			MutePortal(portal);
			yield return new WaitForSeconds(Settings.Ambient3dFadeDuration);
			if (portal != null)
				FadeAmbientTo3d(anchor);
		}

		private void PlayPortals(Zone previousZone = null)
		{
			// Portals already playing
			if (previousZone != null && PortalsPlaying())
				return;

			List<PortalAnchor> anchors = ZonePortalHelper.GetAnchors(_owner, _loop);

			// Player leaved this zone.
			if (previousZone == Owner)
			{
				// Change 2D ambient sound to 3D and place it in the nearest passage between this and new zone. Start playing 3D ambient sounds from other passages between this and the new zone.
				PortalAnchor closestPortal = RemoveAnchorNearPlayer(anchors);

				if (!PlayersZone.SameAmbients(_loop.Sound))
				{
					if (!PlayersZone.IsAccessible(_owner))
						StartCoroutine(FadeAmbientTo3dDelayed(closestPortal));
					else FadeAmbientTo3d(closestPortal);
				}
			}

			// Start playback in The remaining exits.
			foreach (PortalAnchor anchor in anchors)
				PlayPortal(anchor);
		}

		private bool PortalsPlaying()
		{
			return _portals != null && _portals.Any(p => p.Value.isPlaying);
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
			portal.minDistance = Settings.PortalMinDistance;

			List<Zone> zones = anchor.Passage.Zones.ToList();
			if (zones.Any(z => _multipleExitZOnes.Contains(z)))
			{
				UseIndividualParameters();
				return;
			}

			// Set individual parameters for the exit nearest to the player. Use default parameters for the others.
			Passage exitNearPlayer = _owner.GetNearestExits(World.Player.Center, null, true).First();
			if (anchor.Passage == exitNearPlayer)
				UseIndividualParameters();
			else UseDefaultParameters();

			void UseDefaultParameters()
			{
				if (anchor.Passage is Door door)
					portal.maxDistance = door.Open ? Settings.OpenDoorMaxDistance : Settings.ClosedDoorMaxDistance;
				else portal.maxDistance = Settings.PortalMaxDistance;
			}

			void UseIndividualParameters()
			{
				if (anchor.Passage is Door door)
					portal.maxDistance = door.Open ? _loop.OpenDoorMaxDistance.Value : _loop.ClosedDoorMaxDistance.Value;
				else portal.maxDistance = _loop.PortalMaxDistance.Value;
			}
		}

		private void SetOcclusion(PortalAnchor anchor, AudioSource portal, bool fadingTo3d = false)
		{
			Passage passage = anchor.Passage;

			if (passage.GetDistanceToPlayer() > Settings.PortalAttenuationDistanceLimit)
				return;

			Zone targetZone = passage.AnotherZone(PlayersZone);
			if (!PlayersZone.HasPath(targetZone))
			{
				Closed(false);
				return;
			}
			Open(true);

			void Closed(bool doubleAttenuation)
			{
				float frequency = doubleAttenuation ? Sounds.OverWallLowpass : Sounds.OverClosedDoorLowpass;
				Sounds.SlideLowPass(portal, Settings.DoorClosingOcclusionDuration, frequency);
				portal.outputAudioMixerGroup = Sounds.ResonanceGroup;
				portal.spatialize = true;
			}

			void Open(bool attenuate)
			{
				if (attenuate)
				{
					float duration = fadingTo3d ? Settings.Portal2dFadingDuration : Settings.DoorOpeningOcclusionDuration;
					Sounds.SlideLowPass(portal, duration, Sounds.OverOpenDoorLowpass);
				}
				else Sounds.SlideLowPass(portal, Settings.DoorOpeningOcclusionDuration, 22000, true);
			}
			portal.outputAudioMixerGroup = null;
			portal.spatialize = false;
		}

		private void SetSpatialBlend(Passage exit, AudioSource portal)
		{
			float oldBlend = portal.spatialBlend;
			if (exit is Door)
			{
				Action action = () =>
				{
					if (!exit.Open)
						Sounds.EnableSpatializer(portal);
				};
				Sounds.SlideSpatialBlend(portal, Settings.PortalBlendSlidingDuration, 1, action);
				return;
			}

			int distance = (int)exit.Area.Value.GetDistanceFrom(World.Player.Area.Value);
			float finalBlend = distance > 10 ? 1 : distance * .2f;
			Sounds.SlideSpatialBlend(portal, Settings.PortalBlendSlidingDuration, finalBlend);

			if (exit.Open)
				Sounds.DisableSpatializer(portal);
		}

		private void SetVolume(Passage passage, AudioSource portal)
		{
			if (
PlayersZone.SameAmbients(_loop.Sound)
|| (!PlayersZone.IsAccessible(_owner) && !AudibleInPlayersZone())
				)
			{
				MutePortal(portal);
				return;
			}

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

		private bool PlayPortalsIfNeeded(Zone previousZone)
		{
			if (_portals.IsNullOrEmpty() && !Owner.PlayerInHere())
			{
				PlayPortals(previousZone);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Updates position of passage sound loops.
		/// </summary>
		private void UpdatePortals(Zone previousZone = null)
		{
			PlayPortalsIfNeeded(previousZone);

			List<PortalAnchor> anchors = ZonePortalHelper.GetAnchors(_owner, _loop)
				.Where(p => _portals.ContainsKey(p.Passage))
				.ToList();
			foreach (PortalAnchor anchor in anchors)
			{
				AudioSource portal = _portals[(Passage)anchor.Passage];
				if (PlayerInPassage(anchor.Passage))
					SnapPortalToPlayer(World.Player.Center, portal);
				else
				{
					MovePortalInFrontOfPlayer(anchor, portal);
					SetPortalParameters(anchor, portal, true, true, true, true);
				}
			}
		}

		private bool PlayerInPassage(Passage passage) => passage.Area.Value.Contains(World.Player.Center);

		private bool AudibleInPlayersZone()
		{
			return _loop.PortalAudibleZones.Contains(PlayersZone.Name.Indexed);
		}

		private void MutePortal(AudioSource portal)
		{
			Sounds.SlideVolume(portal, Settings.Ambient2dFadeDuration, 0, false);
		}

		private void MovePortalInFrontOfPlayer(PortalAnchor anchor, AudioSource portal)
		{
			Vector2? point = anchor.Passage.Area.Value.GetAlignedPoint(World.Player.Center)
			?? anchor.Passage.Area.Value.GetClosestPoint(World.Player.Center);
			portal.transform.position = point.Value.ToVector3(2);
		}

		private static void SnapPortalToPlayer(Vector2 player, AudioSource portal)
		{
			portal.transform.position = player.ToVector3(4);
		}
	}
}
