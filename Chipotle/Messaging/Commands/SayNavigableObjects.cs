﻿using System;
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
    public class SayNavigableObjects : GameMessage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        public SayNavigableObjects(object sender) : base(sender) { }
    }
}