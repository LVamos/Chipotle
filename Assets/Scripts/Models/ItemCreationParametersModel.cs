namespace Game.Models
{
	/// <summary>
	/// Describes parameters for item creation.
	/// </summary>
	public class ItemCreationParametersModel
	{
		/// <summary>
		/// The sound played on collision.
		/// </summary>
		public string CollisionSound { get; }

		/// <summary>
		/// The action sound.
		/// </summary>
		public string ActionSound { get; }

		/// <summary>
		/// The looping sound.
		/// </summary>
		public string LoopSound { get; }

		/// <summary>
		/// The cutscene description.
		/// </summary>
		public string Cutscene { get; }

		/// <summary>
		/// Indicates whether the item can be used only once.
		/// </summary>
		public bool UsableOnce { get; }

		/// <summary>
		/// Indicates whether the sound is audible over walls.
		/// </summary>
		public bool AudibleOverWalls { get; }

		/// <summary>
		/// The volume of the sound.
		/// </summary>
		public float Volume { get; }

		/// <summary>
		/// Indicates whether the sound should stop when the player moves.
		/// </summary>
		public bool StopWhenPlayerMoves { get; }

		/// <summary>
		/// Indicates whether quick actions are allowed with the item.
		/// </summary>
		public bool QuickActionsAllowed { get; }

		/// <summary>
		/// The sound played when picking up the item.
		/// </summary>
		public string PickingSound { get; }

		/// <summary>
		/// The sound played when placing the item.
		/// </summary>
		public string PlacingSound { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="collisionSound">Sound played on collision</param>
		/// <param name="actionSound">Sound played on interaction</param>
		/// <param name="loopSound">Sound played in loop</param>
		/// <param name="cutscene">A cutscene played on interaction</param>
		/// <param name="usableOnce">Indicates if the item can be used only once</param>
		/// <param name="audibleOverWalls">Indicates if sounds of the item can be heart over walls</param>
		/// <param name="volume">Vojlume for item sounds</param>
		/// <param name="stopWhenPlayerMoves">Indicates if the action soudn stops when the player moves</param>
		/// <param name="quickActionsAllowed">Indicates if the action sound can be played repeatedly in a short interval</param>
		/// <param name="pickingSound">Sound played on picking</param>
		/// <param name="placingSound">Sound played on placing</param>
		public ItemCreationParametersModel(
			string collisionSound = null,
			string actionSound = null,
			string loopSound = null,
			string cutscene = null,
			bool usableOnce = false,
			bool audibleOverWalls = true,
			float volume = 1,
			bool stopWhenPlayerMoves = false,
			bool quickActionsAllowed = false,
			string pickingSound = null,
			string placingSound = null)
		{
			CollisionSound = collisionSound;
			ActionSound = actionSound;
			LoopSound = loopSound;
			Cutscene = cutscene;
			UsableOnce = usableOnce;
			AudibleOverWalls = audibleOverWalls;
			Volume = volume;
			StopWhenPlayerMoves = stopWhenPlayerMoves;
			QuickActionsAllowed = quickActionsAllowed;
			PickingSound = pickingSound;
			PlacingSound = placingSound;
		}
	}
}