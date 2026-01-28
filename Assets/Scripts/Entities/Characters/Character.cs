using Game.Entities.Characters.Components;
using Game.Entities.Items;
using Game.Messaging;
using Game.Messaging.Commands.Physics;
using Game.Messaging.Events.Movement;
using Game.Messaging.Events.Physics;
using Game.Terrain;

using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;

using Input = Game.Entities.Characters.Components.Input;
using Message = Game.Messaging.Message;
using Physics = Game.Entities.Characters.Components.Physics;

namespace Game.Entities.Characters
{
	/// <summary>
	/// Represents an NPC.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class Character : Entity
	{

		private void OnPlaceItemResult(PlaceItemResult message)
		{
			if (message.Success)
				_inventory.Remove(message.Item.Name.Indexed);
		}

		/// <summary>
		/// Destroys the NPC.
		/// </summary>
		public override void Destroy()
		{
			World.Remove(this);
			Zone.TakeMessage(new CharacterLeftZone(this, this, Zone, null));
		}

		/// <summary>
		/// A backing field for Zone property.
		/// </summary>
		protected string _zone;

		/// <summary>
		/// Zone intersecting with the character.
		/// </summary>
		[ProtoIgnore]
		public Zone Zone => _zone == null ? null : World.GetZone(_zone);

		/// <summary>
		/// Represents the inventory of the entity.
		/// </summary>
		public IEnumerable<Item> Inventory
		{
			get
			{
				if (_inventory == null)
					_visitedZones = new();

				return _inventory.Select(World.GetItem);
			}
		}

		/// <summary>
		/// Represents the inventory. The objects are stored as indexed names.
		/// </summary>
		protected HashSet<string> _inventory;

		/// <summary>
		/// List of all zones visited by the NPC
		/// </summary>
		protected HashSet<string> _visitedZones;

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="name">Inner and public anme of the NPC</param>
		/// <param name="type">Type of the NPC</param>
		/// <param name="ai">Reference to an AI component</param>
		/// <param name="input">Reference to an input component</param>
		/// <param name="physics">Reference to an physics component</param>
		/// <param name="sound">Reference to an sound component</param>
		public new void Initialize(Name name, string type, AI ai, Input input, Physics physics, Sound sound)
		{
			base.Initialize(name, type, null);
			_inventory = new();
			_visitedZones = new();
			_zone = null;

			Usable = true;
			_components =
				new CharacterComponent[] { ai, physics, input, sound }
					.Where(c => c != null)
					.ToArray();

			foreach (CharacterComponent c in _components)
			{
				c.Initialize();
				c.SetParent(name.Indexed);

			}

			if (physics.StartPosition != null)
			{
				Area = physics.StartPosition;
				transform.position = Area.Value.Center.ToVector3(2);
			}

			// Find intersecting zone
			if (_area != null)
				_zone = World.GetZone(_area.Value.Center).Name.Indexed;
		}

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case PlaceItemResult m: OnPlaceItemResult(m); break;
				case PickUpItemResult m: OnPickUpObjectResult(m); break;
				case OrientationChanged och: OnOrientationChanged(och); break;
				case ZoneChanged lcd: OnZoneChanged(lcd); break;
				case PositionChanged pcd: OnPositionChanged(pcd); break;
				default: base.HandleMessage(message); break;
			}
		}

		/// <summary>
		/// Handles the PickUpObjectResult message.
		/// </summary>
		/// <param name="m">The message to be processed</param>
		private void OnPickUpObjectResult(PickUpItemResult m)
		{
			if (m.Result == PickUpItemResult.ResultType.Success)
				_inventory.Add(m.Object.Name.Indexed);
		}

		/// <summary>
		/// Handles the OrienttationChanged message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		private void OnOrientationChanged(OrientationChanged message)
		{
			Orientation = message.Target;
			float degrees = (float)message.Target.Angle.CartesianDegrees;
			transform.eulerAngles = new Vector3(0, degrees, 0);
		}

		/// <summary>
		/// Returns the current orientation of the NPC in the game world.
		/// </summary>
		public Orientation2D Orientation { get; private set; }

		/// <summary>
		/// List of all zones visited by the NPC
		/// </summary>
		public IEnumerable<Zone> VisitedZones
		{
			get
			{
				_visitedZones ??= new();

				return _visitedZones.Select(World.GetZone);
			}
		}

		/// <summary>
		/// Initializes the NPC and starts its message loop.
		/// </summary>
		public override void Activate()
		{
			base.Activate();

			void startComponent(Type type)
			{
				MessagingObject c = _components.FirstOrDefault(c => IsOfTypeOrSubclass(c, type));
				c?.Activate();

				bool IsOfTypeOrSubclass(MessagingObject component, Type type)
				{
					Type componentType = component.GetType();
					return componentType.IsSubclassOf(type) || componentType == type;
				}
			}

			startComponent(typeof(Sound));
			startComponent(typeof(Input));
			startComponent(typeof(Physics));
			startComponent(typeof(AI));
		}

		/// <summary>
		/// Processes the ZoneChanged message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnZoneChanged(ZoneChanged message) => _zone = message.Target.Name.Indexed;

		/// <summary>
		/// Processes incoming messages.
		/// </summary>
		public override void GameUpdate()
		{
			base.GameUpdate();

			foreach (CharacterComponent c in _components)
				c.GameUpdate();
		}

		/// <summary>
		/// Processes the PositionChanged message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected void OnPositionChanged(PositionChanged message)
		{
			Rectangle targetPosition = message.TargetPosition;
			SetPosition(targetPosition, message.TargetZone);
			RecordZone(message.SourceZone, message.TargetZone);
			AnnounceZoneChange(message.SourceZone, message.TargetZone);
			AnnouncePosition(message.SourcePosition, targetPosition, message.SourceZone, message.TargetZone);
		}

		private void AnnounceZoneChange(Zone sourceZone, Zone targetZone)
		{
			CharacterLeftZone left = new(this, this, sourceZone, targetZone);
			CharacterCameToZone came = new(this, this, targetZone, sourceZone);

			if (sourceZone != null && sourceZone != targetZone)
				World.MessageZones(left);

			if (sourceZone != targetZone)
				World.MessageZones(came);
		}

		private void AnnouncePosition(Rectangle? sourcePosition, Rectangle targetPosition, Zone sourceZone, Zone targetZone, bool messageZones = true)
		{
			// todo Implement listener pattern
			CharacterMoved moved = new(this, sourcePosition, targetPosition, sourceZone, targetZone);
			World.MessageCharacters(moved);
			if (messageZones)
				World.MessageZones(moved);
		}

		private void SetPosition(Rectangle position, Zone zone)
		{
			Area = position;
			transform.position = Center.ToVector3(1.8f);
			_zone = zone.Name.Indexed;
		}

		/// <summary>
		/// Records the current zone as visited.
		/// </summary>
		protected void RecordZone(Zone sourceZone, Zone targetZone)
		{
			if (sourceZone != null && sourceZone != targetZone)
				_visitedZones.Add(sourceZone.Name.Indexed);
		}

		/// <summary>
		/// Processes the Destroy message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private new void OnDestroyObject(DestroyObject message) => Destroy();
	}
}