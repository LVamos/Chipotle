using Game.Terrain;

namespace Game.Messaging.Events
{
    internal class PassageDisappearedMessage : PassageShown
    {
        /// <summary>
        /// Constructs new instance of the message.
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="passage">Vanishing passage</param>
        public PassageDisappearedMessage(MessagingObject sender, Passage passage) : base(sender, passage) { }
    }
}
