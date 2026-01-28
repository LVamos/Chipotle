namespace Game.Messaging.Commands.Characters
{
	/// <summary>
	/// Instructs a physics component of a character to use an object.
	/// </summary>
	/// <remarks>When Object is null, the NPC finds usable objects.</remarks>
	public class Interact : Message
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source o the message</param>
		public Interact(object sender) : base(sender)
		{
		}
	}
}