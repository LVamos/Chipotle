using System;
using System.Collections.Generic;

namespace Game.Messaging.Commands
{
    /// <summary>
    /// Instructs the NPC to report names of the closest objects in its surroundings.
    /// </summary>
    /// <remarks>
    /// Applies to the <see cref="Game.Entities.Entity"/> class. Can be sent only from inside the
    /// NPC from a descendant of the <see cref="Game.Entities.EntityComponent"/> class.
    /// </remarks>
    [Serializable]
    public class SayNearestObjects : GameMessage
    {
        /// <summary>
        /// Indicates that no objects were found.
        /// </summary>
        public readonly bool NothingFound;

        /// <summary>
        /// Information about the nearest objects
        /// </summary>
        public readonly IEnumerable<(string friendlyName, double compassDegrees)> ObjectInfo;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        public SayNearestObjects(object sender) : base(sender) { }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Sorce of the message</param>
        /// <param name="objectInfo">Information about the nearest objects</param>
        public SayNearestObjects(object sender, IEnumerable<(string friendlyName, double compassDegrees)> objectInfo) : this(sender)
        => ObjectInfo = objectInfo;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Sorce of the message</param>
        /// <param name="nothingFound">Indicates that no objects were found</param>
        public SayNearestObjects(object sender, bool nothingFound) : this(sender)
        => NothingFound = nothingFound;
    }
}