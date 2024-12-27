using System.Collections.Generic;
using Game.Entities.Characters;
using Game.Entities.Items;
using Game.Terrain;
using ProtoBuf;

namespace Game
{
	/// <summary>
	/// A helper class that stores game map, NPCs and objects.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	public class SerializerHelper
	{
		/// <summary>
		/// Stores all NPCs.
		/// </summary>
		public readonly Dictionary<string, Character> Entities;

		/// <summary>
		/// stores all localities.
		/// </summary>
		public readonly Dictionary<string, Locality> Localities;

		/// <summary>
		/// stores all game objects.
		/// </summary>
		public readonly Dictionary<string, Item> Objects;

		/// <summary>
		/// stores all passages.
		/// </summary>
		public readonly Dictionary<string, Passage> Passages;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="map">Whole map of the game world to be serialized</param>
		/// <param name="entities">All entities to be serialized</param>
		/// <param name="objects">All objects to be serialized</param>
		/// <param name="passages">All passages to be serialized</param>
		public SerializerHelper(Dictionary<string, Character> entities, Dictionary<string, Item> objects, Dictionary<string, Passage> passages, Dictionary<string, Locality> localities)
		{
			Entities = entities;
			Objects = objects;
			Passages = passages;
			Localities = localities;
		}
	}
}