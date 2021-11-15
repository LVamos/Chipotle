using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Game.Entities;

namespace Game.Messaging.Events
{
    public class SaySurroundingObjectsResult: GameMessage
    {
        /// <summary>
        /// Information about the nearest objects
        /// </summary>
        public readonly IEnumerable<(DumpObject o, double compassDegrees)> ObjectInfo;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Sorce of the message</param>
        /// <param name="objectInfo">Information about the nearest objects</param>
        public SaySurroundingObjectsResult(object sender, IEnumerable<(DumpObject o, double compassDegrees)> objectInfo) : base(sender)
        => ObjectInfo = objectInfo;
    }
}
