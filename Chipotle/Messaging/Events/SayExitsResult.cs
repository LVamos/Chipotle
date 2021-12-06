using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Game.Terrain;

namespace Game.Messaging.Events
{
    public class SayExitsResult: GameMessage
    {
        /// <summary>
        /// Information about the exits including description of particular exits and angles between
        /// an NPC and each exit
        /// </summary>
        public readonly IEnumerable<(string description, double compassDegrees)> ExitInfo;

        /// <summary>
        /// An exit the NPC stands in.
        /// </summary>
        public readonly Passage OccupiedPassage;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="exitInfo">Information about the exits</param>
        public SayExitsResult(object sender, IEnumerable<(string description, double compassDegrees)> exitInfo) : base(sender)
            => ExitInfo = exitInfo;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="occupiedPassage">An exit the NPC stands in</param>
        public SayExitsResult(object sender, Passage occupiedPassage):base(sender)
        {
            OccupiedPassage = occupiedPassage;
            ExitInfo = null;

        }
    }
}
