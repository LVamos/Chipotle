namespace Game.Messaging.Events.Characters.Movement
{
	public class SonarToggled : Message
	{
		public readonly bool SonarEnabled;
		public readonly bool Announce;

		public SonarToggled(object sender, bool enabled, bool announce = true) : base(sender)
		{
			SonarEnabled = enabled;
			Announce = announce;
		}
	}
}
