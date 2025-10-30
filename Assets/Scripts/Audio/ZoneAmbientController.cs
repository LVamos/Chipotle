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

		[ProtoIgnore]
		public override Zone Owner => _owner ??= World.GetZone(_ownerName) ?? throw new InvalidOperationException(nameof(_ownerName));

		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case ChipotlesCarMoved m: OnChipotlesCarMoved(m); break;
				case Reloaded: OnGameReloaded(); break;
				case CharacterCameToZone m: OnCharacterCameToZone(m); break;
				default: base.HandleMessage(message); break;
			}
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
			AmbientRegistry.Unregister2D(AmbientSound);
			return (_ambientSource, _ambientSource = null).Item1;
		}

		public bool SameAmbients(string soundName) =>
			string.Equals(soundName, AmbientSound, StringComparison.OrdinalIgnoreCase);

		protected bool IsSameAmbientNearBy() =>
			_owner.Neighbours.Any(n => n.SameAmbients(AmbientSound));

		public void UpdateAmbient(Zone previousZone = null)
		{
			//test
			if (_owner.Name.Indexed == "výčep h1" && Owner.PlayerInHere())
				Console.WriteLine("");

			if (!_owner.PlayerInHere())
			{
				if (_ambientSource != null)
				{
					AmbientRegistry.Unregister2D(AmbientSound);
					Sounds.SlideVolume(_ambientSource, Settings.Ambient2dFadeDuration, 0, false);
				}
				return;
			}

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
				AmbientRegistry.Register2D(AmbientSound, _ambientSource);
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
			//test
			if (_owner.Name.Indexed == "výčep h1" && Owner.PlayerInHere())
				Console.WriteLine("");
			string description = $"2d ambient; {_owner.Name.Indexed}";

			// Get portals and select the one closest to the player.
			HashSet<AudioSource> portals = AmbientRegistry.TryGetPortals(AmbientSound);
			// Find the closest one to the player
			if (portals.IsNullOrEmpty())
				_ambientSource = Sounds.Play2d(AmbientSound, 0, true, false, description: description);
			else
			{
				AudioSource portal = GetClosestPortal(portals);
				Sounds.ConvertTo2d(portal, false);
				_ambientSource = portal;
				_ambientSource.name = description;
			}

			AmbientRegistry.Register2D(AmbientSound, _ambientSource);
			//test
			if (_owner.Name.Indexed == "výčep h1")
				Console.WriteLine("");

			Sounds.SlideVolume(_ambientSource, Settings.Ambient2dFadeDuration, _defaultVolume);
		}

		private void StopAmbient()
		{
			if (_ambientSource?.isPlaying == true)
			{
				AmbientRegistry.Unregister2D(AmbientSound);
				Sounds.SlideVolume(_ambientSource, Settings.Ambient2dFadeDuration, 0);
			}
			_ambientSource = null;
		}

		private void OnGameReloaded() => UpdateAmbient();

		private void OnChipotlesCarMoved(ChipotlesCarMoved _)
		{
			StopAmbient();
			throw new NotImplementedException();
		}

		private void OnCharacterCameToZone(CharacterCameToZone message)
		{
			if (message.Character != World.Player)
				return;
			//test
			if (_owner.Name.Indexed == "výčep h1")
				Console.WriteLine("");
			Sounds.SetRoomParameters(_owner, _materials);
			UpdateAmbient(message.PreviousZone);
		}
	}
}
