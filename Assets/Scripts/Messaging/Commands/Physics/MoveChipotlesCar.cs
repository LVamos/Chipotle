using System;
using Game.Terrain;

namespace Game.Messaging.Commands.Physics
{
	/// <summary>
	/// Relocates the Detective's car (detektivovo auto) to the specified zone.
	/// </summary>
	/// <remarks>Applies to the <see cref="Entities.Items.ChipotlesCar"/> class.</remarks>
	[Serializable]
	public class MoveChipotlesCar : Message
	{
		/// <summary>
		/// The zone to which the car will move.
		/// </summary>
		public readonly Zone Destination;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="destination">The zone to which the car will move</param>
		public MoveChipotlesCar(object sender, Zone destination) : base(sender) => Destination = destination;
	}
}