namespace Game.Messaging.Events.GameManagement
{
	public class GameMenuOptionselected : Message
	{
		public readonly string OptionId;

		public GameMenuOptionselected(object sender, string optionId) : base(sender)
			=> OptionId = optionId;
	}
}
