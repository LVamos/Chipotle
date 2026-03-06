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

	public class ProtobufSerializerHelper
	{
		/// <summary>
		/// Stores all NPCs.
		/// </summary>
		public readonly Dictionary<string, CharacterSave> Entities;

		/// <summary>
		/// stores all zones.
		/// </summary>
		public readonly Dictionary<string, ZoneSave> Zones;

		/// <summary>
		/// stores all game objects.
		/// </summary>
		public readonly Dictionary<string, ItemSave> Items;

		/// <summary>
		/// stores all passages.
		/// </summary>
		public readonly Dictionary<string, PassageSave> Passages;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="map">Whole map of the game world to be serialized</param>
		/// <param name="entities">All entities to be serialized</param>
		/// <param name="items">All objects to be serialized</param>
		/// <param name="passages">All passages to be serialized</param>
		public ProtobufSerializerHelper(
			Dictionary<string, CharacterSave> entities,
			Dictionary<string, ItemSave> items,
			Dictionary<string, PassageSave> passages,
			Dictionary<string, ZoneSave> zones)
		{
			Entities = entities;
			Items = items;
			Passages = passages;
			Zones = zones;
		}
	}
}