using Game.Entities.Characters.Components;
using Game.Entities.Items;
using Game.Mapping.Saves;
using Game.Messaging;
using Game.Messaging.Commands.Characters;
using Game.Messaging.Commands.Physics;
using Game.Messaging.Events.Characters.Movement;
using Game.Messaging.Events.Movement;
using Game.Messaging.Events.Physics;
using Game.Serialization.Protobuf.Snapshots.Characters;
using Game.Terrain;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;

using Input = Game.Entities.Characters.Components.Input;
using Message = Game.Messaging.Message;
using Physics = Game.Entities.Characters.Components.PhysicsComponent.Physics;

namespace Game.Entities.Characters
{
	/// <summary>
	/// Represents an NPC.
	/// </summary>
	public class Character : Entity
	{
		public void Restore(CharacterSave save)
		{
			base.Restore(save);

			transform.localScale = save.Dimensions.ToVector3();
			_inventory = save.Inventory != null ? new(save.Inventory) : null;
			_visitedZones = save.VisitedZones != null ? new(save.VisitedZones) : null;
			_zone = save.Zone;
			Orientation = save.Orientation.ToVector2();

			foreach (CharacterComponent component in _components)
			{
				foreach (ComponentSave componentSave in save.Components)
					component.Restore(componentSave);
			}
		}

		public CharacterSave Export()
		{
			var save = base.Export().ToCharacterSave();

			save.Dimensions = transform.localScale.ToVector3Save();
			save.Inventory = !_inventory.IsNullOrEmpty() ? new(_inventory) : null;
			save.VisitedZones = !_visitedZones.IsNullOrEmpty() ? new(_visitedZones) : null;
			save.Zone = _zone;
			save.Orientation = Orientation.UnitVector.ToVector2Save();

			// Export components
			List<ComponentSave> exportedComponents = new();
			foreach (CharacterComponent component in _components)
				exportedComponents.Add(component.Export());
			save.Components = exportedComponents;

			return save;
		}

		private void OnPlaceItemResult(PlaceItemResult message)
		{
			if (message.Success)
				_inventory.Remove(message.Item.Name.Inner);
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
		public new void Initialize(
			Name name,
			string type,
			Vector2 position,
			Vector2 orientation,
			Vector3 dimensions,
			AI ai,
			Input input,
			Physics physics,
			Sound sound)
		{
			base.Initialize(name, type, null);
			_inventory = new();
			_visitedZones = new();

			Usable = true;
			_components =
				new CharacterComponent[] { ai, physics, input, sound }
					.Where(c => c != null)
					.ToArray();

			InitComponents();

			transform.localScale = dimensions;
			Rectangle area = Rectangle.FromCenter(position, dimensions.x, dimensions.z);
			Zone zone = World.GetZone(area.Center);
			SavePosition(area, zone);
			SetOrientation(new(orientation));
		}

		private void InitComponents()
		{
			foreach (CharacterComponent c in _components)
			{
				c.Initialize();
				c.SetParent(Name.Inner);
			}
		}

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case DiscardInventoryItem m:
					OnDiscardInventoryItem(m); break;
				case PlaceItemResult m: OnPlaceItemResult(m); break;
				case PickUpItemResult m: OnPickUpObjectResult(m); break;
				case OrientationChanged och: OnOrientationChanged(och); break;
				case ZoneChanged lcd: OnZoneChanged(lcd); break;
				case PositionChanged pcd: OnPositionChanged(pcd); break;
				default: base.HandleMessage(message); break;
			}
		}

		private void OnDiscardInventoryItem(DiscardInventoryItem message)
		{
			string name = message.Item.Name.Inner;
			_inventory.Remove(name);
		}

		/// <summary>
		/// Handles the PickUpObjectResult message.
		/// </summary>
		/// <param name="m">The message to be processed</param>
		private void OnPickUpObjectResult(PickUpItemResult m)
		{
			if (m.Result == PickUpItemResult.ResultType.Success)
				_inventory.Add(m.Object.Name.Inner);
		}

		/// <summary>
		/// Handles the OrienttationChanged message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		private void OnOrientationChanged(OrientationChanged message)
		{
			SetOrientation(message.Target);
			AnnounceOrientation(message.Source, message.Target);
		}

		private void AnnounceOrientation(Orientation2D source, Orientation2D target)
		{
			CharacterRotated message = new(this, source, target);
			World.MessagePlayerCameraController(message);
		}

		private void SetOrientation(Orientation2D orientation)
		{
			Orientation = orientation;
			float degrees = (float)orientation.Angle.CartesianDegrees;
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

			// Start components
			startComponent(typeof(Sound));
			startComponent(typeof(Input));
			startComponent(typeof(Physics));
			startComponent(typeof(AI));

			// Announce position
			AnnouncePosition(null, Area.Value, null, Zone);
			AnnounceOrientation(Orientation, Orientation);
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
		}

		/// <summary>
		/// Processes the ZoneChanged message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnZoneChanged(ZoneChanged message) => _zone = message.Target.Name.Inner;

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
			SavePosition(message.TargetPosition, message.TargetZone);
			AnnouncePosition(
						message.SourcePosition.Value,
						message.TargetPosition,
						message.SourceZone,
						message.TargetZone);
		}

		private void AnnouncePosition(Rectangle? sourcePosition, Rectangle targetPosition, Zone sourceZone, Zone targetZone)
		{
			RecordZone(sourceZone, targetZone);
			AnnounceZoneChange(sourceZone, targetZone);

			CharacterMoved moved = new(this, sourcePosition, targetPosition, sourceZone, targetZone);
			World.MessagePlayerCameraController(moved);
			World.MessageCharacters(moved);
			World.MessageZones(moved);
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

		private void SavePosition(Rectangle position, Zone zone)
		{
			Area = position;
			transform.position = Center.ToVector3(1.8f);
			_zone = zone.Name.Inner;
		}

		/// <summary>
		/// Records the current zone as visited.
		/// </summary>
		protected void RecordZone(Zone sourceZone, Zone targetZone)
		{
			if (sourceZone != null && sourceZone != targetZone)
				_visitedZones.Add(sourceZone.Name.Inner);
		}

		/// <summary>
		/// Processes the Destroy message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private new void OnDestroyObject(DestroyObject message) => Destroy();
	}
}