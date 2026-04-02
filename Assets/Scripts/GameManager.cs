using Game.Messaging.Events.GameManagement;

namespace Game
{
	public static class GameManager
	{
		private static GameState _state = GameState.None;

		public static GameState State
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
			State = GameState.Finished;
		}

		public static void StartGame()
			=> State = GameState.Playing;

		public static void LoadGame()
			=> State = GameState.Loading;

		public static void PauseGame()
	=> State = GameState.Paused;

		public static void ResumeGame()
=> State = GameState.Playing;

		private static void AnnounceStatechange(GameState previous, GameState current)
		{
			GameStatechanged message = new(previous, current);
			World.TakeMessage(message);
		}
	}
}
