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

		public static CommandId? SetKeyboardBinding(CommandId command, KeyboardInput shortcut)
		{
			if (!IsBindable(command))
				throw new InvalidOperationException($"Command {command} is not configurable.");

			Dictionary<CommandId, CommandBindings> bindables = GetBindableCommands();
			List<CommandId> blockingCommands = GetBindableCommands(shortcut)
				.Where(c => c != command)
				.ToList();
			if (blockingCommands != null && blockingCommands.Count > 0)
				return blockingCommands.First();

			_commands[command].Keyboard = shortcut;
			SaveBindings();
			return null;
		}

		public static void RestoreKeyboardBinding(CommandId command)
		{
			CommandBindings binding = _commands[command];
			CommandBindings defaultBinding = _defaultBindings[command];
			binding.Keyboard = defaultBinding.Keyboard;
			SaveBindings();
		}

		public static void RestoreDualsenseBinding(CommandId command)
		{
			CommandBindings binding = _commands[command];
			CommandBindings defaultBinding = _defaultBindings[command];
			binding.DualSense = defaultBinding.DualSense;
			SaveBindings();
		}

		public static CommandId? SetDualSenseBinding(CommandId command, DualSenseInput shortcut)
		{
			if (!IsBindable(command))
				throw new InvalidOperationException($"Command {command} is not configurable.");

			Dictionary<CommandId, CommandBindings> bindables = GetBindableCommands();
			List<CommandId> blockingCommands = GetBindableCommands(shortcut)
				.Where(c => c != command)
				.ToList();
			if (blockingCommands != null && blockingCommands.Count > 0)
				return blockingCommands.First();

			_commands[command].DualSense = shortcut;
			SaveBindings();
			return null;
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

				// Duplicate bindings
				_defaultBindings =
					_commands.ToDictionary(
						kv => kv.Key,
						kv => new CommandBindings(
							kv.Value.Keyboard,
							kv.Value.DualSense
						)
					);
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

		public static List<CommandId> GetBindableCommands(KeyboardInput shortcut)
		{
			var value = (KeyboardInput?)shortcut;
			Dictionary<CommandId, CommandBindings> bindables = GetBindableCommands();
			return bindables
				.Where(record => record.Value.Keyboard == value)
				.Select(record => record.Key)
				.ToList();
		}

		public static List<CommandId> GetBindableCommands(DualSenseInput shortcut)
		{
			var value = (DualSenseInput?)shortcut;
			Dictionary<CommandId, CommandBindings> bindables = GetBindableCommands();
			return bindables
				.Where(record => record.Value.DualSense == value)
				.Select(record => record.Key)
				.ToList();
		}

		public static void StartKeyboardBinding(CommandId command, Action<BindingResult> callback)
		{
			if (!IsBindable(command))
				throw new InvalidOperationException($"Command {command} is not configurable.");

			_rebindedCommand = command;
			_keyboardBindingFinished = callback;
			KeyboardRebinding = true;
		}

		public static void StartDualSenseBinding(CommandId command, Action<BindingResult> callback)
		{
			if (!IsBindable(command))
				throw new InvalidOperationException($"Command {command} is not configurable.");

			_rebindedCommand = command;
			_dualSenseBindingFinished = callback;
			DualSenseRebinding = true;
		}

		private static bool IsBindable(CommandId command)
		{
			return _bindableCommands.Contains(command);
		}

		public static void FinishKeyboardBinding()
		{
			Action<BindingResult> callback = _keyboardBindingFinished;
			_keyboardBindingFinished = null;
			CommandId command = _rebindedCommand.Value;
			_rebindedCommand = null;
			KeyboardRebinding = false;

			BindingResult result = null;
			if (_bindingKeyboardShortcut == null)
			{
				result = new(command, false);
				callback(result);
				return;
			}

			CommandId? blockingCommand = SetKeyboardBinding(command, _bindingKeyboardShortcut.Value);
			result = new(command, blockingCommand == null, blockingCommand);
			callback(result);
		}

		public static void FinishDualSenseBinding()
		{
			Action<BindingResult> callback = _dualSenseBindingFinished;
			_dualSenseBindingFinished = null;
			CommandId command = _rebindedCommand.Value;
			_rebindedCommand = null;
			DualSenseRebinding = false;

			BindingResult result = null;
			if (_bindingDualSenseShortcut == null)
			{
				result = new(command, false);
				callback(result);
				return;
			}

			CommandId? blockingCommand = SetDualSenseBinding(command, _bindingDualSenseShortcut.Value);
			result = new(command, blockingCommand == null, blockingCommand);
			callback(result);
		}

		private static KeyboardInput? _bindingKeyboardShortcut;

		public static void CatchKeysForBinding(KeyboardInput shortcut)
		{
			if (shortcut == new KeyboardInput(Key.Escape))
			{
				_bindingKeyboardShortcut = null;
				FinishKeyboardBinding();
				return;
			}

			_bindingKeyboardShortcut = shortcut;
		}

		public static void CatchKeysForBinding(DualSenseInput shortcut)
		{
			if (shortcut == new DualSenseInput("Circle"))
			{
				_bindingDualSenseShortcut = null;
				FinishDualSenseBinding();
				return;
			}

			_bindingDualSenseShortcut = shortcut;
		}

		public static void RestoreCommands()
		{
			// Delte the YAML file
			try
			{
				string path = MainScript.UserInputPath;
				if (File.Exists(path))
					File.Delete(path);
			}
			catch (Exception) { }

			// Restore defaults
			_commands = _defaultBindings
				.ToDictionary(
					kv => kv.Key,
					kv => new CommandBindings(
						kv.Value.Keyboard,
						kv.Value.DualSense
					)
				);
		}

		private static CommandId? _rebindedCommand;
		private static Action<BindingResult> _dualSenseBindingFinished;

		public static bool DualSenseRebinding { get; private set; }

		private static Action<BindingResult> _keyboardBindingFinished;

		private static Dictionary<CommandId, CommandBindings> _commands;
		private static Dictionary<CommandId, CommandBindings> _defaultBindings;
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
		private static DualSenseInput? _bindingDualSenseShortcut;

		public static bool KeyboardRebinding { get; private set; }
	}
}
