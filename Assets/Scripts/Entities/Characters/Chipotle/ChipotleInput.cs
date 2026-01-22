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
using System.Linq;

using UnityEngine;
using UnityEngine.InputSystem;

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
			CollectMenuItems();
			CollectWalkCommands();
		}

		private void CollectMenuItems()
		{
			_menuCommands = new()
			{
				{ CommandId.GameExploreItem, ExploreItem },
				{ CommandId.GameSayZoneDescription, SayZoneDescription },
				{ CommandId.GameInventoryMenu, InventoryMenu },
				{ CommandId.GameInteract, Interact },
				{ CommandId.GamePickUpItem, PickUpItem },
				{ CommandId.GameGoForward, StepForward },
				{ CommandId.GameGoBack, StepBack },
				{ CommandId.GameGoLeft, StepLeft },
				{ CommandId.GameGoRight, StepRight },
				{ CommandId.GameTurnLeft, TurnLeft },
				{ CommandId.GameTurnRight, TurnRight },
				{ CommandId.GameTurnSharplyLeft, TurnSharplyLeft },
				{ CommandId.GameTurnSharplyRight, TurnSharplyRight },
				{ CommandId.GameTurnAround, TurnAround },
				{ CommandId.GameSayNavigatedObjectLocation, SayNavigatedObjectLocation},
				{ CommandId.GameSayItems, SayItems },
				{ CommandId.GameListItems, ListItems },
				{ CommandId.GameSayExits, SayExits },
				{ CommandId.GameListExits, ListExits },
				{ CommandId.GameSayZoneName, SayZoneName },
				{ CommandId.GameSayVisitedRegion, SayVisitedRegion },
				{ CommandId.GameSayZoneSize, SayZoneSize },
				{ CommandId.GameSayOrientation, SayOrientation },
				{ CommandId.GameSayAbsoluteCoordinates, SayAbsoluteCoordinates },
				{ CommandId.GameSendFeedback, MainScript.SendFeedback },
				{ CommandId.GameQuit, World.QuitGame },
			};
		}

		private Dictionary<CommandId, Action> _menuCommands;

		private void OnGameMenuOptionselected(GameMenuOptionselected message)
		{
			Action action = _menuCommands[message.Command];
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
				case DualSenseKeyPressed m: OnDualSenseKeyPressed(m); break;
				case DualSenseKeyReleased m: OnDualsenseKeyReleased(m); break;
				default: base.HandleMessage(message); break;
			}
		}

		protected override void OnDualSenseKeyPressed(DualSenseKeyPressed message)
		{
			DualSenseInput? command = InputConfig.GetBindings(CommandId.GameStopCutscene)?.DualSense;
			if (_cutsceneInProgress && command != null && message.Shortcut != command)
				return;

			base.OnDualSenseKeyPressed(message);
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

		/// <summary>
		/// Constructor
		/// </summary>
		protected override void AddCommands()
		{
			base.AddCommands();

			AddShortcut(CommandId.GameSayAbsoluteCoordinates, SayAbsoluteCoordinates);
			AddShortcut(CommandId.GameLoadPredefinedSave, LoadPredefinedSave);
			AddShortcut(CommandId.GameCreatePredefinedSave, CreatePredefinedSave);
			AddShortcut(CommandId.GameSayCharacters, SayCharacters);
			AddShortcut(CommandId.GameListCharacters, ListCharacters);
			AddShortcut(CommandId.GameSayNavigatedObjectLocation, SayNavigatedObjectLocation);
			AddShortcut(CommandId.GameExploreItem, ExploreItem);
			AddShortcut(CommandId.GameSayZoneDescription, SayZoneDescription);
			AddShortcut(CommandId.GameInventoryMenu, InventoryMenu);
			AddShortcut(CommandId.GamePickUpItem, PickUpItem);
			AddShortcut(CommandId.GameMenu, GameMenu);
			AddShortcut(CommandId.GameSayZoneSize, SayZoneSize);
			AddShortcut(CommandId.GameListExits, ListExits);
			AddShortcut(CommandId.GameListItems, ListItems);
			AddShortcut(CommandId.GameSayOrientation, SayOrientation);
			AddShortcut(CommandId.GameSayExits, SayExits);
			AddShortcut(CommandId.GameStopCutscene, StopCutscene);
			AddShortcut(CommandId.GameTerrainInfo, TerrainInfo);
			AddShortcut(CommandId.GameSayVisitedRegion, SayVisitedRegion);
			AddShortcut(CommandId.GameGoLeft, GoLeft);
			AddShortcut(CommandId.GameGoRight, GoRight);
			AddShortcut(CommandId.GameSayItems, SayItems);
			AddShortcut(CommandId.GameSayZoneName, SayZoneName);
			AddShortcut(CommandId.GameGoForward, GoForward);
			AddShortcut(CommandId.GameGoBack, GoBack);
			AddShortcut(CommandId.GameTurnLeft, TurnLeft);
			AddShortcut(CommandId.GameTurnRight, TurnRight);
			AddShortcut(CommandId.GameTurnSharplyLeft, TurnSharplyLeft);
			AddShortcut(CommandId.GameTurnSharplyRight, TurnSharplyRight);
			AddShortcut(CommandId.GameTurnAround, TurnAround);
			AddShortcut(CommandId.GameInteract, Interact);
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
			StopWalk();
			List<CommandId> commands = _menuCommands.Keys.ToList();
			OpenGameMenu message = new(Owner, commands);
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

			if (_walkCommands.Any(c => c.Keyboard == message.Shortcut))
				StopWalk();
		}

		private HashSet<CommandBindings> _walkCommands;

		private void CollectWalkCommands()
		{
			_walkCommands = new()
			{
				new(new KeyboardInput(Key.LeftShift)),
				new(new KeyboardInput(Key.RightShift)),
				Get(CommandId.GameTurnLeft),
				Get(CommandId.GameTurnRight),
				Get(CommandId.GameGoForward),
				Get(CommandId.GameGoBack),
				Get(CommandId.GameGoLeft),
				Get(CommandId.GameGoRight),
				Get(CommandId.GameTurnSharplyLeft),
				Get(CommandId.GameTurnSharplyRight)
			};

			CommandBindings Get(CommandId command) => InputConfig.GetBindings(command);
		}

		protected void OnDualsenseKeyReleased(DualSenseKeyReleased message)
		{
			if (_cutsceneInProgress)
				return;

			if (_walkCommands.Any(c => c.DualSense == message.Shortcut))
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

		/// <summary>
		/// Processes the KeyDown message.
		/// </summary>
		/// <param name="message">The message to be processed</param>
		protected override void OnKeyPressed(KeyPressed message)
		{
			KeyboardInput? command = InputConfig.GetBindings(CommandId.GameStopCutscene)?.Keyboard;
			if (_cutsceneInProgress && command != null && message.Shortcut != command)
				return;

			base.OnKeyPressed(message);
		}
	}
}