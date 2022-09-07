using Game.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Game.UI
{
	public class InventoryMenu: MenuWindow
	{

		/// <summary>
		/// The inventory from which the player selects an object.
		/// </summary>
		private DumpObject[] _inventory;

		/// <summary>
		/// The object selected for maniuplation.
		/// </summary>
		public DumpObject SelectedObject { get; private set; }


		public static (ActionType a, DumpObject o) Run(DumpObject[] inventory)
		{
			InventoryMenu menu = new InventoryMenu(inventory);
			WindowHandler.OpenModalWindow(menu);
			return (menu.Action, menu.SelectedObject);
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
			Put,

			/// <summary>
			/// Uses an object.
			/// </summary>
			Use
		}

		/// <summary>
		/// The action selected by the player
		/// </summary>
		public ActionType Action { get; protected set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public InventoryMenu(DumpObject[] inventory): base(null, "inventář", false)
		{
			// Prepare the menu items. 
			_inventory = inventory;
			_items =
				(from o in inventory
				 select o.Name.Friendly)
				.ToArray<string>();

			// Add new key shortcuts
			RegisterShortcuts(
(new KeyShortcut (Keys.Return), UseObject),
	(new KeyShortcut(KeyShortcut.Modifiers.Control, Keys.Return), PutObject)
);

		}

		/// <summary>
		/// Selects the current item as an object that should be put and quits the menu.
		/// </summary>
		private void PutObject()
		{
			Action = ActionType.Put;
			ActivateItem();
		}

		/// <summary>
		/// Selects the current item as an object that should be used and quits the menu.
		/// </summary>
		private void UseObject()
		{
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

		protected override void ActivateItem()
		{
			if (CursorOffEdge())
				return;

			// Identify the selected object.
			SelectedObject = 
			(from o in _inventory
			 where o.Name.Friendly == _items[Cursor]
			 select o)
				 .First();

			base.ActivateItem();
		}
	}
}
