using System;
using Game.Entities.Items;
using Game.UI;

namespace Game.Models
{
	public class InventoryMenuResultModel
	{
		/// <summary>
		/// Selected action
		/// </summary>
		public readonly InventoryMenu.ActionType Action;

		/// <summary>
		/// Selected item
		/// </summary>
		public readonly Item Item;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="action">Selected action</param>
		/// <param name="item">selected item</param>
		public InventoryMenuResultModel(InventoryMenu.ActionType action, Item item)
		{
			Action = action;
			Item = item ?? throw new ArgumentNullException(nameof(item));
		}
	}
}