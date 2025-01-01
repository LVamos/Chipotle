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

		/// <summary>
		/// Destroys the NPC.
		/// </summary>
		protected override void DestroyObject()
		{
			World.Remove(this);
			Locality.TakeMessage(new CharacterLeftLocality(this, this, Locality, null));
		}

		/// <summary>
		/// A backing field for Locality property.
		/// </summary>
		protected string _locality;

		/// <summary>
		/// Locality intersecting with the character.
		/// </summary>
		[ProtoIgnore]
		public Locality Locality => _locality == null ? null : World.GetLocality(_locality);

		/// <summary>
		/// Represents the inventory of the entity.
		/// </summary>
		public IEnumerable<Item> Inventory
		{
			get
			{
				if (_inventory == null)
					_visitedLocalities = new();

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
		/// List of all localities visited by the NPC
		/// </summary>
		protected HashSet<string> _visitedLocalities;

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

			// Find intersecting locality
			if (_area != null)
				_locality = World.GetLocality(_area.Value.Center).Name.Indexed;
		}

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case PickUpObjectResult m: OnPickUpObjectResult(m); break;
				case OrientationChanged och: OnOrientationChanged(och); break;
				case LocalityChanged lcd: OnLocalityChanged(lcd); break;
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
		/// List of all localities visited by the NPC
		/// </summary>
		public IEnumerable<Locality> VisitedLocalities
		{
			get
			{
				_visitedLocalities ??= new();

				return _visitedLocalities.Select(World.GetLocality);
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
				CharacterComponent c = _components.FirstOrDefault(cm => cm.GetType().IsSubclassOf(type));
				if (c != null)
					c.Activate();
			}

			startComponent(typeof(Sound));
			startComponent(typeof(Input));
			startComponent(typeof(Physics));
			startComponent(typeof(AI));
		}

		/// <summary>
		/// Processes the LocalityChanged message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		private void OnLocalityChanged(LocalityChanged message)
		{
			_locality = message.Target.Name.Indexed;
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

			_locality = message.TargetLocality.Name.Indexed;

			// Record visited locality.
			if (message.SourceLocality != null && message.SourceLocality != message.TargetLocality)
				RecordLocality(message.SourceLocality);

			// Inform all adjecting localities
			List<Locality> localities = new();
			if (message.SourceLocality != null && message.SourceLocality != message.TargetLocality)
			{
				localities.Add(message.SourceLocality);
				localities.AddRange(message.SourceLocality.Neighbours);
			}

			if (message.TargetLocality != null)
			{
				localities.Add(message.TargetLocality);
				localities.AddRange(message.TargetLocality.Neighbours);
			}

			IEnumerable<Locality> targetLocalities = localities.Distinct();

			CharacterMoved moved = new(this, message.SourcePosition, message.TargetPosition, message.SourceLocality, message.TargetLocality);
			CharacterLeftLocality left = new(this, this, message.SourceLocality, message.TargetLocality);
			CharacterCameToLocality came = new(this, this, message.TargetLocality, message.SourceLocality);

			foreach (Locality l in targetLocalities)
			{
				l.TakeMessage(moved);

				if (message.SourceLocality != null && message.SourceLocality != message.TargetLocality)
					l.TakeMessage(left);

				if (message.SourceLocality != message.TargetLocality)
					l.TakeMessage(came);
			}
		}

		/// <summary>
		/// Records the current locality as visited.
		/// </summary>
		protected void RecordLocality(Locality locality)
		{
			if (!VisitedLocalities.Contains(locality))
				_visitedLocalities.Add(locality.Name.Indexed);
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