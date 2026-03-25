using Game.Serialization.Protobuf.Snapshots.Characters;
using Game.Serialization.Protobuf.Snapshots.Entities.Items.Items;
using Game.Serialization.Protobuf.Snapshots.Spatial;
using Game.Serialization.Protobuf.Snapshots.Spatial.Passages;

using System.Collections.Generic;

namespace Game.Serialization.Protobuf
{
	/// <summary>
	/// A helper class that stores game map, NPCs and objects.
	/// </summary>
	public class GameSave
	{
		/// <summary>
		/// Stores all characters.
		/// </summary>
		public readonly HashSet<CharacterSave> Characters;

		/// <summary>
		/// stores all zones.
		/// </summary>
		public readonly HashSet<ZoneSave> Zones;

		/// <summary>
		/// stores all items.
		/// </summary>
		public readonly HashSet<ItemSave> Items;

		/// <summary>
		/// stores all passages.
		/// </summary>
		public readonly HashSet<PassageSave> Passages;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="characters">All characters</param>
		/// <param name="items">All items</param>
		/// <param name="passages">All passages</param>
		public GameSave(
			HashSet<CharacterSave> characters,
			HashSet<ItemSave> items,
			HashSet<PassageSave> passages,
			HashSet<ZoneSave> zones)
		{
			Characters = characters;
			Items = items;
			Passages = passages;
			Zones = zones;
		}
	}
}