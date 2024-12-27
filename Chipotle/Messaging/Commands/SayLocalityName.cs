using System;

namespace Game.Messaging.Commands
{
    [Serializable]
    public class SayLocalityName : GameMessage
    {
        /// <summary>
        /// Instructs the NPC to report the name of the locality in which it is currently located.
        /// </summary>
        /// <remarks>
        /// Applies to the <see cref="Game.Entities.Character"/> class. Can be sent only from inside
        /// the NPC from a descendant of the <see cref="Game.Entities.CharacterComponent"/> class.
        /// </remarks>
        public SayLocalityName(object sender) : base(sender)
        {
        }
    }
}