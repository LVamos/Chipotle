using Game;
// No changes needed as the file already includes `using Assets.Scripts.Entities.Items`.
using Game.Entities.Items;
using Game.Models;

using System;
using System.Collections.Generic;
using System.IO;

using UnityEngine;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

using Rectangle = Game.Terrain.Rectangle;

namespace Assets.Scripts.Entities.Items
{
	public class ItemFactory : InteractiveItem
	{
		public static Item AddComponent(GameObject obj, string type)
		{
			Item item = null;

			if (_types.TryGetValue(type, out Type itemType))
			{
				item = obj.AddComponent(itemType) as InteractiveItem;
				return item;
			}

			if (_itemParameters.ContainsKey(type))
				item = obj.AddComponent<Item>() as Item;
			else
				item = obj.AddComponent<Item>();

			return item;
		}

		private static Dictionary<string, Type> _types = new();
		private static Dictionary<string, ItemCreationParametersModel> _itemParameters = new();

		/// <summary>
		/// Loads items from a YAML file.
		/// </summary>
		public static void LoadItems()
		{
			IDeserializer deserializer = new DeserializerBuilder()
				.WithNamingConvention(PascalCaseNamingConvention.Instance)
				.Build();

			string yamlText = File.ReadAllText(MainScript.ItemsPath);
			YamlItemsModel items = deserializer.Deserialize<YamlItemsModel>(yamlText);

			foreach (YamlItemModel item in items.Items)
			{
				if (string.IsNullOrWhiteSpace(item.Type))
					throw new ArgumentException("Item type must be specified.");

				if (!string.IsNullOrWhiteSpace(item.ClassName))
				{
					string className = $"Game.Entities.Items.{item.ClassName}";
					_types[item.Type] = System.Type.GetType(className);
					continue;
				}

				_itemParameters[item.Type] = new(
					item.CollisionSound,
					item.ActionSound,
					item.LoopSound,
					item.Cutscene,
					item.UsableOnce,
					item.AudibleOverWalls,
					item.Volume,
					item.StopWhenPlayerMoves,
					item.QuickActionsAllowed,
					item.PickingSound,
					item.PlacingSound
				);
			}
		}

		/// <summary>
		/// Creates a new item.
		/// </summary>
		/// <param name="name">The name of the item.</param>
		/// <param name="area">The area of the item.</param>
		/// <param name="type">The type of the item.</param>
		/// <param name="decorative">Specifies if the item is decorative.</param>
		/// <param name="pickable">Specifies if the item is pickable.</param>
		/// <param name="usable">Specifies if the item is usable.</param>
		public static Item CreateItem(GameObject obj, Name name, Rectangle area, string type, bool decorative = false, bool pickable = false, bool usable = false)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (string.IsNullOrWhiteSpace(type))
				throw new ArgumentNullException(nameof(type));

			Item item = null;

			if (_types.TryGetValue(type, out Type itemType))
			{
				InteractiveItem interactiveItem = obj.GetComponent(itemType) as InteractiveItem;
				interactiveItem.Initialize(name, area, type, decorative, pickable, usable);
				return interactiveItem;
			}

			ItemCreationParametersModel parameters;
			if (_itemParameters.TryGetValue(type, out parameters))
			{
				item = obj.GetComponent<Item>() as Item;
				item.Initialize(
					name,
					area,
					type,
					decorative,
					pickable,
					usable,
					parameters.CollisionSound,
					parameters.ActionSound,
					parameters.LoopSound,
					parameters.Cutscene,
					parameters.UsableOnce,
					parameters.AudibleOverWalls,
					parameters.Volume,
					parameters.StopWhenPlayerMoves,
					parameters.QuickActionsAllowed,
					parameters.PickingSound,
					parameters.PlacingSound
				);
				return item;
			}

			item = obj.GetComponent<Item>() as Item;
			item.Initialize(name, area, type, decorative, pickable, usable);
			return item;
		}
	}
}