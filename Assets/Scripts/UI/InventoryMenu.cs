using Assets.Scripts.Models;

using DavyKager;

using Game.Entities.Items;
using Game.Models;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

namespace Game.UI
{
	public class InventoryMenu : MenuWindow
	{
		protected override void FinalizeMenu()
		{
			_menuClosed?.Invoke(Index, Action);
		}

		private Action<int, ActionType> _menuClosed;

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
		/// Defines possible actions
		/// </summary>
		public enum ActionType
		{
			/// <summary>
			/// No action selected
			/// </summary>
			None = 0,

			/// <summary>
			/// Puts an object on the ground.
			/// </summary>
			Place,

			/// <summary>
			/// Uses the selected item.
			/// </summary>
			Use,

			/// <summary>
			/// Applies the selected item to another item.
			/// </summary>
			ApplyToTarget
		}

		/// <summary>
		/// The action selected by the player
		/// </summary>
		public ActionType Action { get; protected set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public void Initialize(InventoryMenuParametersDTO parameters)
		{
			base.Initialize(new(null, "inventář", " ", 0, false));
			_menuClosed = parameters.MenuClosed;

			// Prepare the menu items and sort them by picking time.
			_inventory = parameters.Inventory;
			_items =
				_inventory.Select(o => new List<string> { o.Name.Indexed })
					.Reverse()
					.ToList();

			// Add new key shortcuts
			RegisterShortcuts(
				(new(KeyCode.Return), UseObject),
				(new(KeyShortcut.Modifiers.Control, KeyCode.Return), PlaceItem),
				(new(KeyShortcut.Modifiers.ControlShift, KeyCode.Return), ApplyItemToTarget)
			);
		}

		private void ApplyItemToTarget()
		{
			if (!SelectedItem.Usable)
			{
				Tolk.Speak("Tohle se použít nedá");
				return;
			}

			Action = ActionType.ApplyToTarget;
			ActivateItem();
		}

		/// <summary>
		/// Selects the current item as an object that should be put and quits the menu.
		/// </summary>
		private void PlaceItem()
		{
			Action = ActionType.Place;
			ActivateItem();
		}

		/// <summary>
		/// Selects the current item as an object that should be used and quits the menu.
		/// </summary>
		private void UseObject()
		{
			if (!SelectedItem.Usable)
			{
				Tolk.Speak("Tohle se použít nedá");
				return;
			}

			Action = ActionType.Use;
			ActivateItem();
		}

		/// <summary>
		/// Quits the menu
		/// </summary>
		protected override void Quit()
		{
			base.Quit();
			Action = ActionType.None;
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
			SelectedItem = _inventory.First(o => o.Name.Indexed == _items[_index][0]);
		}

		/// <summary>
		/// Announces selected item using a screen reader or voice synthesizer
		/// </summary>
		protected override void SayItem()
		{
			Play(_selectionSound);
			Tolk.Speak(SelectedItem.Name.Friendly);
		}
	}
}