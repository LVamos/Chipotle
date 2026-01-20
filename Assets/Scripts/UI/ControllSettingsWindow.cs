using DavyKager;

using Game.Controls;
using Game.Controls.DualSense;
using Game.Controls.Keyboard;

using System;
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
			_commands = new();
			Type enumType = typeof(CommandId);
			List<CommandId> ids = Enum.GetValues(enumType)
				.Cast<CommandId>()
				.ToList();

			foreach (CommandId id in ids)
			{
				string name = InputConfig.GetCommandName(id);
				_commands[name] = id;
			}
		}

		private Dictionary<string, CommandId> _commands;

		public override void OnActivate()
		{
			base.OnActivate();

			SelectCommandMenu();
		}

		private void SelectCommandMenu()
		{
			const string prompt = "Příkazy";
			List<List<string>> items =
				_commands.Keys
				.Select(command => new List<string>() { command })
				.ToList();

			Action<int> menuHandler = (option) =>
			{
				if (option == -1)
					HandleSelectCommandMenu();
				else HandleSelectCommandMenu(items[option][0]);
			};
			MenuParameters parameters = new(
				items,
				prompt,
				" ",
				0,
				false,
												introSound: "MenuItemActivated",
								outroSound: "MenuOpened",
		selectionSound: "MenuItemSelected",
		wrapDownSound: "MenuWrapped",
		wrapUpSound: "MenuWrapped",
		upperEdgeSound: "MenuEdge",
		lowerEdgeSound: "MenuEdge",
				menuClosed: menuHandler);

			WindowHandler.Menu(parameters, false);
		}

		private void HandleSelectCommandMenu(string commandName = null)
		{
			if (string.IsNullOrEmpty(commandName))
			{
				WindowHandler.Switch(_mainMenu);
				return;
			}

			BindingMenu(commandName);
		}

		private enum BindingAction
		{
			SetKeyboardBinding = 0,
			RemoveKeyboardBinding = 1,
			SetDualSenseBinding = 2,
			RemoveDualSenseBinding = 3,
			Cancel = -1
		}

		private void BindingMenu(string commandName)
		{
			List<List<string>> items = new()
			{
			new List<string>() { "Nastavit klávesovou zkratku" },
			new List<string>() { "Zrušit klávesovou zkratku" },
			new List<string>() { "Nastavit zkratku pro Dual sense tlačítko" },
			new List<string>() { "Zrušit zkratku pro Dual sense tlačítko" }
			};

			Action<int> menuHandler = (option) =>
			{
				HandleSetBindingsMenu((BindingAction)option, _commands[commandName]);
			};

			MenuParameters parameters = new(
				items,
				wrappingAllowed: false,
												introSound: "MenuItemActivated",
								outroSound: "MenuOpened",
		selectionSound: "MenuItemSelected",
		wrapDownSound: "MenuWrapped",
		wrapUpSound: "MenuWrapped",
		upperEdgeSound: "MenuEdge",
		lowerEdgeSound: "MenuEdge",
				menuClosed: menuHandler);
			WindowHandler.Menu(parameters, false);
		}

		private void HandleSetBindingsMenu(BindingAction action, CommandId command)
		{
			switch (action)
			{
				case BindingAction.SetKeyboardBinding: SetKeyboardBinding(command); break;
				case BindingAction.RemoveKeyboardBinding: RemoveKeyboardBinding(command); break;
				case BindingAction.SetDualSenseBinding: SetDualSenseBinding(command); break;
				case BindingAction.RemoveDualSenseBinding:
					RemoveDualSenseBinding(command); break;
				case BindingAction.Cancel: SelectCommandMenu(); break;
			}
		}

		private void RemoveDualSenseBinding(CommandId command)
		{
			InputConfig.RemoveDualsenseBinding(command);
			Tolk.Speak("Zrušeno");
		}

		private void SetDualSenseBinding(CommandId command)
		{
			DualSenseInput? shortcut = WindowHandler.CatchDualSenseShortcut();
			if (shortcut == null)
			{
				BindingMenu(InputConfig.GetCommandName(command));
				return;
			}

			bool result = InputConfig.SetDualSenseBinding(command, shortcut.Value);
			if (!result)
				AnnounceBlockedShortcut(shortcut.Value);
			else Tolk.Speak("Nastaveno.");
		}

		private void RemoveKeyboardBinding(CommandId command)
		{
			InputConfig.RemoveKeyboardBinding(command);
			Tolk.Speak("Zrušeno");
		}

		private void KeyboardBindingFinished(KeyboardBindingResult result)
		{
			if (result.ShortcutAlreadyUsed)
				AnnounceBlockedShortcut(result.Shortcut.Value);
			else Tolk.Speak("Nastaveno");

			string commandName = InputConfig.GetCommandName(result.Command);
			BindingMenu(commandName);
		}

		private void SetKeyboardBinding(CommandId command)
		{
			Tolk.Speak("Zadej klávesovou zkratku");
			InputConfig.StartKeyboardBinding(command, KeyboardBindingFinished);
		}

		private static void AnnounceBlockedShortcut(DualSenseInput shortcut)
		{
			CommandId blockingCommand = InputConfig.GetCommandByShortcut(shortcut);
			string blockingCommandName = InputConfig.GetCommandName(blockingCommand);
			string message = $"Tuhle zkratku už máš nastavenou pro příkaz {blockingCommandName}.";
			Tolk.Speak(message);
		}

		private static void AnnounceBlockedShortcut(KeyboardInput shortcut)
		{
			CommandId blockingCommand = InputConfig.GetCommandByShortcut(shortcut);
			string blockingCommandName = InputConfig.GetCommandName(blockingCommand);
			string message = $"Tuhle zkratku už máš nastavenou pro příkaz {blockingCommandName}.";
			Tolk.Speak(message);
		}
	}
}
