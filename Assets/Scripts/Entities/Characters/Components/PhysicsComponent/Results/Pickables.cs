using Game.Entities.Items;

using System.Collections.Generic;

namespace Game.Entities.Characters.Components.PhysicsComponent.Results
{
	/// <summary>
	/// A model for PhysicsComponent.GetPickableItemsBefore.
	/// </summary>
	public class Pickables
	{
		/// <summary>
		/// Pickable items before the NPC.
		/// </summary>
		public readonly List<Item> Items;

		/// <summary>
		/// Result of the search
		/// </summary>
		public PickablesResult Result;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="items">The list of items.</param>
		public Pickables(List<Item> items = null, PickablesResult result = PickablesResult.NothingFound)
		{
			Items = items;
			Result = result;
		}
	}
}