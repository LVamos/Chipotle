using Game.Entities.Items;

namespace Game.Models
{
	/// <summary>
	/// Model for ChipotlePhysicsComponent.GetNavigableItems method
	/// </summary>
	public class NavigableItemsModel
	{
		/// <summary>
		/// Object descriptions
		/// </summary>
		public readonly string[] Descriptions;

		/// <summary>
		/// Detected objects
		/// </summary>
		public readonly Item[] Items;

		/// <summary>
		/// Initializes a new instance of the <see cref="NavigableItemsModel"/> class with descriptions and objects.
		/// </summary>
		/// <param name="descriptions">The descriptions of the items.</param>
		/// <param name="objects">The items.</param>
		public NavigableItemsModel(string[] descriptions, Item[] objects)
		{
			Descriptions = descriptions;
			Items = objects;
		}
	}
}