using Assets.Scripts.Models;

using Game.Messaging;
using Game.Messaging.Events.GameManagement;
using Game.Messaging.Events.Movement;
using Game.Terrain;


using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Game.Audio
{
	public class ZoneAmbientController : GameComponent<Zone>
	{
		private Zone _owner;
		private AudioSource _ambientSource;
		private ZoneMaterials _materials;
		private float _defaultVolume;

		public string AmbientSound;


		public override Zone Owner => _owner ??= World.GetZone(_ownerName) ?? throw new InvalidOperationException(nameof(_ownerName));

		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case GameStatechanged m:
					OnGameStatechanged(m); break;
				case ChipotlesCarMoved m: OnChipotlesCarMoved(m); break;
				case Reloaded: OnGameReloaded(); break;
				case CharacterCameToZone m: OnCharacterCameToZone(m); break;
				default: base.HandleMessage(message); break;
			}
		}

		private void OnGameStatechanged(GameStatechanged message)
		{
			if (message.Current == GameState.Finished)
				StopAmbient();
		}

		public void Initialize(Zone owner, ZoneLoopInfo loop, ZoneMaterials materials = null)
		{
			_owner = owner ?? throw new ArgumentNullException(nameof(owner));
			_materials = materials;
			if (loop != null)
			{
				AmbientSound = loop.Sound;
				_defaultVolume = loop.Volume;
			}
		}

		public AudioSource ReleaseAmbientSource()
		{
			AmbientRegistry.Unregister2D(_owner.Name.Inner);
			return (_ambientSource, _ambientSource = null).Item1;
		}

		public bool SameAmbients(string soundName) =>
			string.Equals(soundName, AmbientSound, StringComparison.OrdinalIgnoreCase);

		protected bool IsSameAmbientNearBy() =>
			_owner.Neighbours.Any(n => n.SameAmbients(AmbientSound));

		private Zone PlayersZone => World.Player.Zone;

		public void UpdateAmbient(Zone previousZone = null)
		{
			if (!_owner.PlayerInHere())
				return;

			ResonanceRoomParameters parameters = new(
				_owner.transform.position,
				_owner.transform.localScale,
				_materials,
				_owner.Type == ZoneType.Outdoor);
			Sounds.RoomManager.SimulateRoom(parameters);

			if (AmbientSound == null)
				return;

			if (previousZone != null && TryStealAmbient(previousZone))
				return;

			PlayAmbient();
		}

		private bool TryStealAmbient(Zone previousZone)
		{
			if (previousZone == null || !previousZone.SameAmbients(AmbientSound))
				return false;

			if (_owner.PlayerInHere())
			{
				_ambientSource = previousZone.ReleaseAmbientSource();
				AmbientRegistry.Register2D(_owner.Name.Inner, _ambientSource);
				return true;
			}
			return false;
		}

		private AudioSource GetClosestPortal(IEnumerable<AudioSource> portals)
		{
			AudioSource closest = null;
			float minDistance = float.MaxValue;
			Vector3 playerPosition = World.Player.gameObject.transform.position;

			foreach (AudioSource portal in portals)
			{
				Vector3 portalPosition = portal.gameObject.transform.position;
				float distance = (portalPosition - playerPosition).sqrMagnitude;
				if (distance < minDistance)
				{
					minDistance = distance;
					closest = portal;
				}
			}

			return closest;
		}

		private void PlayAmbient()
		{
			string description = $"2d ambient; {_owner.Name.Inner}";

			// Get portals and select the one closest to the player.
			HashSet<AudioSource> portals = AmbientRegistry.TryGetPortals(_owner.Name.Inner);
			// Find the closest one to the player
			if (portals.IsNullOrEmpty())
				_ambientSource = Sounds.Play2d(AmbientSound, 0, true, false, description: description);
			else
				FadePortalTo2d(description, portals);

			AmbientRegistry.Register2D(_owner.Name.Inner, _ambientSource);
			Sounds.SlideVolume(_ambientSource, Settings.Ambient2dFadeDuration, _defaultVolume);
		}

		private void FadePortalTo2d(string description, HashSet<AudioSource> portals)
		{
			AudioSource portal = GetClosestPortal(portals);
			AmbientRegistry.UnregisterPortal(_owner.Name.Inner, portal);
			Sounds.SwitchTo2d(portal, true, Settings.Portal2dFadingDuration);
			_ambientSource = portal;
			_ambientSource.name = description;
		}

		private void StopAmbient()
		{
			if (_ambientSource?.isPlaying == true)
			{
				AmbientRegistry.Unregister2D(_owner.Name.Inner);
				Sounds.SlideVolume(_ambientSource, Settings.Ambient2dFadeDuration, 0);
			}
			_ambientSource = null;
		}

		private void OnGameReloaded() => UpdateAmbient();

		private void OnChipotlesCarMoved(ChipotlesCarMoved _)
		{
			StopAmbient();
		}

		private void OnCharacterCameToZone(CharacterCameToZone message)
		{
			if (message.Character != World.Player)
				return;

			UpdateAmbient(message.PreviousZone);
		}
	}
}
