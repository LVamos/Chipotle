using Game.Terrain;

using System;

namespace Game.Messaging.Commands
{
    /// <summary>
    /// Relocates the Detective's car (detektivovo auto) to the specified locality.
    /// </summary>
    /// <remarks>Applies to the <see cref="Game.Entities.ChipotlesCar"/> class.</remarks>
    [Serializable]
    public class MoveChipotlesCar : GameMessage
    {
        /// <summary>
        /// The locality to which the car will move.
        /// </summary>
        public readonly Locality Destination;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="destination">The locality to which the car will move</param>
        public MoveChipotlesCar(object sender, Locality destination) : base(sender) => Destination = destination;
    }
}