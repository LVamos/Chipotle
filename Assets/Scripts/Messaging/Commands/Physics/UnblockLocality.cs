using System;
using Game.Terrain;

namespace Game.Messaging.Commands.Physics
{
	/// <summary>
	/// Tells the Detective Chipotle's car (detektivovo auto) to make the specified locality
	/// accessible for the player.
	/// </summary>
	/// <remarks>Applies only to the <see cref="Entities.Items.ChipotlesCar"/> class.</remarks>
	[Serializable]
	public class UnblockLocality : Message
	{
		/// <summary>
		/// The locality to be unblocked
		/// </summary>
		public readonly Locality Locality;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="locality">The locality to be unblocked</param>
		public UnblockLocality(object sender, Locality locality) : base(sender) => Locality = locality;
	}
}