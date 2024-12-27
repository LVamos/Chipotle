using System;

namespace Game.Messaging.Commands
{
    /// <summary>
    /// Instructs the NPC to report names of the closest objects in its surroundings.
    /// </summary>
    /// <remarks>
    /// Applies to the <see cref="Game.Entities.Character"/> class. Can be sent only from inside the
    /// NPC from a descendant of the <see cref="Game.Entities.CharacterComponent"/> class.
    /// </remarks>
    [Serializable]
    public class SayObjects : GameMessage
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        public SayObjects(object sender) : base(sender) { }
    }
}