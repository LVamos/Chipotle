using Game.Entities.Items;

using System.Collections.Generic;

namespace Game.Models
{
	/// <summary>
	/// A model for PhysicsComponent.GetPickableItemsBefore.
	/// </summary>
	public class PickableItems
	{
		/// <summary>
		/// Result of the search.
		/// </summary>
		public enum ResultType
		{
			/// <summary>
			/// No items before the NPC and in range
			/// </summary>
			NothingFound,
			/// <summary>
			/// Only unpickable items before the NPC
			/// </summary>
			Unpickable,
			/// <summary>
			/// Pickable items before the NPC
			/// </summary>
			Success
		};

		/// <summary>
		/// Pickable items before the NPC.
		/// </summary>
		public readonly List<Item> Items;

		/// <summary>
		/// Result of the search
		/// </summary>
		public ResultType Result;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="items">The list of items.</param>
		public PickableItems(List<Item> items = null, ResultType result = ResultType.NothingFound)
		{
			Items = items;
			Result = result;
		}
	}
}