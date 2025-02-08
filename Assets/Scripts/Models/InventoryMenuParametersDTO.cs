using Game.Entities.Items;
using Game.UI;

using System;
using System.Collections.Generic;

namespace Assets.Scripts.Models
{
	public class InventoryMenuParametersDTO
	{
		public List<Item> Inventory { get; }
		public Action<int, InventoryAction> MenuClosed { get; }

		public InventoryMenuParametersDTO(
			List<Item> items,
			Action<int, InventoryAction> menuClosed)
		{
			Inventory = items ?? throw new ArgumentNullException(nameof(items));
			MenuClosed = menuClosed ?? throw new ArgumentNullException(nameof(menuClosed));
		}
	}
}
