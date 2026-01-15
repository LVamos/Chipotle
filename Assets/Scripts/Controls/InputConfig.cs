using game.debug;

using Game.Controls.DualSense;
using Game.Controls.Keyboard;
using Game.Debug;
using Game.Serialization;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using UnityEditor.Build;

using UnityEngine.UIElements;

namespace Game.Controls
{
	public static class InputConfig
	{
		public static bool TrySetBinding(Command command, CommandBindings bindings)
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

		public static CommandBindings GetBindings(Command command)
		{
			CommandBindings bindings=null;
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

		public static void LoadBindings()
		{
			LoadDefaultBindings();
			LoadUserBindings();
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
				Command command;
				if (!Enum.TryParse<Command>(pair.Key, out command))
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

		private static Dictionary<Command, CommandBindings> _commands;
	}
}
