using System;
using Game.Terrain;

namespace Game.Messaging.Commands.Physics
{
	/// <summary>
	/// Tells the Detective Chipotle's car (detektivovo auto) to make the specified zone
	/// accessible for the player.
	/// </summary>
	/// <remarks>Applies only to the <see cref="Entities.Items.ChipotlesCar"/> class.</remarks>
	[Serializable]
	public class UnblockZone : Message
	{
		/// <summary>
		/// The zone to be unblocked
		/// </summary>
		public readonly Zone Zone;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="zone">The zone to be unblocked</param>
		public UnblockZone(object sender, Zone zone) : base(sender) => Zone = zone;
	}
}