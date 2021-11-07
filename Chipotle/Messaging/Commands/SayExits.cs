using System.Collections.Generic;

namespace Game.Messaging.Commands
{
    public class SayExits : GameMessage
    {
        /// <summary>
        /// Information about the exits including description of particular exits and angles between
        /// an NPC and each exit
        /// </summary>
        public readonly IEnumerable<(string description, double compassDegrees)> ExitInfo;

        /// <summary>
        /// Indicates that there's no exit from a concerning locality.
        /// </summary>
        public readonly bool NothingFound;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        public SayExits(object sender) : base(sender) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="exitInfo">Information about the exits</param>
        public SayExits(object sender, IEnumerable<(string description, double compassDegrees)> exitInfo) : base(sender)
            => ExitInfo = exitInfo;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="nothingFound">Indicates that there's no exit from a concerning locality</param>
        public SayExits(object sender, bool nothingFound) : base(sender)
            => NothingFound = nothingFound;
    }
}