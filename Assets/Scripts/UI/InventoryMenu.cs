using Assets.Scripts.Models;

using DavyKager;

using Game.Controls;
using Game.Entities.Items;
using Game.Messaging.Events.GameActions;
using Game.Models;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEditor;

using UnityEngine;

namespace Game.UI
{
	public class InventoryMenu : MenuWindow
	{
		protected override void SayItem(bool playSound = true, bool interruptSpeech = true)
		{
			if (SelectedItem == null)
				AssignSelectedObject();

			if (playSound)
				Play(_selectionSound);

			string text = SelectedItem.Name.Friendly;
			if (Settings.SayInnerItemNames)
				text += ", " + SelectedItem.Name.Inner;

			Tolk.Speak(text, interruptSpeech);
		}

		public override void OnActivate()
		{
			base.OnActivate();
			AssignSelectedObject();
		}

		protected override void FinalizeMenu()
		{
			_menuClosed?.Invoke(Index, Action);
		}

		private Action<int, InventoryAction> _menuClosed;

		public static InventoryMenu CreateInstance(InventoryMenuParametersDTO parameters)
		{
			GameObject obj = new();
			InventoryMenu menu = obj.AddComponent<InventoryMenu>();
			menu.Initialize(parameters);
			return menu;
		}

		/// <summary>
		/// The inventory from which the player selects an object.
		/// </summary>
		private List<Item> _inventory;

		/// <summary>
		/// The object selected for maniuplation.
		/// </summary>
		public Item SelectedItem { get; private set; }

		/// <summary>
		/// Executes the inventory menu with the given list of items.
		/// </summary>
		/// <param name="inventory">The list of items in the inventory.</param>
		/// <returns>InventoryMenuResultModel</returns>
		public static InventoryMenuResultModel Run(InventoryMenuParametersDTO parameters)
		{
			InventoryMenu menu = CreateInstance(parameters);
			WindowHandler.OpenModalWindow(menu);

			if (menu.SelectedItem == null)
				return null;
			return new(menu.Action, menu.SelectedItem);
		}

		/// <summary>
		/// The action selected by the player
		/// </summary>
		public InventoryAction Action { get; protected set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public void Initialize(InventoryMenuParametersDTO parameters)
		{
			MenuParameters remainingParams =
				new(
					null,
					"inventář",
					" ",
					0,
					false,
													introSound: "MenuItemActivated",
								outroSound: "MenuOpened",
		selectionSound: "MenuItemSelected",
		wrapDownSound: "MenuWrapped",
		wrapUpSound: "MenuWrapped",
		upperEdgeSound: "MenuEdge",
		lowerEdgeSound: "MenuEdge"
					);
			base.Initialize(remainingParams);

			_menuClosed = parameters.MenuClosed;

			// Prepare the menu items and sort them by picking time.
			_inventory = parameters.Inventory;
			_items =
				_inventory.Select(o => new List<string> { o.Name.Inner })
					.Reverse()
					.ToList();
		}

		protected override void AddShortcuts()
		{
			base.AddShortcuts();

			AddShortcut(CommandId.GameInteract, UseItem);
			AddShortcut(CommandId.GameExploreItem, ExploreItem);
			AddShortcut(CommandId.GamePlaceItem, PlaceItem);
			AddShortcut(CommandId.GameApplyItemToItem, ApplyItemToTarget);
		}

		private void ExploreItem()
		{
			ExplorationObjectSelected message = new(this, SelectedItem);
			World.Player.TakeMessage(message);
		}

		private void ApplyItemToTarget()
		{
			bool usable = SelectedItem.Usable || SelectedItem.UsableWith != null;
			if (!usable)
			{
				Tolk.Speak("Tohle se použít nedá");
				Close();
				return;
			}

			Action = InventoryAction.ApplyToTarget;
			ActivateItem();
		}

		/// <summary>
		/// Selects the current item as an object that should be put and quits the menu.
		/// </summary>
		private void PlaceItem()
		{
			Action = InventoryAction.Place;
			ActivateItem();
		}

		/// <summary>
		/// Selects the current item as an object that should be used and quits the menu.
		/// </summary>
		private void UseItem()
		{
			if (!SelectedItem.Usable)
			{
				Tolk.Speak("Tohle se použít nedá");
				Close();
				return;
			}

			Action = InventoryAction.Use;
			ActivateItem();
		}

		/// <summary>
		/// Quits the menu
		/// </summary>
		protected override void Quit()
		{
			base.Quit();
			Action = InventoryAction.None;
		}

		/// <summary>
		/// Uses the selected object.
		/// </summary>
		protected override void ActivateItem()
		{
			if (IndexOffEdge())
				return;

			base.ActivateItem();
		}

		/// <summary>
		/// A setter for the Index property.
		/// </summary>
		/// <param name="value"></param>
		protected override void SetIndex(int value)
		{
			base.SetIndex(value);

			if (!IndexOffEdge())
				AssignSelectedObject();
		}

		/// <summary>
		/// Identifies the currently selected object and assigns it to the SelectedObject property.
		/// </summary>
		protected void AssignSelectedObject()
		{
			SelectedItem = _inventory.First(o => o.Name.Inner == _items[_index][0]);
		}
	}
}