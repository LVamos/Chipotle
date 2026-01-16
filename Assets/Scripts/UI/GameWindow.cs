using Assets.Scripts.Messaging.Commands.Characters;
using Assets.Scripts.Messaging.Events.Input;
using Assets.Scripts.Models;
using Assets.Scripts.Narration.WorldDescribers;

using DavyKager;

using Game.Controls;
using Game.Controls.DualSense;
using Game.Controls.Keyboard;
using Game.Entities;
using Game.Entities.Items;
using Game.Messaging;
using Game.Messaging.Commands.Characters;
using Game.Messaging.Commands.Physics;
using Game.Messaging.Commands.UI;
using Game.Messaging.Events.GameManagement;
using Game.Messaging.Events.Input;
using Game.Models;
using Game.Narration.WorldDescribers;
using Game.Terrain;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Message = Game.Messaging.Message;

namespace Game.UI
{
	/// <summary>
	/// A virtual window opened during the game
	/// </summary>
	[Serializable]
	public class GameWindow : VirtualWindow
	{
		private Dictionary<string, string> _gameMenuOptions = new()
{
	{ "Prozkoumej předmět: pé", "ResearchItem" },
	{ "Rozhlédni se: r", "SayZoneDescription" },
	{ "inventář: I", "RunInventoryMenu" },
	{ "použij předmět nebo dveře: entr", "Interact" },
	{ "Vezmi předmět: šift entr", "PickUpItem" },
	{ "Jdi dopředu: horní šipka", "StepForward" },
	{ "Jdi dozadu: dolní šipka", "StepBack" },
	{ "Jdi doleva: šift levá šipka", "StepLeft" },
	{ "Jdi doprava: šift pravá šipka", "StepRight" },
	{ "Otoč se trochu doleva: levá šipka", "TurnLeft" },
	{ "Otoč se trochu doprava: pravá šipka", "TurnRight" },
	{ "Otoč se ostře doleva: kontrol levá šipka", "TurnSharplyLeft" },
	{ "Otoč se ostře doprava: kontrol pravá šipka", "TurnSharplyRight" },
	{ "Otoč se čelem vzad: kontrol dolní šipka", "TurnAround" },
	{ "Kde je hledaný předmět, východ nebo postava: shift K", "SayNavigatedObjectLocation" },
	{ "okolní předměty: O", "SayItems" },
	{ "Naveď mě k předmětu: šift O", "ListItems" },
	{ "východy: Vé", "SayExits" },
	{ "Naveď mě k východu: šift vé", "ListExits" },
	{ "kde jsem: ká", "SayZoneName" },
	{ "Byl jsem tu: bé", "SayVisitedRegion" },
	{ "Rozměry lokace: el", "SayZoneSize" },
	{ "kompas: Es", "SayOrientation" },
	{ "souřadnice: Cé", "SayAbsoluteCoordinates" },
	{ "Poslat zprávu autorovi: Kontrol zet", "SendFeedback" },
	{ "hlavní menu: Iskejp", "QuitGame" },
};


		/// <summary>
		/// Runs the game menu
		/// </summary>
		private void OnOpenGameMenu(OpenGameMenu message)
		{
			List<List<string>> items =
				_gameMenuOptions
				.Select(c => new List<string>() { c.Key }).ToList();
			Action<int> menuHandler = (option) =>
			{
				HandleGameMenu(items, option, message.Sender as MessagingObject);
			};
			MenuParameters parameters = new(
				items,
				"Menu",
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
			WindowHandler.Menu(parameters);
		}

		private void HandleGameMenu(List<List<string>> items, int option, MessagingObject initiator)
		{
			if (option == -1)
			{
				Tolk.Speak("hra");
				return;
			}
			string optionDescription = items[option][0];
			string optionId = _gameMenuOptions[optionDescription];
			GameMenuOptionselected message = new(this, optionId);
			initiator.TakeMessage(message);
		}

		public static GameWindow CreateInstance()
		{
			GameObject obj = new("GameWindow");
			GameWindow window = obj.AddComponent<GameWindow>();
			window.Initialize();
			return window;
		}


		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected override void HandleMessage(Message message)
		{
			base.HandleMessage(message);

			switch (message)
			{
				case SelectNavigableExit m: OnSelectNavigableExit(m); break;
				case SelectNavigableCharacter m: OnSelectNavigableCharacter(m); break;
				case SelectNavigableItem m: OnSelectNavigableItem(m); break;
				case OpenGameMenu m: OnOpenGameMenu(m); break;
				case SelectObjectToApply m: OnSelectObjectToApply(m); break;
				case selectInventoryAction m: OnselectInventoryAction(m); break;
				case SelectItemToPick m: OnSelectItemToPick(m); break;
				case SelectObjectToExplore m: OnSelectObjectToExplore(m); break;
				case SelectObjectToUse m: OnSelectObjectToUse(m); break;
			}
		}

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		private void OnSelectObjectToApply(SelectObjectToApply message)
		{
			const string prompt = "Na co to chceš použít?";
			List<Entity> objects = message.Objects.Cast<Entity>().ToList();
			List<List<string>> names = GetFriendlyNames(objects);
			Action<int> action =
				(option) => HandleApplyItemMenu(message.Sender as MessagingObject, message.Objects, option, message.ItemToApply);
			MenuParameters parameters = new(
							names,
							prompt,
							wrappingAllowed: false,
								introSound: "MenuItemActivated",
								outroSound: "MenuOpened",
		selectionSound: "MenuItemSelected",
		wrapDownSound: "MenuWrapped",
		wrapUpSound: "MenuWrapped",
		upperEdgeSound: "MenuEdge",
		lowerEdgeSound: "MenuEdge",
							menuClosed: action);
			WindowHandler.Menu(parameters);
		}

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		private void OnselectInventoryAction(selectInventoryAction message)
		{
			InventoryMenuParametersDTO parameters = new(
				message.Items,
				(option, action) => HandleInventoryAction(message.Sender as MessagingObject, message.Items, option, action)
			);

			InventoryMenu.Run(parameters);
		}

		private void HandleExploringMenu(MessagingObject sender, List<Entity> objects, int option)
		{
			if (option == -1 || sender == null)
				return;

			ExploreItem message = new(this, objects[option]);
			sender.TakeMessage(message);
		}

		private void HandleNavigableExitMenu(MessagingObject sender, List<NavigableExitInfo> exits, int option)
		{
			if (option == -1 || sender == null)
				return;

			NavigateToExit message = new(this, exits[option].Exit);
			sender.TakeMessage(message);
		}

		private void HandleInteractionMenu(MessagingObject sender, List<MapElement> objects, int option)
		{
			if (option == -1 || sender == null)
				return;

			Interact message = new(this, objects[option]);
			sender.TakeMessage(message);
		}


		private void HandleInventoryAction(MessagingObject sender, List<Item> items, int option, InventoryAction action)
		{
			if (option == -1 || sender == null)
				return;

			Item selectedItem = items[option];

			switch (action)
			{
				case InventoryAction.Use:
					sender.TakeMessage(new Interact(this, selectedItem));
					break;
				case InventoryAction.ApplyToTarget:
					sender.TakeMessage(new ApplyItemToTarget(this, selectedItem));
					break;
				case InventoryAction.Place:
					sender.TakeMessage(new PlaceItem(this, selectedItem));
					break;
			}
		}

		private void OnSelectItemToPick(SelectItemToPick message)
		{
			const string prompt = "Co chceš sebrat?";
			List<Entity> entities = message.Items.Cast<Entity>().ToList();
			List<List<string>> names = GetFriendlyNames(entities);
			Action<int> action =
				(option) => HandlePickingMenu(message.Sender as MessagingObject, message.Items, option);
			MenuParameters parameters = new(
							names,
							prompt,
							wrappingAllowed: false,
															introSound: "MenuItemActivated",
								outroSound: "MenuOpened",
		selectionSound: "MenuItemSelected",
		wrapDownSound: "MenuWrapped",
		wrapUpSound: "MenuWrapped",
		upperEdgeSound: "MenuEdge",
		lowerEdgeSound: "MenuEdge",
							menuClosed: action);
			WindowHandler.Menu(parameters);
		}

		private void HandleApplyItemMenu(MessagingObject sender, List<MapElement> objects, int option, Item itemToApply)
		{
			if (option == -1 || sender == null)
				return;

			ApplyItemToTarget message = new(this, itemToApply, objects[0]);
			sender.TakeMessage(message);
		}


		private void HandlePickingMenu(MessagingObject sender, List<Item> objects, int option)
		{
			if (option == -1 || sender == null)
				return;

			PickUpItem message = new(this, objects[option]);
			sender.TakeMessage(message);
		}

		private MapElement ElementSelectionMenu(List<MapElement> objects, string prompt)
		{
			if (objects.IsNullOrEmpty())
				throw new ArgumentException($"{objects} is null or empty");

			// Copy friendly names from the given objects into an array.
			List<List<string>> names =
				objects.Select(o => new List<string>() { o.Name.Friendly })
					.ToList();

			MenuParameters parameters = null;
			int option = WindowHandler.Menu(parameters);

			if (option == -1)
				return null;

			return objects[option];
		}

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		private void OnSelectObjectToUse(SelectObjectToUse message)
		{
			const string prompt = "Co chceš použít?";

			// Copy friendly names from the given objects into an array.
			List<Entity> objects = message.Objects.Cast<Entity>().ToList();
			List<List<string>> names = GetFriendlyNames(objects);

			MenuParameters parameters = new(
				names,
				prompt,
				wrappingAllowed: false,
								introSound: "MenuItemActivated",
								outroSound: "MenuOpened",
		selectionSound: "MenuItemSelected",
		wrapDownSound: "MenuWrapped",
		wrapUpSound: "MenuWrapped",
		upperEdgeSound: "MenuEdge",
		lowerEdgeSound: "MenuEdge",
				menuClosed: (option) => HandleInteractionMenu(message.Sender as MessagingObject, message.Objects, option));
			WindowHandler.Menu(parameters);
		}

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		private void OnSelectObjectToExplore(SelectObjectToExplore message)
		{
			const string prompt = "Co chceš prozkoumat?";
			List<List<string>> names = GetFriendlyNames(message.Objects);
			MenuParameters parameters = new(
							names,
							prompt,
							wrappingAllowed: false,
								introSound: "MenuItemActivated",
								outroSound: "MenuOpened",
		selectionSound: "MenuItemSelected",
		wrapDownSound: "MenuWrapped",
		wrapUpSound: "MenuWrapped",
		upperEdgeSound: "MenuEdge",
		lowerEdgeSound: "MenuEdge",
							menuClosed: (option) => HandleExploringMenu(message.Sender as MessagingObject, message.Objects, option));
			WindowHandler.Menu(parameters);
		}

		private NavigableExitDescriber _exitDescriber;
		private NavigableItemDescriber _itemDescriber;
		private NavigableCharacterDescriber _characterDescriber;

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		private void OnSelectNavigableExit(SelectNavigableExit message)
		{
			const string prompt = "Východy";
			List<NavigableObjectInfo> objectInfo = message.Exits.Cast<NavigableObjectInfo>().ToList();
			List<List<string>> descriptions = _exitDescriber.GetStructuredDescriptions(objectInfo);
			MenuParameters parameters = new(
							descriptions,
							prompt,
	searchIndex: 2,
								wrappingAllowed: false,
								introSound: "MenuItemActivated",
								outroSound: "MenuOpened",
		selectionSound: "MenuItemSelected",
		wrapDownSound: "MenuWrapped",
		wrapUpSound: "MenuWrapped",
		upperEdgeSound: "MenuEdge",
		lowerEdgeSound: "MenuEdge",
							menuClosed: (option) => HandleNavigableExitMenu(message.Sender as MessagingObject, message.Exits, option));
			WindowHandler.Menu(parameters);
		}

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		private void OnSelectNavigableItem(SelectNavigableItem message)
		{
			const string prompt = "Okolní předměty";
			List<NavigableObjectInfo> objectInfo =
				message.Items
				.Cast<NavigableObjectInfo>()
				.ToList();
			List<List<string>> descriptions = _itemDescriber.GetStructuredDescriptions(objectInfo);
			MenuParameters parameters = new(
							descriptions,
							prompt,
								wrappingAllowed: false,
																introSound: "MenuItemActivated",
								outroSound: "MenuOpened",
		selectionSound: "MenuItemSelected",
		wrapDownSound: "MenuWrapped",
		wrapUpSound: "MenuWrapped",
		upperEdgeSound: "MenuEdge",
		lowerEdgeSound: "MenuEdge",

							menuClosed: (option) => HandleNavigableItemMenu(message.Sender as MessagingObject, message.Items, option));
			WindowHandler.Menu(parameters);
		}

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		private void OnSelectNavigableCharacter(SelectNavigableCharacter message)
		{
			const string prompt = "Okolní postavy";
			List<NavigableObjectInfo> objectInfo =
				message.Characters
				.Cast<NavigableObjectInfo>()
				.ToList();
			List<List<string>> descriptions = _characterDescriber.GetStructuredDescriptions(objectInfo);
			MenuParameters parameters = new(
							descriptions,
							prompt,
								wrappingAllowed: false,
								introSound: "MenuItemActivated",
								outroSound: "MenuOpened",
		selectionSound: "MenuItemSelected",
		wrapDownSound: "MenuWrapped",
		wrapUpSound: "MenuWrapped",
		upperEdgeSound: "MenuEdge",
		lowerEdgeSound: "MenuEdge",
							menuClosed: (option) => HandleNavigableCharacterMenu(message.Sender as MessagingObject, message.Characters, option));
			WindowHandler.Menu(parameters);
		}

		private void HandleNavigableItemMenu(MessagingObject sender, List<NavigableItemInfo> items, int option)
		{
			if (option == -1 || sender == null)
				return;

			NavigateToItem message = new(this, items[option].Item);
			sender.TakeMessage(message);
		}

		private void HandleNavigableCharacterMenu(MessagingObject sender, List<NavigableCharacterInfo> characters, int option)
		{
			if (option == -1 || sender == null)
				return;

			NavigateToCharacter message = new(this, characters[option].Character);
			sender.TakeMessage(message);
		}

		private List<List<string>> GetFriendlyNames(List<Entity> objects)
		{

			List<List<string>> names =
							objects.Select(o => new List<string>() { o.Name.Friendly })
								.ToList();
			return names;
		}

		/// <summary>
		/// constructor
		/// </summary>
		public void Initialize()
		{
			base.Initialize();

			_messagingEnabled = true;
			_exitDescriber = new();
			_itemDescriber = new();
			_characterDescriber = new();
		}

		protected override void AddShortcuts()
		{
			base.AddShortcuts();

			AddShortcut(Command.GameQuit, QuitGameMenu);
			AddShortcut(Command.GameSendFeedback, MainScript.SendFeedback);
		}

		public override void OnKeyDown(DualSenseInput shortcut)
		{
			base.OnKeyDown(shortcut);

			if (World.GameInProgress)
				World.Player.TakeMessage(new DualSenseKeyPressed(this, shortcut));
		}

		/// <summary>
		/// Processes the KeyDown message.
		/// </summary>
		/// <param name="e">The message to be handled</param>
		public override void OnKeyDown(KeyboardInput shortcut)
		{
			base.OnKeyDown(shortcut);

			if (World.GameInProgress)
				World.Player.TakeMessage(new KeyPressed(this, shortcut));
		}

		/// <summary>
		/// Processes the KeyUpmessage.
		/// </summary>
		/// <param name="shortcut">The message to be handled</param>
		public override void OnKeyUp(KeyboardInput shortcut)
		{
			if (World.GameInProgress && World.Player != null)
			{
				KeyReleased message = new(this, shortcut);
				World.Player.TakeMessage(message);
			}
		}

		public override void OnKeyUp(DualSenseInput shortcut)
		{
			if (World.GameInProgress && World.Player != null)
			{
				DualSenseKeyReleased message = new(this, shortcut);
				World.Player.TakeMessage(message);
			}
		}


		/// <summary>
		/// Quits the game.
		/// </summary>
		private void QuitGame()
		{
			World.QuitGame();
		}

		private void QuitGameMenu()
		{
			const string prompt = "Chceš ukončit hru?";
			List<List<string>> items = new()
			{
			new List<string>(){ "Ano"},
			new List<string>(){ "Ne"}
			};
			MenuParameters parameters = new(
							items,
							prompt,
	searchIndex: 0,
								wrappingAllowed: false,
								introSound: "MenuItemActivated",
								outroSound: "MenuOpened",
		selectionSound: "MenuItemSelected",
		wrapDownSound: "MenuWrapped",
		wrapUpSound: "MenuWrapped",
		upperEdgeSound: "MenuEdge",
		lowerEdgeSound: "MenuEdge",
							menuClosed: (option) => HandleQuitGameMenu(option == 0));
			WindowHandler.Menu(parameters);
		}

		private void HandleQuitGameMenu(bool quit)
		{
			if (quit)
			{
				Tolk.Speak("Tak si naser");
				QuitGame();
			}
			else Tolk.Speak("Tak si hraj, tu máš kohouta.");
		}
	}
}