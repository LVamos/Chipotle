using System;

namespace Game.Messaging.Commands
{
    /// <summary>
    /// Reports current position of an NPC in absolute or relative coordinates.
    /// </summary>
    [Serializable]
    public class SayCoordinates : GameMessage
    {
        /// <summary>
        /// Specifies if the game should report relative or absolute coordinates. Relative coordinates are computed as an offset from upper left corner of the current locality.
        /// </summary>
        public readonly bool Relative;

        public SayCoordinates(object sender, bool relative = true) : base(sender)
            => Relative = relative;
    }
}
