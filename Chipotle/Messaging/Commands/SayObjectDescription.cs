using Game.Entities;

using System;

namespace Game.Messaging.Commands
{
    /// <summary>
    /// Instructs a soudn component of a character to read description of the specified object or character.
    /// </summary>
    public class SayObjectDescription : GameMessage
    {
        /// <summary>
        /// The object whose description should be read.
        /// </summary>
        public readonly GameObject Object;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="object">The object whose description should be read.</param>
        /// <remarks>If Object is null then the sound component should announce that there's no object to be described.</remarks>
        public SayObjectDescription(object sender, GameObject @object) : base(sender) => Object = @object ?? throw new ArgumentNullException(nameof(@object));
    }
}
