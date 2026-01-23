using DavyKager;

using Game.Controls;

using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace Game.UI
{
	public class ControllSettingsWindow : VirtualWindow
	{
		public static ControllSettingsWindow CreateInstance(MainMenuWindow mainMenu)
		{
			GameObject obj = new(nameof(ControllSettingsWindow));
			ControllSettingsWindow window = obj.AddComponent<ControllSettingsWindow>();
			window.Initialize(mainMenu);
			return window;
		}


		public void Initialize(MainMenuWindow mainMenu)
		{
			base.Initialize();
			CollectCommands();
			_mainMenu = mainMenu;
		}

		private MainMenuWindow _mainMenu;

		private void CollectCommands()
		{
			_commands = InputConfig.GetBindableCommands();
		}

		private Dictionary<CommandId, CommandBindings> _commands;

		public override void OnActivate()
		{
			base.OnActivate();

			SelectCommandMenu();
		}

		private void SelectCommandMenu(CommandId? command = null)
		{
			int index = command != null ? GetCommandIndex(command.Value) : 0;

			const string prompt = "Příkazy";
			List<List<string>> items =
				_commands
				.Select(command => CreateItem(command))
				.ToList();

			MenuParameters parameters = new(
				items,
				prompt,
				" ",
				0,
				false,
				defaultIndex: index,
												introSound: "MenuItemActivated",
								outroSound: "MenuOpened",
		selectionSound: "MenuItemSelected",
		wrapDownSound: "MenuWrapped",
		wrapUpSound: "MenuWrapped",
		upperEdgeSound: "MenuEdge",
		lowerEdgeSound: "MenuEdge",
				menuClosed: SelectCommandMenuHandler);
			WindowHandler.Menu(parameters, false);

			List<string> CreateItem(KeyValuePair<CommandId, CommandBindings> command)
			{
				string name = InputConfig.GetCommandName(command.Key);
				string keyboard = command.Value.Keyboard.ToString();
				string dualSense = command.Value.DualSense.ToString();

				return new() { name, keyboard, dualSense };
			}
		}

		private int GetCommandIndex(CommandId command)
		{
			return _commands.Keys
								.ToList()
								.IndexOf(command);
		}

		private void SelectCommandMenuHandler(int option)
		{
			if (option == -1)
				WindowHandler.Switch(_mainMenu);
			else BindingMenu(GetCommandByIndex(option));
		}

		private CommandId GetCommandByIndex(int option)
		{
			return _commands.Keys.ToArray()[option];
		}

		private enum BindingAction
		{
			SetKeyboardBinding = 0,
			RemoveKeyboardBinding = 1,
			SetDualSenseBinding = 2,
			RemoveDualSenseBinding = 3,
			Cancel = -1
		}

		private List<List<string>> _bindingMenuItems = new()
			{
			new () { "Nastavit klávesovou zkratku" },
			new () { "Vrátit výchozí klávesovou zkratku" },
			new () { "Nastavit kombinaci tlačítek pro Dual sense" },
			new () { "Vrátit výchozí kombinaci tlačítek pro Dual sense" }
			};

		private void BindingMenu(CommandId command, int index = 0)
		{
			MenuParameters parameters = new(
				_bindingMenuItems,
				wrappingAllowed: false,
												introSound: "MenuItemActivated",
								outroSound: "MenuOpened",
		selectionSound: "MenuItemSelected",
		wrapDownSound: "MenuWrapped",
		wrapUpSound: "MenuWrapped",
		upperEdgeSound: "MenuEdge",
		lowerEdgeSound: "MenuEdge",
		defaultIndex: index,
				menuClosed: (option) => BindingMenuHandler((BindingAction)option, command));
			WindowHandler.Menu(parameters, false);
		}

		private void BindingMenuHandler(BindingAction action, CommandId command)
		{
			switch (action)
			{
				case BindingAction.SetKeyboardBinding: SetKeyboardBinding(command); break;
				case BindingAction.RemoveKeyboardBinding: RestoreKeyboardBinding(command); break;
				case BindingAction.SetDualSenseBinding: SetDualSenseBinding(command); break;
				case BindingAction.RemoveDualSenseBinding:
					RestoreDualSenseBinding(command); break;
				case BindingAction.Cancel: SelectCommandMenu(command); break;
			}
		}

		private void RestoreDualSenseBinding(CommandId command)
		{
			InputConfig.RestoreDualsenseBinding(command);
			Tolk.Speak("Obnoveno");
			BindingMenu(command, 3);
		}

		private void SetDualSenseBinding(CommandId command)
		{
			Tolk.Speak("Zadej kombinaci tlačítek");
			InputConfig.StartDualSenseBinding(command, DualSenseBindingFinished);
		}

		private void RestoreKeyboardBinding(CommandId command)
		{
			InputConfig.RestoreKeyboardBinding(command);
			Tolk.Speak("Obnoveno");
			BindingMenu(command, 1);
		}

		private void KeyboardBindingFinished(BindingResult result)
		{
			if (result.CommandWithSameShortcut != null)
				AnnounceBlockedShortcut(result.CommandWithSameShortcut.Value);
			else if (result.Success)
				Tolk.Speak("Nastaveno");
			BindingMenu(result.Command);
		}

		private void DualSenseBindingFinished(BindingResult result)
		{
			if (result.CommandWithSameShortcut != null)
				AnnounceBlockedShortcut(result.CommandWithSameShortcut.Value);
			else if (result.Success)
				Tolk.Speak("Nastaveno");
			BindingMenu(result.Command, 2);
		}

		private void SetKeyboardBinding(CommandId command)
		{
			Tolk.Speak("Zadej klávesovou zkratku");
			InputConfig.StartKeyboardBinding(command, KeyboardBindingFinished);
		}

		private static void AnnounceBlockedShortcut(CommandId blockingCommand)
		{
			string blockingCommandName = InputConfig.GetCommandName(blockingCommand);
			string message = $"Tuhle zkratku už máš nastavenou pro příkaz {blockingCommandName}.";
			Tolk.Speak(message);
		}
	}
}
