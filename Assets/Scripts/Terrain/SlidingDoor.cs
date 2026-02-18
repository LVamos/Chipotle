using Game.Entities.Characters;
using Game.Messaging.Events.Movement;



using System.Collections.Generic;

using UnityEngine;

using Message = Game.Messaging.Message;

namespace Game.Terrain
{
	/// <summary>
	/// Represents a sliding door.
	/// </summary>
	
	public class SlidingDoor : Door
	{

		/// <summary>
		/// Processes the EntityMoved message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected override void OnCharacterMoved(CharacterMoved message)
		{
			base.OnCharacterMoved(message);

			Character npc = message.Sender as Character;
			if (npc != World.Player)
				return;

			Vector2 center = npc.Center;
			bool opposite = IsInFrontOrBehind(center);
			bool near = _area.Value.GetDistanceFrom(center) <= _minDistance;

			// Find point from which the door sound should be heart.
			Vector2? point = _area.Value.GetAlignedPoint(center);
			if (point == null)
				point = _area.Value.GetClosestPoint(center);
			if (opposite && near && (State == PassageState.Closed || State == PassageState.Locked))
				Open(npc, point.Value);
			else if (!near && State == PassageState.Open)
				Close(npc, point.Value);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Inner name of the door</param>
		/// <param name="area">Coordinates of the area the door occupies</param>
		/// <param name="zones">The zones connected by the door</param>
		public override void Initialize(Name name, Rectangle area, IEnumerable<string> zones)
		{
			base.Initialize(name, PassageState.Closed, area, zones);
			State = PassageState.Locked;
			_openingSound = _closingSound = "SlidingDoor1";
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
				case CharacterMoved em: OnCharacterMoved(em); break;
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