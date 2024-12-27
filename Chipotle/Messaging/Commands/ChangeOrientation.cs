using Game.Terrain;

using System;

namespace Game.Messaging.Commands
{
    /// <summary>
    /// Tells the NPC to turn a given number of degrees.
    /// </summary>
    /// <remarks>
    /// Applies to the <see cref="Game.Entities.Character"/> class. Can be sent only from inside the
    /// NPC from a descendant of the <see cref="Game.Entities.CharacterComponent"/> class.
    /// </remarks>
    [Serializable]
    public class ChangeOrientation : GameMessage
    {
        /// <summary>
        /// Specifies how many degrees the NPC should turn.
        /// </summary>
        public readonly int Degrees;

        /// <summary>
        /// Specifies how much the NPC should turn.
        /// </summary>
        public readonly TurnType Direction;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="direction">Specifies how much the NPC should turn.</param>
        public ChangeOrientation(object sender, TurnType direction) : base(sender)
        {
            Direction = direction;
            Degrees = (int)direction;
        }
    }
}