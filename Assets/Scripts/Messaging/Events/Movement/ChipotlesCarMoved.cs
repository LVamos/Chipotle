using Game.Entities.Items;
using Game.Terrain;



namespace Game.Messaging.Events.Movement
{
	/// <summary>
	/// Indicates that the Detective Chipotle's car object (detektivovo auto) has moved.
	/// </summary>
	/// <remarks>Sent from the <see cref="ChipotlesCar"/> class.</remarks>

	public class ChipotlesCarMoved : Message
	{
		/// <summary>
		/// The location to which the car moved
		/// </summary>
		public readonly Rectangle Target;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="target">The location to which the car moved</param>
		public ChipotlesCarMoved(ChipotlesCar sender, Rectangle target) : base(sender)
			=> Target = target;
	}
}