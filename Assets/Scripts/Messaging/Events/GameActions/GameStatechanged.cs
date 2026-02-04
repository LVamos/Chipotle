namespace Game.Messaging.Events.GameManagement
{
	public class GameStatechanged : Message
	{
		public GameState Previous { get; }
		public GameState Current { get; }

		public GameStatechanged(GameState previous, GameState current) : base(null)
		{
			Previous = previous;
			Current = current;
		}
	}
}
