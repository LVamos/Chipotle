﻿using DavyKager;

using Game.Messaging.Commands;
using Game.Messaging.Commands.GameInfo;
using Game.Messaging.Commands.GameManagement;
using Game.Messaging.Commands.Movement;
using Game.Messaging.Commands.Physics;
using Game.Messaging.Commands.UI;
using Game.Messaging.Events.Input;
using Game.Messaging.Events.Sound;
using Game.Terrain;
using Game.UI;

using ProtoBuf;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Input = Game.Entities.Characters.Components.Input;
using Message = Game.Messaging.Message;

namespace Game.Entities.Characters.Chipotle
{
	/// <summary>
	/// Allows the player to scroll the entity using the keyboard.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class ChipotleInput : Input
	{
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

		private string _lastClipboardText;

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case KeyReleased kr: OnKeyUp(kr); break;
				case KeyPressed m: OnKeyDown(m); break;
				default: base.HandleMessage(message); break;
			}
		}

		/// <summary>
		/// Reports current position of the player in relative coordinates.
		/// </summary>
		private void SayRelativeCoordinates() => InnerMessage(new SayCoordinates(this));

		/// <summary>
		/// Sends the ListCharacter message.
		/// </summary>
		private void ListCharacters() => InnerMessage(new ListCharacters(this));

		/// <summary>
		/// Determines how quickly the game reacts to movement commands. The speed is in milliseconds.
		/// </summary>
		private const int _keyboardSpeed = 10;

		/// <summary>
		/// Constructor
		/// </summary>
		protected override void RegisterShortcuts()
		{
			base.RegisterShortcuts();

			AddShortcuts(
				new()
				{
					// Test commands
					[new(KeyShortcut.Modifiers.Control, KeyCode.R)] = ResetGame,
					[new(KeyShortcut.Modifiers.Shift, KeyCode.S)] = SayItemSize,
					[new(KeyShortcut.Modifiers.Shift, KeyCode.F5)] = LoadPredefinedSave,
					[new(KeyCode.F5)] = CreatePredefinedSave,
					[new(KeyCode.C)] = SayRelativeCoordinates,
					[new(KeyShortcut.Modifiers.Shift, KeyCode.T)] = SayTuttlesPosition,
					[new(KeyCode.F10)] = JumpToZone,
					[new(KeyCode.F11)] = SaveStartPosition,
					[new(false, true, false, KeyCode.C)] = SayAbsoluteCoordinates,
					[new(KeyCode.F12)] = () => GoToClipboardCoords(),

					// Other commands
					[new(KeyCode.Q)] = SayCharacters,
					[new(KeyShortcut.Modifiers.Shift, KeyCode.Q)] = ListCharacters,
					[new(KeyCode.P)] = ResearchObject,
					[new(KeyCode.R)] = SayZoneDescription,
					[new(KeyCode.I)] = RunInventoryMenu,
					[new(KeyShortcut.Modifiers.Shift, KeyCode.Return)] = PickUpObject,
					[new(KeyCode.Tab)] = GameMenu,
					[new(KeyCode.L)] = SayZoneSize,
					[new(false, true, false, KeyCode.V)] = ListExits,
					[new(false, true, false, KeyCode.O)] = ListObjects,
					[new(KeyCode.S)] = SayOrientation,
					[new(KeyCode.V)] = SayExits,
					[new(KeyCode.Space)] = StopCutscene,
					[new(KeyCode.T)] = TerrainInfo,
					[new(KeyCode.B)] = SayVisitedRegion,
					[new(KeyShortcut.Modifiers.Shift, KeyCode.LeftArrow)] = GoLeft,
					[new(KeyShortcut.Modifiers.Shift, KeyCode.RightArrow)] = GoRight,
					[new(KeyCode.O)] = SayObjects,
					[new(KeyCode.K)] = SayZoneName,
					[new(KeyCode.UpArrow)] = GoForward,
					[new(KeyCode.DownArrow)] = GoBack,
					[new(KeyCode.LeftArrow)] = TurnLeft,
					[new(KeyCode.RightArrow)] = TurnRight,
					[new(KeyShortcut.Modifiers.Control, KeyCode.LeftArrow)] = TurnSharplyLeft,
					[new(KeyShortcut.Modifiers.Control, KeyCode.RightArrow)] = TurnSharplyRight,
					[new(KeyShortcut.Modifiers.Control, KeyCode.DownArrow)] = TurnAround,
					[new(KeyCode.Return)] = Interact,
				}
			);

		}

		private void ResetGame() => WindowHandler.ResetGame();


		private void SayItemSize()
		{
			if (Settings.TestCommandsEnabled)
				InnerMessage(new SayItemSize(this));
		}

		/// <summary>
		/// Instruucts the sound component to read description of the current zone.
		/// </summary>
		private void ResearchObject() => InnerMessage(new ExploreObject(this));

		private void SayZoneDescription() => InnerMessage(new SayZoneDescription(this));

		/// <summary>
		/// Performs the command to pick up an object off the ground.
		/// </summary>
		private void PickUpObject() => InnerMessage(new PickUpItem(this));

		/// <summary>
		/// Creates a predefined save.
		/// </summary>
		private void LoadPredefinedSave()
		{
			if (Settings.AllowPredefinedSaves)
				InnerMessage(new LoadPredefinedSave(this));
		}

		/// <summary>
		/// Creates a predefined save.
		/// </summary>
		private void CreatePredefinedSave()
		{
			if (Settings.AllowPredefinedSaves)
				InnerMessage(new CreatePredefinedSave(this));
		}

		/// <summary>
		/// Lists navigable objects.
		/// </summary>
		protected void ListObjects() => InnerMessage(new ListObjects(this));

		/// <summary>
		/// Runs the game menu
		/// </summary>
		private void GameMenu()
		{
			InnerMessage(new StopWalk(this)); // Stop Chipotle if he's going somewhere.

			// Prepare the menu
			(string name, Action command)[] commands =
			{
				("Prozkoumej objekt: pé", ResearchObject),
				("Rozhlédni se: r", SayZoneDescription),
				("inventář: I", RunInventoryMenu),
				("použij objekt nebo dveře: entr", Interact),
				("Vezmi objekt: šift entr", PickUpObject),
				("Jdi dopředu: horní šipka", StepForward),
				("Jdi dozadu: dolní šipka", StepBack),
				("Jdi doleva: šift levá šipka", StepLeft),
				("Jdi doprava: šift pravá šipka", StepRight),
				("Otoč se trochu doleva: levá šipka", TurnLeft),
				("Otoč se trochu doprava: pravá šipka", TurnRight),
				("Otoč se ostře doleva: kontrol levá šipka", TurnSharplyLeft),
				("Otoč se ostře doprava: kontrol pravá šipka", TurnSharplyRight),
				("Otoč se čelem vzad: kontrol dolní šipka", TurnAround),
				("okolní objekty: O", SayObjects),
				("Naveď mě k objektu: šift O", ListObjects),
				("východy z lokace: Vé", SayExits),
				("Naveď mě k východu: šift vé", ListExits),
				("kde jsem: ká", SayZoneName),
				("Byl jsem tu: bé", SayVisitedRegion),
				("Rozměry lokace: el", SayZoneSize),
				("kompas: Es", SayOrientation),
				("souřadnice: Cé", SayAbsoluteCoordinates),
				("Poslat zprávu autorovi: Kontrol zet", MainScript.SendFeedback),
				("hlavní menu: Iskejp", World.QuitGame),
			};

			// Run the menu
			List<List<string>> items = commands.Select(c => new List<string>() { c.name }).ToList();
			World.GameInProgress = false;
			int item = WindowHandler.Menu(new(items, "Menu", " ", 0, false));

			World.GameInProgress = true;
			if (item > 0)
				commands[item].command();
		}

		/// <summary>
		/// Runs the inventory menu.
		/// </summary>
		protected void RunInventoryMenu() => InnerMessage(new RunInventoryMenu(this));

		/// <summary>
		/// Moves the NPC one step to the right.
		/// </summary>
		private void StepRight()
		{
			GoRight();
			StopWalk();
		}

		/// <summary>
		/// Moves the NPC one step to the left.
		/// </summary>
		private void StepLeft()
		{
			GoLeft();
			StopWalk();
		}

		/// <summary>
		/// Moves the NPC one step back.
		/// </summary>
		private void StepBack()
		{
			GoBack();
			StopWalk();
		}

		/// <summary>
		/// Moves the NPC one step forth.
		/// </summary>
		private void StepForward()
		{
			GoForward();
			StopWalk();
		}

		/// <summary>
		/// Reports size of the zone in which the Chipotle NPC is currently located.
		/// </summary>
		private void SayZoneSize() => InnerMessage(new SayZoneSize(this));

		/// <summary>
		/// Test function to announce Tuttle's position
		/// </summary>
		private void SayTuttlesPosition()
		{
			if (!Settings.TestCommandsEnabled)
				return;

			Character tuttle = World.GetCharacter("tuttle");
			string distance = World.GetDistance(tuttle, Owner).ToString();
			string position = tuttle.Area.Value.Center.ToString();
			string zone = tuttle.Zone.Name.Indexed;
			Tolk.Speak(distance + Environment.NewLine + zone + " " + position, true);
		}

		/// <summary>
		/// Opens a menu with all zones and jumps to the nearest walkable position in the selected zone.
		/// </summary>
		private void JumpToZone()
		{
			if (!Settings.TestCommandsEnabled)
				return;

			Vector2 me = Owner.Area.Value.Center;
			List<List<string>> items =
			(
				from l in World.GetZones()
				orderby l.Name.Indexed
				select (new List<string> { l.Name.Indexed })
			).ToList();

			int item = WindowHandler.Menu(new(items, "Vyber lokaci"));
			if (item == -1)
				return;

			Zone zone = World.GetZone(items[item][0]);
			Vector2 point = zone.Area.Value.GetWalkableTiles().First().Position;
			InnerMessage(new SetPosition(this, point));

			// Move Tuttle
			point = zone.Area.Value.GetWalkableTiles().First(t => t.Position != point).Position;
			World.GetCharacter("tuttle").TakeMessage(new SetPosition(null, point));
		}

		/// <summary>
		/// A test method that saves current position as start position.
		/// </summary>
		private void SaveStartPosition()
		{
			if (!Settings.TestCommandsEnabled)
				return;

			Settings.TestChipotleStartPosition = Owner.Area.Value.UpperLeftCorner;
			Settings.SaveSettings();
			Tolk.Speak("Startovní pozice uložena", true);
		}

		private void SayAbsoluteCoordinates()
		{
			if (!Settings.TestCommandsEnabled)
				return;

			Vector2 coords = Owner.Area.Value.Center;
			string result = coords.GetString();
			GUIUtility.systemCopyBuffer = result;
			InnerMessage(new SayCoordinates(this, false));
		}

		/// <summary>
		/// Test method that moves Chipotle to coords taken from clipboard
		/// </summary>
		private bool GoToClipboardCoords()
		{
			if (!Settings.TestCommandsEnabled)
				return false;

			try
			{
				string coords = GUIUtility.systemCopyBuffer;
				Vector2 target = coords.ToVector2();
				InnerMessage(new SetPosition(this, target));
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}

		/// <summary>
		/// Lists exits from current zone.
		/// </summary>
		protected void ListExits() => InnerMessage(new ListExits(this));

		/// <summary>
		/// Processes the CutsceneBegan message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected override void OnCutsceneBegan(CutsceneBegan message)
		{
			base.OnCutsceneBegan(message);

			if (message.CutsceneName is "cs7" or "cs10")
				_messagingEnabled = false;
		}

		/// <summary>
		/// Processes the KeyUp message.
		/// </summary>
		/// <param name="message">The message</param>
		protected void OnKeyUp(KeyReleased message)
		{
			if (_cutsceneInProgress)
				return;

			HashSet<KeyShortcut> walkCommands = new()
			{
				new (KeyCode.LeftShift),
				new (KeyCode.RightShift),
				new (KeyCode.LeftArrow),
				new (KeyCode.RightArrow),
				new (KeyCode.UpArrow),
				new (KeyCode.DownArrow),
				new (false, true, false, KeyCode.LeftArrow),
				new (false, true, false, KeyCode.RightArrow)
			};

			if (walkCommands.Contains(message.Shortcut))
				StopWalk();
		}

		/// <summary>
		/// Reports list of all exits from current zone.
		/// </summary>
		protected void SayExits() => InnerMessage(new SayExits(this));

		/// <summary>
		/// Reports current orientation setting of the Chipotle NPC.
		/// </summary>
		protected void SayOrientation() => InnerMessage(new SayOrientation(this));

		/// <summary>
		/// Allows the player to use a nearby object or door.
		/// </summary>
		private void Interact() => InnerMessage(new Interact(this));

		/// <summary>
		/// Moves the NPC one step back.
		/// </summary>
		private void GoBack() => InnerMessage(new StartWalk(this, TurnType.Around));

		/// <summary>
		/// Starts Moving the NPC forth.
		/// </summary>
		private void GoForward() => InnerMessage(new StartWalk(this, TurnType.None));

		/// <summary>
		/// Starts moving the NPC to the left.
		/// </summary>
		private void GoLeft() => InnerMessage(new StartWalk(this, TurnType.SharplyLeft));

		/// <summary>
		/// Moves the NPC one step to the right perpendicullar to current orientation.
		/// </summary>
		private void GoRight() => InnerMessage(new StartWalk(this, TurnType.SharplyRight));

		/// <summary>
		/// Announces the public name of the zone where the NPC is currently located using a
		/// screen reader or a voice synthesizer.
		/// </summary>
		private void SayZoneName() => InnerMessage(new SayZoneName(this));

		/// <summary>
		/// Reports the nearest characters around this character using a screen reader or voice synthesizer.
		/// </summary>
		private void SayCharacters() => InnerMessage(new SayCharacters(this));

		/// <summary>
		/// Reports the nearest objects around the NPC using a screen reader or voice synthesizer.
		/// </summary>
		private void SayObjects() => InnerMessage(new SayObjects(this));

		/// <summary>
		/// Stops the currently playing cutscene.
		/// </summary>
		private void StopCutscene()
		{
			if (_cutsceneInProgress)
			{
				_cutsceneInProgress = false;
				World.StopCutscene(Owner);
			}
		}

		/// <summary>
		/// Tells the physics to stop the Chipotle NPC.
		/// </summary>
		private void StopWalk() => InnerMessage(new StopWalk(this));

		/// <summary>
		/// Reports the terrain on which the NPC is standing.
		/// </summary>
		private void TerrainInfo() => InnerMessage(new SayTerrain(this));

		/// <summary>
		/// Rotates the NPC around Z axis.
		/// </summary>
		private void TurnAround() => InnerMessage(new ChangeOrientation(this, TurnType.Around));

		/// <summary>
		/// Rotates the NPC around Z axis 45 degrees to the left.
		/// </summary>
		private void TurnLeft() => InnerMessage(new ChangeOrientation(this, TurnType.SlightlyLeft));

		/// <summary>
		/// Rotates the NPC around Z axis 45 degrees to the right.
		/// </summary>
		private void TurnRight() => InnerMessage(new ChangeOrientation(this, TurnType.SlightlyRight));

		/// <summary>
		/// Rotates the NPC around Z axis 90 degrees to the left.
		/// </summary>
		private void TurnSharplyLeft() => InnerMessage(new ChangeOrientation(this, TurnType.SharplyLeft));

		/// <summary>
		/// Rotates the NPC around Z axis 90 degrees to the right.
		/// </summary>
		private void TurnSharplyRight() => InnerMessage(new ChangeOrientation(this, TurnType.SharplyRight));

		/// <summary>
		/// Reports if the player have already visited the current zone.
		/// </summary>
		private void SayVisitedRegion() => InnerMessage(new SayVisitedRegion(this));

		/// <summary>
		/// Processes the KeyDown message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected override void OnKeyDown(KeyPressed message)
		{
			if (_cutsceneInProgress
				&& message.Shortcut != new KeyShortcut(KeyCode.Space))
				return;

			base.OnKeyDown(message);
		}
	}
}