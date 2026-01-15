using Assets.Scripts.Messaging.Events.Input;

using Game.Controls;
using Game.Controls.DualSense;
using Game.Controls.Keyboard;
using Game.Messaging.Commands;
using Game.Messaging.Commands.GameInfo;
using Game.Messaging.Commands.GameManagement;
using Game.Messaging.Commands.Movement;
using Game.Messaging.Commands.Physics;
using Game.Messaging.Commands.UI;
using Game.Messaging.Events.GameManagement;
using Game.Messaging.Events.Input;
using Game.Messaging.Events.Sound;
using Game.Terrain;
using Game.UI;

using ProtoBuf;

using System;
using System.Collections.Generic;

using UnityEditor.ShortcutManagement;

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
		private void SayNavigatedObjectLocation()
		{
			InnerMessage(new SayNavigatedObjectLocation(this));
		}

		public override void Initialize()
		{
			base.Initialize();

			_gameMenuCommands = new Dictionary<string, Action>
			{
				{ "ResearchItem", ExploreItem },
				{ "SayZoneDescription", SayZoneDescription },
				{ "RunInventoryMenu", InventoryMenu },
				{ "Interact", Interact },
				{ "PickUpItem", PickUpItem },
				{ "StepForward", StepForward },
				{ "StepBack", StepBack },
				{ "StepLeft", StepLeft },
				{ "StepRight", StepRight },
				{ "TurnLeft", TurnLeft },
				{ "TurnRight", TurnRight },
				{ "TurnSharplyLeft", TurnSharplyLeft },
				{ "TurnSharplyRight", TurnSharplyRight },
				{ "TurnAround", TurnAround },
				{ "SayNavigatedObjectLocation", SayNavigatedObjectLocation},
				{ "SayItems", SayItems },
				{ "ListItems", ListItems },
				{ "SayExits", SayExits },
				{ "ListExits", ListExits },
				{ "SayZoneName", SayZoneName },
				{ "SayVisitedRegion", SayVisitedRegion },
				{ "SayZoneSize", SayZoneSize },
				{ "SayOrientation", SayOrientation },
				{ "SayAbsoluteCoordinates", SayAbsoluteCoordinates },
				{ "SendFeedback", MainScript.SendFeedback },
				{ "QuitGame", World.QuitGame },
			};
		}

		private Dictionary<string, Action> _gameMenuCommands;

		private void OnGameMenuOptionselected(GameMenuOptionselected message)
		{
			Action action = _gameMenuCommands[message.OptionId];
			action();
			World.GameInProgress = true;
		}

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			switch (message)
			{
				case GameMenuOptionselected m: OnGameMenuOptionselected(m); break;
				case KeyReleased kr: OnKeyReleased(kr); break;
				case KeyPressed m: OnKeyPressed(m); break;
				case DualSenseKeyPressed m: OnDualsenseKeyPressed(m); break;
				case DualSenseKeyReleased m: OnDualsenseKeyReleased(m); break;
				default: base.HandleMessage(message); break;
			}
		}

		/// <summary>
		/// Sends the ListCharacter message.
		/// </summary>
		private void ListCharacters()
		{
			ListCharacters message = new(this);
			InnerMessage(message);
		}

		/// <summary>
		/// Determines how quickly the game reacts to movement commands. The speed is in milliseconds.
		/// </summary>
		private const int _keyboardSpeed = 10;

		private void AddShortcut(Command command1, Action command)
		{
			CommandBindings shortcut = InputConfig.GetBindings(command1);
			if (shortcut == null)
				return;

			if (shortcut.Keyboard != null)
				_keyboardShortcuts[shortcut.Keyboard.Value] = command;
			if (shortcut.DualSense != null)
				_dualsenseShortcuts[shortcut.DualSense.Value] = command;
		}

		/// <summary>
		/// Constructor
		/// </summary>
		protected override void AddCommands()
		{
			base.AddCommands();

			AddShortcut(Command.GameSayAbsoluteCoordinates, SayAbsoluteCoordinates);
			AddShortcut(Command.GameLoadPredefinedSave, LoadPredefinedSave);
			AddShortcut(Command.GameCreatePredefinedSave, CreatePredefinedSave);
			AddShortcut(Command.GameSayCharacters, SayCharacters);
			AddShortcut(Command.GameListCharacters, ListCharacters);
			AddShortcut(Command.GameSayNavigatedObjectLocation, SayNavigatedObjectLocation);
			AddShortcut(Command.GameExploreItem, ExploreItem);
			AddShortcut(Command.GameSayZoneDescription, SayZoneDescription);
			AddShortcut(Command.GameInventoryMenu, InventoryMenu);
			AddShortcut(Command.GamePickUpItem, PickUpItem);
			AddShortcut(Command.GameMenu, GameMenu);
			AddShortcut(Command.GameSayZoneSize, SayZoneSize);
			AddShortcut(Command.GameListExits, ListExits);
			AddShortcut(Command.GameListItems, ListItems);
			AddShortcut(Command.GameSayOrientation, SayOrientation);
			AddShortcut(Command.GameSayExits, SayExits);
			AddShortcut(Command.GameStopCutscene, StopCutscene);
			AddShortcut(Command.GameTerrainInfo, TerrainInfo);
			AddShortcut(Command.GameSayVisitedRegion, SayVisitedRegion);
			AddShortcut(Command.GameGoLeft, GoLeft);
			AddShortcut(Command.GameGoRight, GoRight);
			AddShortcut(Command.GameSayItems, SayItems);
			AddShortcut(Command.GameSayZoneName, SayZoneName);
			AddShortcut(Command.GameGoForward, GoForward);
			AddShortcut(Command.GameGoBack, GoBack);
			AddShortcut(Command.GameTurnLeft, TurnLeft);
			AddShortcut(Command.GameTurnRight, TurnRight);
			AddShortcut(Command.GameTurnSharplyLeft, TurnSharplyLeft);
			AddShortcut(Command.GameTurnSharplyRight, TurnSharplyRight);
			AddShortcut(Command.GameTurnAround, TurnAround);
			AddShortcut(Command.GameInteract, Interact);
		}

		/// <summary>
		/// Instruucts the sound component to read description of the current zone.
		/// </summary>
		private void ExploreItem() => InnerMessage(new ExploreItem(this));

		private void SayZoneDescription() => InnerMessage(new SayZoneDescription(this));

		/// <summary>
		/// Performs the command to pick up an object off the ground.
		/// </summary>
		private void PickUpItem() => InnerMessage(new PickUpItem(this));

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
		protected void ListItems() => InnerMessage(new ListItems(this));

		/// <summary>
		/// Runs the game menu
		/// </summary>
		private void GameMenu()
		{
			InnerMessage(new StopWalk(this)); // Stop Chipotle if he's going somewhere.
			OpenGameMenu message = new(Owner);
			WindowHandler.ActiveWindow.TakeMessage(message);
		}

		/// <summary>
		/// Runs the inventory menu.
		/// </summary>
		protected void InventoryMenu() => InnerMessage(new RunInventoryMenu(this));

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

		private void SayAbsoluteCoordinates()
		{
			Vector2 coords = Owner.Area.Value.Center;
			string result = coords.GetString();
			GUIUtility.systemCopyBuffer = result;
			InnerMessage(new SayCoordinates(this, false));
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
		protected void OnKeyReleased(KeyReleased message)
		{
			if (_cutsceneInProgress)
				return;

			HashSet<KeyboardInput> walkCommands = new()
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

		protected void OnDualsenseKeyReleased(DualSenseKeyReleased message)
		{
			if (_cutsceneInProgress)
				return;

			HashSet<DualSenseInput> walkCommands = new()
			{
				new ("DPadLeft"),
				new ("DPadRight"),
				new ("RightStickLeft"),
				new ("RightStickRight"),
				new ("DPadUp"),
				new ("DPadDown")
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
		private void SayItems() => InnerMessage(new SayItems(this));

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

		protected override void OnDualsenseKeyPressed(DualSenseKeyPressed message)
		{
			if (_cutsceneInProgress
				&& message.Shortcut != new DualSenseInput("TouchpadButton"))
				return;

			base.OnDualsenseKeyPressed(message);
		}


		/// <summary>
		/// Processes the KeyDown message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected override void OnKeyPressed(KeyPressed message)
		{
			if (_cutsceneInProgress
				&& message.Shortcut != new KeyboardInput(KeyCode.Space))
				return;

			base.OnKeyPressed(message);
		}
	}
}