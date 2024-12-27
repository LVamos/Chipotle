namespace Game.UI
{
	/// <summary>
	/// Manages user interface, redistributes keyboard input to virtual windows
	/// </summary>
	public static class WindowHandler
	{
		/// <summary>
		/// Handles the KeyPress message.
		/// </summary>
		/// <param name="letter">The key that was pressed</param>
		public static void OnKeyPress(char letter)
			=> ActiveWindow.OnKeyPress(letter);

		/// <summary>
		/// Internal reference to currently active window
		/// </summary>
		private static VirtualWindow _activeWindow;

		/// <summary>
		/// Currently focused window
		/// </summary>
		public static VirtualWindow ActiveWindow { get => _activeWindow; private set => _activeWindow = value; }

		/// <summary>
		/// Reference to previously active window
		/// </summary>
		public static VirtualWindow PreviousWindow { get; private set; }

		/// <summary>
		/// Runs the main menu.
		/// </summary>
		public static void MainMenu() => Switch(MainMenuWindow.CreateInstance());

		/// <summary>
		/// Runs a voice menu
		/// </summary>
		/// <param name="items">Items for the menu</param>
		/// <param name="introText">Text to announce when menu is activated</param>
		/// <param name="wrappingAllowed">Enables or disables menu wrapping</param>
		/// <param name="introSound">Name of a sound to be played when the menu is activated</param>
		/// <param name="outroSound">Name of a sound to be played when menu is closed</param>
		/// <param name="selectionSound">Name of a sound to be played when user selects an item</param>
		/// <param name="wrapDownSound">Name of a sound to be played when the menu wraps to lower edge</param>
		/// <param name="wrapUpSound">Name of a sound to be played when the menu wraps to upper edge</param>
		/// <param name="upperEdgeSound">Name of a sound to be played when cursor gets to upper edge of the menu</param>
		/// <param name="lowerEdgeSound">Name of a sound to be played when cursor gets to lower edge of the menu</param>
		/// <returns>Tuple with index of selected item and value of selected item</returns>
		public static int Menu(MenuParametersDTO parameters)
		{
			var menu = MenuWindow.CreateInstance(parameters);

			OpenModalWindow(menu);
			return 0;
		}

		/// <summary>
		/// Delegates event to event handler of active window
		/// </summary>
		/// <param name="shortcut">Event parameters</param>
		public static void OnKeyDown(KeyShortcut shortcut)
			=> ActiveWindow?.OnKeyDown(shortcut);

		/// <summary>
		/// Sends the KeyUp event to the current active window.
		/// </summary>
		/// <param name="e">Event parameters</param>
		public static void OnKeyUp(KeyShortcut shortcut)
			=> ActiveWindow?.OnKeyUp(shortcut);

		/// <summary>
		/// Opens virtual modal window
		/// </summary>
		/// <param name="modalWindow">Reference to new window</param>
		public static void OpenModalWindow(VirtualWindow modalWindow)
		{
			PreviousWindow = ActiveWindow;
			_activeWindow = modalWindow;
			_activeWindow.ParentWindow = PreviousWindow;
			_activeWindow.OnActivate();
		}

		/// <summary>
		/// Begins the game.
		/// </summary>
		public static void StartGame()
		{
			Switch(GameWindow.CreateInstance());
			World.StartGame();
		}

		/// <summary>
		/// Closes currently active window and activates another one.
		/// </summary>
		/// <param name="window">New window</param>
		public static void Switch(VirtualWindow window)
		{
			_activeWindow?.OnDeactivate(); // Let active window react on deactivating
			PreviousWindow = ActiveWindow; // Backing up for future use
			_activeWindow = window;
			ActiveWindow.OnActivate(); // Let new window react on activation
		}
	}
}