using Game.Controls.DualSense;
using Game.Controls.Keyboard;
using Game.Serialization;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine.InputSystem;

namespace Game.Controls
{
	public static class InputConfig
	{
		public static string GetCommandName(CommandId command)
		{
			return _commandNames[command];
		}

		public static bool SetKeyboardBinding(CommandId command, KeyboardInput shortcut)
		{
			if (!IsBindable(command))
				throw new InvalidOperationException($"Command {command} is not configurable.");

			Dictionary<CommandId, CommandBindings> bindables = GetBindableCommands();
			if (bindables.Values.Any(b => b.Keyboard == shortcut))
				return false;

			_commands[command].Keyboard = shortcut;
			SaveBindings();
			return true;
		}

		public static void RemoveKeyboardBinding(CommandId command)
		{
			_commands[command].Keyboard = null;
			SaveBindings();
		}

		public static void RemoveDualsenseBinding(CommandId command)
		{
			_commands[command].DualSense = null;
			SaveBindings();
		}

		public static bool SetDualSenseBinding(CommandId command, DualSenseInput shortcut)
		{
			if (_commands.Values.Any(b => b.DualSense == shortcut))
				return false;

			_commands[command].DualSense = shortcut;
			SaveBindings();
			return true;
		}

		public static Dictionary<CommandId, CommandBindings> GetBindableCommands()
		{
			return _commands
				.Where(record => _bindableCommands.Contains(record.Key))
				.ToDictionary(record => record.Key, record => record.Value);
		}

		private static void SaveBindings()
		{
			try
			{
				string path = MainScript.UserInputPath;

				Dictionary<string, YamlCommandBindings> data = GetBindableCommands()
					.ToDictionary(
						kv => kv.Key.ToString(),
						kv => new YamlCommandBindings
						{
							Keyboard = kv.Value.Keyboard?.ToString(),
							DualSense = kv.Value.DualSense?.ToString()
						}
					);

				YamlHelper.SaveToFile(path, data);
			}
			catch (Exception e)
			{
				Logger.LogError("Nepodařilo se uložit uživatelské mapování vstupů.", e.ToString());
			}
		}

		public static CommandBindings GetBindings(CommandId command)
		{
			CommandBindings bindings = null;
			_commands.TryGetValue(command, out bindings);
			return bindings;
		}

		private static void LoadDefaultBindings()
		{
			_commands = new();
			string path = MainScript.DefaultInputMappingPath;

			try
			{
				Dictionary<string, YamlCommandBindings> map = null;
				YamlHelper.LoadFromResources(path, out map);
				AddBindings(map);
			}
			catch (Exception e)
			{
				Logger.LogError("Chyba při načítání mapování vstupů", e.ToString());
				throw;
			}
		}

		private static void LoadCommandNames()
		{
			_commandNames = new();
			string path = MainScript.CommandNamesPath;

			try
			{
				Dictionary<string, string> map = null;
				YamlHelper.LoadFromResources(path, out map);

				foreach (KeyValuePair<string, string> record in map)
				{
					CommandId command;
					string key = record.Value;
					if (!Enum.TryParse<CommandId>(record.Key, out command))
						throw new InvalidOperationException($"Unable to parse CommandId from YAML: {key}");
					_commandNames[command] = key
						?? throw new ArgumentNullException($"Missing display name for {key} command.");
				}
			}
			catch (Exception e)
			{
				Logger.LogError("Chyba při načítání mapování vstupů", e.ToString());
				throw;
			}
		}

		public static void LoadBindings()
		{
			LoadDefaultBindings();
			LoadUserBindings();
			LoadCommandNames();
		}

		private static void LoadUserBindings()
		{
			string path = MainScript.UserInputPath;
			if (!File.Exists(path))
				return;

			try
			{
				Dictionary<string, YamlCommandBindings> map = null;
				YamlHelper.LoadFromFile(path, out map);
				AddBindings(map);
			}
			catch (Exception e)
			{
				Logger.LogError("Chyba při načítání mapování vstupů", e.ToString());
			}
		}

		private static void AddBindings(Dictionary<string, YamlCommandBindings> map)
		{
			foreach (KeyValuePair<string, YamlCommandBindings> pair in map)
			{
				CommandId command;
				if (!Enum.TryParse<CommandId>(pair.Key, out command))
					continue;

				// Add keyboard input to dictionary if present
				KeyboardInput? keyboard = null;
				DualSenseInput? dualSense = null;
				if (!string.IsNullOrEmpty(pair.Value.Keyboard))
					keyboard = new KeyboardInput(pair.Value.Keyboard);
				if (!string.IsNullOrEmpty(pair.Value.DualSense))
					dualSense = new(pair.Value.DualSense);

				if (keyboard == null && dualSense == null)
					continue;

				CommandBindings bindings = new(keyboard, dualSense);
				_commands[command] = bindings;
			}

		}

		public static CommandId GetBindableCommand(KeyboardInput shortcut)
		{
			var value = (KeyboardInput?)shortcut;
			Dictionary<CommandId, CommandBindings> bindables = GetBindableCommands();
			return bindables
				.First(record => record.Value.Keyboard == value).Key;
		}

		public static CommandId GetBindableCommand(DualSenseInput shortcut)
		{
			var value = (DualSenseInput?)shortcut;
			return _commands.First(record => record.Value.DualSense == value).Key;
		}

		public static void StartKeyboardBinding(CommandId command, Action<KeyboardBindingResult> callback)
		{
			if (!IsBindable(command))
				throw new InvalidOperationException($"Command {command} is not configurable.");

			_rebindedCommand = command;
			_keyboardBindingFinished = callback;
			KeyboardRebinding = true;
		}

		private static bool IsBindable(CommandId command)
		{
			return _bindableCommands.Contains(command);
		}

		public static void FinishKeyboardBinding(KeyboardInput shortcut)
		{
			Action<KeyboardBindingResult> callback = _keyboardBindingFinished;
			_keyboardBindingFinished = null;
			CommandId command = _rebindedCommand.Value;
			_rebindedCommand = null;
			KeyboardRebinding = false;

			KeyboardBindingResult result = null;
			if (shortcut == new KeyboardInput(Key.Escape))
			{
				result = new(command);
				callback(result);
				return;
			}

			bool success = SetKeyboardBinding(command, shortcut);
			result = new(command, shortcut, !success);
			callback(result);
		}

		private static CommandId? _rebindedCommand;

		private static Action<KeyboardBindingResult> _keyboardBindingFinished;

		private static Dictionary<CommandId, CommandBindings> _commands;
		private static HashSet<CommandId> _bindableCommands = new()
		{
					CommandId.GamePlaceItem,
		CommandId.GameApplyItemToItem,
		CommandId.GameSendFeedback,
		CommandId.GameSayAbsoluteCoordinates,
CommandId.      GameLoadPredefinedSave,
		CommandId.GameCreatePredefinedSave,
		CommandId.GameSayCharacters,
		CommandId.GameListCharacters,
		CommandId.GameSayNavigatedObjectLocation,
		CommandId.GameExploreItem,
		CommandId.GameSayZoneDescription,
		CommandId.GameInventoryMenu,
		CommandId.GamePickUpItem,
		CommandId.GameMenu,
		CommandId.GameSayZoneSize,
		CommandId.GameListExits,
		CommandId.GameListItems,
		CommandId.GameSayOrientation,
		CommandId.GameSayExits,
		CommandId.GameStopCutscene,
		CommandId.GameTerrainInfo,
		CommandId.GameSayVisitedRegion,
		CommandId.GameGoLeft,
		CommandId.GameGoRight,
		CommandId.GameSayItems,
		CommandId.GameSayZoneName,
		CommandId.GameGoForward,
		CommandId.GameGoBack,
		CommandId.GameTurnLeft,
		CommandId.GameTurnRight,
		CommandId.GameTurnSharplyLeft,
		CommandId.GameTurnSharplyRight,
		CommandId.GameTurnAround,
		CommandId.GameInteract,
		CommandId.GameQuit
		};

		private static Dictionary<CommandId, string> _commandNames;

		public static bool KeyboardRebinding { get; private set; }
	}
}
