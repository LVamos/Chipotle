using Game.Controls.DualSense;
using Game.Controls.Keyboard;
using Game.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Game.Controls
{
	public static class InputConfig
	{
		public static string GetCommandName(CommandId command)
		{
			return _commandNames[command];
		}

		public static bool TrySetBinding(CommandId command, CommandBindings bindings)
		{
			if (_commands.Values.Contains(bindings))
				return false;

			_commands[command] = bindings;
			SaveBindings();
			return true;
		}

		private static void SaveBindings()
		{
			try
			{
				string path = MainScript.UserInputPath;
				YamlHelper.SaveToFile(path, _commands);
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

		private static Dictionary<CommandId, CommandBindings> _commands;

		private static Dictionary<CommandId, string> _commandNames;
	}
}
