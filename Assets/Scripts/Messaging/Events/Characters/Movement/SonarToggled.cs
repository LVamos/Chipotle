namespace Game.Messaging.Events.Characters.Movement
{
	public class SonarToggled : Message
	{
		public readonly bool SonarEnabled;

		public SonarToggled(object sender, bool enabled) : base(sender)
			=> SonarEnabled = enabled;
	}
}
