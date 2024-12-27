using Game.Entities.Items;
using Game.Terrain;

using ProtoBuf;

namespace Game.Messaging.Events.Movement
{
    /// <summary>
    /// Indicates that the Detective Chipotle's car object (detektivovo auto) has moved.
    /// </summary>
    /// <remarks>Sent from the <see cref="ChipotlesCar"/> class.</remarks>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public class ChipotlesCarMoved : Message
    {
        /// <summary>
        /// The location to which the car moved
        /// </summary>
        public readonly Terrain.Rectangle Target;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="target">The location to which the car moved</param>
        public ChipotlesCarMoved(ChipotlesCar sender, Rectangle target) : base(sender)
            => Target = target;
    }
}