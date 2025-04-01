using Game.Entities.Characters;
using Game.Messaging.Events.Movement;

using ProtoBuf;

using System.Collections.Generic;

using UnityEngine;

using Message = Game.Messaging.Message;

namespace Game.Terrain
{
	/// <summary>
	/// Represents a sliding door.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class SlidingDoor : Door
	{

		/// <summary>
		/// Processes the EntityMoved message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected override void OnEntityMoved(CharacterMoved message)
		{
			base.OnEntityMoved(message);

			Character entity = message.Sender as Character;
			bool opposite = IsInFrontOrBehind(entity.Area.Value.Center);
			bool near = _area.Value.GetDistanceFrom(entity.Area.Value.Center) <= _minDistance;

			// Find point from which the door sound should be heart.
			Vector2 center = entity.Area.Value.Center;
			Vector2? tmp = _area.Value.GetAlignedPoint(center);
			Vector2 point = tmp.HasValue ? (Vector2)tmp : _area.Value.GetClosestPoint(entity.Area.Value.Center);

			if (opposite && near && (State == PassageState.Closed || State == PassageState.Locked))
				Open(entity, point);
			else if (!near && State == PassageState.Open)
				Close(entity, point);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Inner name of the door</param>
		/// <param name="area">Coordinates of the area the door occupies</param>
		/// <param name="localities">The localities connected by the door</param>
		public override void Initialize(Name name, Rectangle area, IEnumerable<string> localities)
		{
			base.Initialize(name, PassageState.Closed, area, localities);
			State = PassageState.Locked;
			_openingSound = _closingSound = "SlidingDoor";
		}

		/// <summary>
		/// Specifies the minimum distance between the entity and the door at which the door opens.
		/// </summary>
		protected int _minDistance = 4;

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case CharacterMoved em: OnEntityMoved(em); break;
				default: base.HandleMessage(message); break;
			}
		}

		/// <summary>
		/// Initializes the door and starts its message loop.
		/// </summary>
		public override void Activate()
		{
			base.Activate();
		}
	}
}