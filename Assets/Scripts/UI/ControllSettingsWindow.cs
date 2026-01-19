using DavyKager;

using Game.Controls;

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

			SetBindingsMenu(commandName);
		}

		private enum BindingAction
		{
			SetKeyboardBinding = 0,
			RemoveKeyboardBinding = 1,
			SetDualSenseBinding = 2,
			RemoveDualSenseBinding = 3,
			Cancel = -1
		}

		private void SetBindingsMenu(string commandName)
		{
			string prompt = $"Nastavení zkratek pro příkaz {commandName}";
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
			Tolk.Speak("Vymaž zkratku pro Dual sense");
		}

		private void SetDualSenseBinding(CommandId command)
		{
			Tolk.Speak("Nastav zkratku pro Dual sense");
		}

		private void RemoveKeyboardBinding(CommandId command)
		{
			Tolk.Speak("Vymaž klávesovou zkratku");
		}

		private void SetKeyboardBinding(CommandId command)
		{
			Tolk.Speak("Nastav klávesovou zkratku");
		}
	}
}
