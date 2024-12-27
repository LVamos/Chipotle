using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using UnityEngine;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Game.Terrain
{
	public static class PassageFactory
	{
		public static Passage AddComponent(GameObject obj, string name, bool isDoor)
		{
			if (_types.TryGetValue(name, out Type itemType))
				return obj.AddComponent(itemType) as Passage;

			if (isDoor)
				return obj.AddComponent<Door>() as Door;

			return obj.AddComponent<Passage>() as Passage;
		}

		private static Dictionary<string, Type> _types;

		/// <summary>
		/// Loads items from a YAML file.
		/// </summary>
		public static void LoadPassages()
		{
			var deserializer = new DeserializerBuilder()
				.WithNamingConvention(PascalCaseNamingConvention.Instance)
				.Build();

			var yamlText = File.ReadAllText(MainScript.PassagesPath);
			Dictionary<string, string> types = deserializer.Deserialize<Dictionary<string, string>>(yamlText);
			if (types.Any(p => string.IsNullOrWhiteSpace(p.Key) || string.IsNullOrWhiteSpace(p.Value)))
				throw new ArgumentException("Invalid record.");

			_types = types.ToDictionary(p => p.Key, p => Type.GetType($"Game.Terrain.{p.Value}"));
		}

		/// <summary>
		/// Creates new instance of a passage according to the given parameters.
		/// </summary>
		/// <param name="name">Inner name of the passage</param>
		/// <param name="area">Coordinates of the are occupied by the passage</param>
		/// <param name="localities">Localities connectedd by the passage</param>
		/// <param name="isDoor">Specifies if the passage is a door.</param>
		/// <param name="state">State of a door</param>
		/// <param name="doorType">Type of a door</param>
		/// <returns>A new instance of the passage</returns>
		public static Passage CreatePassage(GameObject obj, Name name, Rectangle area, IEnumerable<string> localities, bool isDoor, PassageState state, Door.DoorType doorType)
		{
			if (name == null || string.IsNullOrWhiteSpace(name.Indexed))
				throw new ArgumentNullException(nameof(name));
			if (localities.IsNullOrEmpty() || localities.Count() != 2)
				throw new ArgumentException("Invalid localities.");

			Passage passage = obj.GetComponent<Passage>();

			if (passage is not Passage && passage is not Door)
				passage.Initialize(name, area, localities);
			else if (passage is Door)
				(passage as Door).Initialize(name, state, area, localities, doorType);
			else passage.Initialize(name, area, localities);

			return passage;
		}
	}
}
