namespace Game.Messaging.Events.GameManagement
{
	/// <summary>
	/// Indicates that the game state was loaded from file.
	/// </summary>
	public class Reloaded : Message
	{
		public Reloaded() : base(null) { }
	}
}