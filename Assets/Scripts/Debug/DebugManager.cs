using Settings = Game.Settings;
using DavyKager;

using Game;
using Game.Entities.Characters;
using Game.Messaging.Commands.GameInfo;
using Game.Messaging.Commands.Movement;
using Game.Terrain;
using Game.UI;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Assets.Scripts.Debug
{
	public class DebugManager : VirtualWindow
	{
		private Character Tuttle => World.GetCharacter("tuttle");
		private Character Player => Player;

		/// <summary>
		/// A test method that saves current position as start position.
		/// </summary>
		private void SaveStartPosition()
		{
			Settings.TestChipotleStartPosition = Player.Center;
			Settings.SaveSettings();
			Tolk.Speak("Startovní pozice uložena", true);
		}

		/// <summary>
		/// Test function to announce Tuttle's position
		/// </summary>
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
		private void SayRelativeCoordinates()
		{
			SayCoordinates message = new(this);
			Player.TakeMessage(message);
		}

		private void SayItemSize()
		{
			SayItemSize message = new(this);
			Player.TakeMessage(message);
		}

		private void ResetGame() => WindowHandler.ResetGame();

		private void RestoreStartPosition()
		{
			if (!Settings.TestCommandsEnabled)
				return;

			Settings.TestChipotleStartPosition = null;
			Settings.SaveSettings();
			Tolk.Speak("Startovní pozice obnovena", true);
		}

		private string _lastClipboardText;

		private void Update() => WatchClipboard();

		private void WatchClipboard()
		{
			return;
			// Jump to coords in clipboard whenever the clipboard content changes.
			if (!Settings.TestCommandsEnabled)
				return;

			string clipboard = GUIUtility.systemCopyBuffer;
			if (clipboard != _lastClipboardText)
			{
				if (GoToClipboardCoords())
				{
					_lastClipboardText = clipboard;
					WindowHandler.FocusGameWindow();
				}
			}
		}

		/// <summary>
		/// Test method that moves Chipotle to coords taken from clipboard
		/// </summary>
		private bool GoToClipboardCoords()
		{
			try
			{
				string coords = GUIUtility.systemCopyBuffer;
				Vector2 target = coords.ToVector2();
				SetPosition message = new(this, target);
				Player.TakeMessage(message);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		private const string _walkablePointsPath = "WalkablePoints.yaml";

		private Dictionary<string, List<Vector2>> _walkablePoints;

		public override void Initialize()
		{
			base.Initialize();
			LoadWalkablePoints();
			LoadKeyboardShortcuts();
		}

		private void LoadKeyboardShortcuts()
		{
			RegisterShortcuts
							(
												(new(KeyboardInput.Modifiers.Shift, KeyCode.S), SayItemSize),
								(new(KeyCode.C), SayRelativeCoordinates),
								(new(KeyboardInput.Modifiers.Shift, KeyCode.T), SayTuttlesPosition),
								(new(KeyCode.F11), SaveStartPosition),
								(new(KeyboardInput.Modifiers.Control, KeyCode.R), ResetGame),
								(new(KeyboardInput.Modifiers.Shift, KeyCode.F11), RestoreStartPosition),

								(new(KeyCode.F10), JumpToZoneMenu),
								(new(KeyCode.F12), () => GoToClipboardCoords())
							);
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
