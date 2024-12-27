using System.Collections.Generic;

namespace Game.Models
{
	/// <summary>
	/// Represents a list of game items loaded from a YAML file.
	/// </summary>
	public class YamlItemsModel
	{
		/// <summary>
		/// List of YamlItemModel objects.
		/// </summary>
		public List<YamlItemModel> Items { get; set; }
	}
}