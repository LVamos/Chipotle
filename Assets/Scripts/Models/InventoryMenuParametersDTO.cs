using Game.Entities.Items;
using Game.UI;

using System;
using System.Collections.Generic;

namespace Assets.Scripts.Models
{
	public class InventoryMenuParametersDTO
	{
		public List<Item> Inventory { get; }
		public Action<int, InventoryMenu.ActionType> MenuClosed { get; }

		public InventoryMenuParametersDTO(
			List<Item> items,
			Action<int, InventoryMenu.ActionType> menuClosed)
		{
			Inventory = items ?? throw new ArgumentNullException(nameof(items));
			MenuClosed = menuClosed ?? throw new ArgumentNullException(nameof(menuClosed));
		}
	}
}
