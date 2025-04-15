using Game.Entities.Characters.Components;
using Game.Entities.Items;
using Game.Messaging.Commands.Physics;
using Game.Messaging.Events.GameManagement;
using Game.Messaging.Events.Movement;
using Game.Messaging.Events.Physics;
using Game.Terrain;

using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Linq;

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
				_inventory.Remove(message.Sender.Name.Indexed);
		}

		/// <summary>
		/// Destroys the NPC.
		/// </summary>
		protected override void DestroyObject()
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
		protected HashSet<string> _inventory = new();

		/// <summary>
		/// List of all entity components
		/// </summary>
		protected CharacterComponent[] _components;

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

			Usable = true;
			_components =
				new CharacterComponent[] { ai, physics, input, sound }
					.Where(c => c != null)
					.ToArray();

			foreach (CharacterComponent c in _components)
			{
				c.Initialize();
				c.AssignToEntity(name.Indexed);

			}

			if (physics.StartPosition != null)
				Area = new(physics.StartPosition.Value);

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
				case PickUpObjectResult m: OnPickUpObjectResult(m); break;
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
		private void OnPickUpObjectResult(PickUpObjectResult m)
		{
			if (m.Result == PickUpObjectResult.ResultType.Success)
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
		/// Takes an incoming message and saves it into the message queue.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		public override void TakeMessage(Message message)
		{
			base.TakeMessage(message);

			if (_messagingEnabled)
				SendInnerMessage(message);
		}

		/// <summary>
		/// Initializes the NPC and starts its message loop.
		/// </summary>
		public override void Activate()
		{
			base.Activate();

			void startComponent(Type type)
			{
				CharacterComponent c = _components.FirstOrDefault(c => IsOfTypeOrSubclass(c, type));
				c?.Activate();

				bool IsOfTypeOrSubclass(CharacterComponent component, Type type)
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
		private void OnZoneChanged(ZoneChanged message)
		{
			_zone = message.Target.Name.Indexed;
		}

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
			Area = message.TargetPosition; // Set new position.
			transform.position = new Vector3(Area.Value.Center.x, 2, Area.Value.Center.y);

			_zone = message.TargetZone.Name.Indexed;

			// Record visited zone.
			if (message.SourceZone != null && message.SourceZone != message.TargetZone)
				RecordZone(message.SourceZone);

			// Inform all adjecting zones
			HashSet<Zone> zones = new();
			if (message.SourceZone != null && message.SourceZone != message.TargetZone)
			{
				zones.Add(message.SourceZone);
				HashSet<Zone> temp = new(message.SourceZone.Neighbours);
				zones.UnionWith(temp);
			}

			if (message.TargetZone != null)
			{
				zones.Add(message.TargetZone);
				HashSet<Zone> temp = new(message.TargetZone.Neighbours);
				zones.UnionWith(temp);
			}

			CharacterMoved moved = new(this, message.SourcePosition, message.TargetPosition, message.SourceZone, message.TargetZone);
			CharacterLeftZone left = new(this, this, message.SourceZone, message.TargetZone);
			CharacterCameToZone came = new(this, this, message.TargetZone, message.SourceZone);

			foreach (Zone zone in zones)
			{
				zone.TakeMessage(moved);

				if (message.SourceZone != null && message.SourceZone != message.TargetZone)
					zone.TakeMessage(left);

				if (message.SourceZone != message.TargetZone)
					zone.TakeMessage(came);
			}
		}

		/// <summary>
		/// Records the current zone as visited.
		/// </summary>
		protected void RecordZone(Zone zone)
		{
			if (!VisitedZones.Contains(zone))
				_visitedZones.Add(zone.Name.Indexed);
		}

		/// <summary>
		/// Sends a message to al components.
		/// </summary>
		/// <param name="message">Message to redistribute</param>
		protected virtual void SendInnerMessage(Message message)
		{
			foreach (CharacterComponent c in _components)
			{
				if (c != message.Sender || message is Reloaded)
					c.TakeMessage(message);
			}
		}

		/// <summary>
		/// Checks if a message came from inside the NPC.
		/// </summary>
		/// <param name="message">The message to check</param>
		/// <returns>True if the message came from inside the NPC</returns>
		private bool IsInternal(Message message)
		{
			return message.Sender is CharacterComponent c && c.Owner == this;
		}

		/// <summary>
		/// Processes the Destroy message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private new void OnDestroyObject(DestroyObject message)
		{
			if (!IsInternal(message))
				throw new InvalidOperationException("This message can be sent only from an inner component.");

			DestroyObject();
		}
	}
}