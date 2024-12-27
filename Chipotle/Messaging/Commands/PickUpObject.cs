using Game.Entities;

namespace Game.Messaging.Commands
{
    /// <summary>
    /// Tells an entity to pick up an object off the ground.
    /// </summary>
    public class PickUpObject : GameMessage
    {
        /// <summary>
        /// Source of the message (Entity or EntityComponent).
        /// </summary>
        public new readonly MessagingObject Sender;

        /// <summary>
        /// The object that should be picked.
        /// </summary>
        public readonly Item Object;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message (Entity or EntityComponent)</param>
        /// <param name="object">The object that should be picked</param>
        public PickUpObject(MessagingObject sender, Item @object = null) : base(sender)
        {
            Sender = sender;
            Object = @object;
        }
    }
}
