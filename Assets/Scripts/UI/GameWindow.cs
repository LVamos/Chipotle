using Assets.Scripts.Models;

using Game.Entities;
using Game.Entities.Items;
using Game.Messaging;
using Game.Messaging.Commands.Physics;
using Game.Messaging.Commands.UI;
using Game.Messaging.Events.GameManagement;
using Game.Messaging.Events.Input;
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
			// Run the menu
			List<List<string>> items = _gameMenuOptions.Select(c => new List<string>() { c.Key }).ToList();
			Action<int> menuHandler = (option) =>
			{
				HandleGameMenu(items, option, message.Sender as MessagingObject);
			};
			MenuParametersDTO parameters = new(
				items,
				"Menu",
				" ",
				0,
				false,
				menuClosed: menuHandler);
			WindowHandler.Menu(parameters);
		}

		private void HandleGameMenu(List<List<string>> items, int option, MessagingObject initiator)
		{
			string optionDescription = items[option][0];
			string optionId = _gameMenuOptions[optionDescription];
			GameMenuOptionselected message = new(this, optionId);
			initiator.TakeMessage(message);
		}

		public static GameWindow CreateInstance()
		{
			GameObject obj = new();
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
			List<List<string>> names = GetFriendlyNames(message.Objects);
			Action<int> action =
				(option) => HandleApplyItemMenu(message.Sender as MessagingObject, message.Objects, option, message.ItemToApply);
			MenuParametersDTO parameters = new(
							names,
							prompt,
							wrappingAllowed: false,
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


		private void HandleInteractionMenu(MessagingObject sender, List<Entity> objects, int option)
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
			MenuParametersDTO parameters = new(
							names,
							prompt,
							wrappingAllowed: false,
							menuClosed: action);
			WindowHandler.Menu(parameters);
		}

		private void HandleApplyItemMenu(MessagingObject sender, List<Entity> objects, int option, Item itemToApply)
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

			MenuParametersDTO parameters = null;
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
			List<List<string>> names = GetFriendlyNames(message.Objects);

			MenuParametersDTO parameters = new(
				names,
				prompt,
				wrappingAllowed: false,
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
			MenuParametersDTO parameters = new(
							names,
							prompt,
							wrappingAllowed: false,
							menuClosed: (option) => HandleExploringMenu(message.Sender as MessagingObject, message.Objects, option));
			WindowHandler.Menu(parameters);
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
			_messagingEnabled = true;
			RegisterShortcuts(
				(new(KeyCode.Escape), QuitGame),
				(new(KeyShortcut.Modifiers.Control, KeyCode.Y), MainScript.SendFeedback)
			);
		}

		/// <summary>
		/// Processes the KeyDown message.
		/// </summary>
		/// <param name="e">The message to be handled</param>
		public override void OnKeyDown(KeyShortcut shortcut)
		{
			base.OnKeyDown(shortcut);

			if (World.GameInProgress)
				World.Player.TakeMessage(new KeyPressed(this, shortcut));
		}

		/// <summary>
		/// Processes the KeyUpmessage.
		/// </summary>
		/// <param name="shortcut">The message to be handled</param>
		public override void OnKeyUp(KeyShortcut shortcut)
		{
			if (World.GameInProgress && World.Player != null)
				World.Player.TakeMessage(new KeyReleased(this, shortcut));
		}

		/// <summary>
		/// Quits the game.
		/// </summary>
		private void QuitGame() => World.QuitGame();
	}
}