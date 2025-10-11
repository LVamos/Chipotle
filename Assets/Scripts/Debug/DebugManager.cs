using DavyKager;

using game.debug;

using Game;
using Game.Controls;
using Game.Controls.DualSense;
using Game.Entities.Characters;
using Game.Messaging.Commands.GameInfo;
using Game.Messaging.Commands.Movement;
using Game.Terrain;
using Game.UI;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

using Settings = Game.Settings;

namespace Game.Debug
{
	public class DebugManager : VirtualWindow
	{
		[DebugCommand(DebugCommand.OpenSettings)]
		public void OpenSettings() => WindowHandler.OpenDebugSettings();

		[DebugCommand(DebugCommand.OpenLog)]
		public void OpenLog() => Logger.OpenLog();

		public override void OnKeyDown(KeyboardInput shortcut)
		{
			base.OnKeyDown(shortcut);

			Action action = null;
			if (_keyboardCommands.TryGetValue(shortcut, out action))
				action();
		}

		private Dictionary<KeyboardInput, Action> _keyboardCommands = new();
		private Dictionary<DualSenseInput, Action> _gamepadCommands = new();

		private Character Tuttle => World.GetCharacter("tuttle");
		private Character Player => Player;

		/// <summary>
		/// A test method that saves current position as start position.
		/// </summary>
		[DebugCommand(DebugCommand.SaveStartPosition)]
		private void SaveStartPosition()
		{
			Settings.TestChipotleStartPosition = Player.Center;
			Settings.SaveSettings();
			Tolk.Speak("Startovní pozice uložena", true);
		}

		/// <summary>
		/// Test function to announce Tuttle's position
		/// </summary>
		[DebugCommand(DebugCommand.SayTuttlesPosition)]
		private void SayTuttlesPosition()
		{
			string distance = World.GetDistance(Tuttle, Player).ToString();
			string position = Tuttle.Center.ToString();
			string zone = Tuttle.Zone.Name.Indexed;
			Tolk.Speak(distance + Environment.NewLine + zone + " " + position, true);
		}

		/// <summary>
		/// Reports current position of the player in relative coordinates.
		/// </summary>
		[DebugCommand(DebugCommand.SayRelativeCoordinates)]
		private void SayRelativeCoordinates()
		{
			SayCoordinates message = new(this);
			Player.TakeMessage(message);
		}

		[DebugCommand(DebugCommand.SayItemSize)]
		private void SayItemSize()
		{
			SayItemSize message = new(this);
			Player.TakeMessage(message);
		}

		[DebugCommand(DebugCommand.ResetGame)]
		private void ResetGame() => WindowHandler.ResetGame();

		[DebugCommand(DebugCommand.RestoreStartPosition)]
		private void RestoreStartPosition()
		{
			if (!Settings.TestCommandsEnabled)
				return;

			Settings.TestChipotleStartPosition = null;
			Settings.SaveSettings();
			Tolk.Speak("Startovní pozice obnovena", true);
		}

		private string _lastClipboardText;

		/// <summary>
		/// Test method that moves Chipotle to coords taken from clipboard
		/// </summary>
		[DebugCommand(DebugCommand.GoToClipboardCoords)]
		private void GoToClipboardCoords()
		{
			try
			{
				string coords = GUIUtility.systemCopyBuffer;
				Vector2 target = coords.ToVector2();
				SetPosition message = new(this, target);
				Player.TakeMessage(message);
				return;
			}
			catch (Exception)
			{
			}
		}

		private const string _walkablePointsPath = "WalkablePoints.yaml";
		private const string _commandMapPath = "DebugCommands.yaml";

		private Dictionary<string, List<Vector2>> _walkablePoints;

		public override void Initialize()
		{
			base.Initialize();
			LoadWalkablePoints();
			LoadCommands();
		}

		private void LoadCommands()
		{
			string path = Path.Combine(MainScript.DebugPath, _commandMapPath);
			if (!File.Exists(path))
			{
				Logger.LogError($"Definice testovacích příkazů nenalezena: {path}");
				return;
			}

			try
			{
				string yamlText = File.ReadAllText(path);

				var deserializer = new DeserializerBuilder()
					.WithNamingConvention(PascalCaseNamingConvention.Instance)
					.Build();

				// Deserialize YAML into a dictionary: DebugCommand name → Keyboard & DualSense bindings
				var rawMap = deserializer.Deserialize<Dictionary<string, DebugCommandBindings>>(yamlText);

				// Get all DebugManager methods with DebugCommand attribute
				var methods = typeof(DebugManager).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
				var commandMethods = methods
					.Select(m => new
					{
						Method = m,
						Attr = m.GetCustomAttribute<DebugCommandAttribute>()
					})
					.Where(x => x.Attr != null)
					.ToDictionary(x => x.Attr.Command, x => (Action)Delegate.CreateDelegate(typeof(Action), this, x.Method));

				// Clear old dictionaries
				_keyboardCommands.Clear();
				_gamepadCommands.Clear();

				foreach (var kvp in rawMap)
				{
					// Convert string key to DebugCommand enum
					if (!Enum.TryParse<DebugCommand>(kvp.Key, out var command))
						continue;

					// Skip if there is no method for this DebugCommand
					if (!commandMethods.TryGetValue(command, out var action))
						continue;

					// Add keyboard input to dictionary if present
					if (!string.IsNullOrEmpty(kvp.Value.Keyboard))
					{
						var keyboardInput = new KeyboardInput(kvp.Value.Keyboard);
						_keyboardCommands[keyboardInput] = action;
					}

					// Add DualSense input to dictionary if present
					if (!string.IsNullOrEmpty(kvp.Value.DualSense))
					{
						var gamepadInput = new DualSenseInput(kvp.Value.DualSense);
						_gamepadCommands[gamepadInput] = action;
					}
				}
			}
			catch (Exception e)
			{
				Logger.LogError("Chyba při načítánídefinice testovacích příkazů", e.ToString());
			}
		}

		private void LoadWalkablePoints()
		{
			IDeserializer deserializer = new DeserializerBuilder()
				.WithNamingConvention(PascalCaseNamingConvention.Instance)
				.Build();

			string path = Path.Combine(MainScript.DebugPath, _walkablePointsPath);
			string yaml = File.ReadAllText(path);
			var raw = deserializer.Deserialize<Dictionary<string, List<float[]>>>(yaml);
			_walkablePoints = raw.ToDictionary(k => k.Key, v => v.Value.Select(p => new Vector2(p[0], p[1])).ToList());
		}

		/// <summary>
		/// Opens a menu with all zones and jumps to the nearest walkable position in the selected zone.
		/// </summary>
		[DebugCommand(DebugCommand.JumpToZoneMenu)]
		private void JumpToZoneMenu()
		{
			IEnumerable<Zone> zones = World.GetZones();
			List<List<string>> items =
			(
				from z in zones
				orderby z.Name.Indexed
				select (new List<string> { z.Name.Indexed })
			).ToList();

			MenuParameters parameters = new
				(
				items,
				"Vyber lokaci",
				menuClosed: (int item) => JumpToZone(item, items)
				);
			WindowHandler.Menu(parameters);
		}

		private void JumpToZone(int item, List<List<string>> items)
		{
			if (item == -1)
				return;

			string zone = items[item][0];
			Vector2 pointForPlayer = _walkablePoints[zone][0];
			SetPosition message1 = new(this, pointForPlayer);
			Player.TakeMessage(message1);

			// Move Tuttle
			Vector2 pointForTuttle = _walkablePoints[zone][1];
			SetPosition message2 = new(null, pointForTuttle);
			Tuttle.TakeMessage(message2);
		}
	}
}
