namespace Game.Messaging.Commands.GameInfo
{
	/// <summary>
	/// Instructs a sound component of a character to read description of the current locality.
	/// </summary>
	public class SayLocalityDescription : Message
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Souruce of the message</param>
		public SayLocalityDescription(object sender) : base(sender)
		{
		}
	}
}