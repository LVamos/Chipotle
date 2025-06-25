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

		private void HandleExploringMenu(MessagingObject sender, List<Entity> objects, int option)
		{
			if (option == -1 || sender == null)
				return;

			ExploreObject message = new(this, objects[option]);
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