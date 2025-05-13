using Game.Terrain;

using System;

using Rectangle = Game.Terrain.Rectangle;

namespace Game.Messaging.Events.Movement
{
	/// <summary>
	/// Indicates that an NPC changed its position.
	/// </summary>
	/// <remarks>sent from the <see cref="Entities.Characters.Character"/> class.</remarks>
	[Serializable]
	public class CharacterMoved : Message
	{
		/// <summary>
		/// The zone in which the NPC was originally located.
		/// </summary>
		public readonly Zone SourceZone;

		/// <summary>
		/// The zone in which the NPC is currently located.
		/// </summary>
		public readonly Zone TargetZone;

		/// <summary>
		/// Original position of the NPC
		/// </summary>
		public readonly Rectangle? SourcePosition;

		/// <summary>
		/// New position of the NPC
		/// </summary>
		public readonly Rectangle TargetPosition;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="sourcePosition">Source position fo the NPC</param>
		/// <param name="targetPosition">Target position of the NPC</param>
		/// <param name="sourceZone">Source zone of the NPC</param>
		/// <param name="targetZone">Target zone of the NPC</param>
		public CharacterMoved(object sender, Rectangle? sourcePosition, Rectangle targetPosition, Zone sourceZone, Zone targetZone) : base(sender)
		{
			SourcePosition = sourcePosition;
			TargetPosition = targetPosition;
			SourceZone = sourceZone;
			TargetZone = targetZone;
		}
	}
}