using Game.Messaging.Commands;
using Game.Terrain;

using System;

namespace Game.Messaging.Events
{
    /// <summary>
    /// Indicates that an NPC has completed a rotation.
    /// </summary>
    /// <remarks>Sent from a descendant of the <see cref="Game.Entities.CharacterComponent"/> class.</remarks>
    [Serializable]
    public class OrientationChanged : ChangeOrientation
    {
        /// <summary>
        /// Indicates fi the orientation was changed immediately or gradually.
        /// </summary>
        public readonly bool Immediately;

        /// <summary>
        /// Original orientation of the NPC
        /// </summary>
        public readonly Orientation2D Source;

        /// <summary>
        /// New orientation of the NPC
        /// </summary>
        public Orientation2D Target;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="source">Original orientation of the NPC</param>
        /// <param name="target">New orientation of the NPC</param>
        public OrientationChanged(object sender, Orientation2D source, Orientation2D target, TurnType direction, bool immediately = false) : base(sender, direction)
        {
            Source = source;
            Target = target;
            Immediately = immediately;
        }
    }
}