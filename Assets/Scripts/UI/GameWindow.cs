using Assets.Scripts.Models;

using Game.Entities;
using Game.Entities.Items;
using Game.Messaging;
using Game.Messaging.Commands.Physics;
using Game.Messaging.Commands.UI;
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
			List<MapElement> mapElements = message.Objects.Cast<MapElement>()
				.ToList();
			const string prompt = "Na co to chceš použít?";
			Entity selectedObject = ElementSelectionMenu(mapElements, prompt) as Entity;
			if (selectedObject == null)
				return;

			// Send message to the characterr.
			ApplyItemToTarget newMessage = new(this, message.ItemToApply, selectedObject);
			(message.Sender as MessagingObject).TakeMessage(newMessage);
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

		private void HandleInventoryAction(MessagingObject sender, List<Item> items, int option, InventoryMenu.ActionType action)
		{
			if (option == -1 || sender == null)
				return;

			Item selectedItem = items[option];

			switch (action)
			{
				case InventoryMenu.ActionType.Use:
					sender.TakeMessage(new Interact(this, selectedItem));
					break;
				case InventoryMenu.ActionType.ApplyToTarget:
					sender.TakeMessage(new ApplyItemToTarget(this, selectedItem));
					break;
				case InventoryMenu.ActionType.Place:
					sender.TakeMessage(new PlaceItem(this, selectedItem));
					break;
			}
		}

		private void OnSelectItemToPick(SelectItemToPick message)
		{
			List<MapElement> mapElements = message.Items.Cast<MapElement>().ToList();
			const string prompt = "Co chceš sebrat?";
			Item selectedItem = ElementSelectionMenu(mapElements, prompt) as Item;

			if (selectedItem == null)
				return;

			// Send message to the characterr.
			PickUpItem newMessage = new(this, selectedItem);
			MessagingObject sender = message.Sender as MessagingObject;
			sender.TakeMessage(newMessage);

		}

		private MapElement ElementSelectionMenu(List<MapElement> objects, string prompt)
		{
			if (objects.IsNullOrEmpty())
				throw new ArgumentException($"{objects} is null or empty");

			// Copy friendly names from the given objects into an array.
			List<List<string>> names =
				objects.Select(o => new List<string>() { o.Name.Friendly })
					.ToList();

			int option = WindowHandler.Menu(new(names, prompt, " ", 0, false));

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
			List<MapElement> mapElements = message.Objects.Cast<MapElement>().ToList();
			const string prompt = "Co chceš použít?";
			Entity selectedObject = ElementSelectionMenu(mapElements, prompt) as Entity;
			if (selectedObject == null)
				return;

			// Send message to the characterr.
			Interact newMessage = new(this, selectedObject);
			(message.Sender as MessagingObject).TakeMessage(newMessage);
		}

		/// <summary>
		/// Handles a message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		private void OnSelectObjectToExplore(SelectObjectToExplore message)
		{
			List<MapElement> mapElements = message.Objects.Cast<MapElement>().ToList();
			const string prompt = "Co chceš prozkoumat?";
			Entity selectedObject = ElementSelectionMenu(mapElements, prompt) as Entity;

			if (selectedObject == null)
				return;

			// Send message to the characterr.
			ExploreObject newMessage = new(this, selectedObject);
			(message.Sender as MessagingObject).TakeMessage(newMessage);
		}

		/// <summary>
		/// constructor
		/// </summary>
		public void Initialize()
		{
			_messagingEnabled = true;
			RegisterShortcuts(
				(new(KeyCode.Escape), QuitGame),
				(new(KeyShortcut.Modifiers.Control, KeyCode.Z), MainScript.SendFeedback)
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
		private void QuitGame()
		{
			World.QuitGame();
		}
	}
}