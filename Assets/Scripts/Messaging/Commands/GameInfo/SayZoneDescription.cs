namespace Game.Messaging.Commands.GameInfo
{
	/// <summary>
	/// Instructs a sound component of a character to read description of the current zone.
	/// </summary>
	public class SayZoneDescription : Message
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Souruce of the message</param>
		public SayZoneDescription(object sender) : base(sender)
		{
		}
	}
}