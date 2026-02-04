using Game.Messaging.Events.GameManagement;

namespace Game
{
	public static class GameManager
	{
		private static GameState _state;

		public static GameState state
		{
			get => _state;
			private set
			{
				GameState previous = _state;
				_state = value;
				AnnounceStatechange(previous, value);
			}
		}

		public static void FinishGame()
		{
			state = GameState.Finished;
		}

		public static void StartGame()
		{
			state = GameState.Playing;
		}

		private static void AnnounceStatechange(GameState previous, GameState current)
		{
			GameStatechanged message = new(previous, current);
			World.TakeMessage(message);
		}
	}
}
