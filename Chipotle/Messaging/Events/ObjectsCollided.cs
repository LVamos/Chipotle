using Game.Entities;

using OpenTK;

using System;

namespace Game.Messaging.Events
{
    /// <summary>
    /// Indicates collision between an NPC and an object.
    /// </summary>
    /// <remarks>Sent from a descendant of the <see cref="Game.Entities.CharacterComponent"/> class.</remarks>
    [Serializable]
    public class ObjectsCollided : GameMessage
    {
        /// <summary>
        /// The colliding object
        /// </summary>
        public readonly GameObject Object;

        /// Position of the tile on which part of the used object lays. It should be always in front
        /// of the NPC. </summary>
        public readonly Vector2 Position;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="collidingObject">The colliding object</param>
        /// <param name="position">POint of the collision</param>
        /// <param name="tile">The tile under the object the NPC bumped to</param>
        public ObjectsCollided(object sender, GameObject collidingObject, Vector2 position) : base(sender)
        {
            Object = collidingObject;
            Position = position;
        }
    }
}