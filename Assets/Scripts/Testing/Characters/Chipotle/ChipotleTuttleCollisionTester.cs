using Game.Entities.Characters;
using Game.Entities.Characters.Components;
using Game.Messaging.Events.Physics;
using Game.Terrain;

using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Game.Testing.Characters.Chipotle
{
	/// <summary>
	/// An AI component that periodically collides Chipotle with Tuttle for testing purposes.
	/// </summary>
	public class ChipotleTuttleCollisionTester : AI
	{
		/// <summary>
		/// Called every frame.
		/// </summary>
		private void Update()
		{
			if (TimerElapsed())
				ShoveTuttle();
		}

		/// <summary>
		/// Shoves Tuttle by jumping Chipotle into him.
		/// </summary>
		private void ShoveTuttle()
		{
			List<Vector2> points = Rectangle.GetPointsAround(Tuttle.Area.Value, 2, 6, 1);
			Vector2 target = points.First();
			JumpTo(target);
			Vector2? point = Tuttle.Area.Value.GetClosestPoint(Owner.Center);
			ObjectsCollided message = new(Owner, Tuttle, point.Value);
			Tuttle.TakeMessage(message);
			_lastTimeStamp = Time.realtimeSinceStartup;
		}

		/// <summary>
		/// Returns reference to the Tuttle NPC.
		/// </summary>
		private Character Tuttle
		{
			get
			{
				_tuttle ??= World.GetCharacter("tuttle");
				return _tuttle;
			}
		}

		/// <summary>
		/// Reference to the Tuttle NPC.
		/// </summary>
		private Character _tuttle;

		/// <summary>
		/// Checks if interval between actions has elapsed.
		/// </summary>
		/// <returns>True if the interval has elapsed, false otherwise.</returns>
		private bool TimerElapsed() => Time.realtimeSinceStartup - _lastTimeStamp >= _interval;

		/// <summary>
		/// Timestamp of the last action.
		/// </summary>
		private float _lastTimeStamp;

		/// <summary>
		/// Interval between actions in seconds.
		/// </summary>
		private const float _interval = 22;

		/// <summary>
		/// Initializes the AI component.
		/// </summary>
		public override void Initialize()
		{
			transform.localScale = new(.4f, 1.7f, .4f);
			_owner = World.Player;
		}
	}
}
