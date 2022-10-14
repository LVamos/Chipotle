using Game.Terrain;

using OpenTK;

using System;

namespace Game.Messaging.Events
{
    /// <summary>
    /// Indicates that an NPC collided with a door object.
    /// </summary>
    /// <remarks>
    /// Sent from the <see cref="Game.Entities.Character"/> class. Can be sent only from inside the NPC
    /// from a descendant of the <see cref="Game.Entities.CharacterComponent"/> class.
    /// </remarks>
    [Serializable]
    public class DoorHit : GameMessage
    {
        /// <summary>
        /// The door to which an NPC bumped.
        /// </summary>
        public readonly Door Door;

        /// <summary>
        /// The point at which the door was hit.
        /// </summary>
        public readonly Vector2 Point;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="door"></param>
        /// <param name="point"></param>
        public DoorHit(object sender, Door door, Vector2 point) : base(sender)
        {
            Door = door;
            Point = point;
        }
    }
}