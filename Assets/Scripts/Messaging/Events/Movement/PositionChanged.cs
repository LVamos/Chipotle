using Game.Terrain;

using System;

using Rectangle = Game.Terrain.Rectangle;

namespace Game.Messaging.Events.Movement
{
	/// <summary>
	/// Indicates that an NPC changed its position.
	/// </summary>
	/// <remarks>Sent from a descendant of the <see cref="Entities.Characters.Components.CharacterComponent"/> class.</remarks>
	[Serializable]
	public class PositionChanged : CharacterMoved
	{
		/// <summary>
		/// Describes type of obstacle between the entity and the player if any.
		/// </summary>
		public readonly ObstacleType Obstacle;

		/// <summary>
		/// Indicates if foot steps of the NPC should be audible.
		/// </summary>
		public readonly bool Silently;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="sourcePosition">Source position of the NPC</param>
		/// <param name="targetPosition">Target position of the NPC</param>
		/// <param name="sourceZone"Source zone of the NPC></param>
		/// <param name="targetZone">Target zone of the NPC</param>
		/// <param name="obstacle">Describes type of obstacle between the entity and the player if any</param>
		/// <param name="silently">Determines if the fott steps of the NPC should be audible</param>
		public PositionChanged(object sender, Rectangle? sourcePosition, Rectangle targetPosition, Zone sourceZone, Zone targetZone, ObstacleType obstacle = ObstacleType.None, bool silently = false)
			: base(sender, sourcePosition, targetPosition, sourceZone, targetZone)
		{
			Obstacle = obstacle;
			Silently = silently;

		}
	}
}