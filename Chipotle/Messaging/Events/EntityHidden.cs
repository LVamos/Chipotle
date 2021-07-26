using Game.Entities;

namespace Game.Messaging.Events
{
    public class EntityHidden : EntityShown
    {
        /// <summary>
        /// Constructs new instance of the message.
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="newEntity">The entity which just disappeared</param>
        public EntityHidden(MessagingObject sender, Entity entity) : base(sender, entity) { }

    }
}
