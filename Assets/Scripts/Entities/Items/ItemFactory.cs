using Game;
// No changes needed as the file already includes `using Assets.Scripts.Entities.Items`.
using Game.Entities.Items;
using Game.Models;
using Game.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

using Rectangle = Game.Terrain.Rectangle;

namespace Assets.Scripts.Entities.Items
{
	public class ItemFactory
	{
		private static Type GetItemCLRType(string itemtype)
		{
			Type result = null;
			_types.TryGetValue(itemtype, out result);
			return result;
		}

		private static string GetItemType(Type type)
		{
			KeyValuePair<string, Type> record = _types
				.FirstOrDefault(r => r.Value == type);
			if (record.Equals(default(KeyValuePair<string, Type>)))
				return null;
			return record.Key;
		}

		public static Item CreateAndActivate(
			Name name,
			string type,
			Rectangle area = default,
			bool decorative = false,
			bool pickable = false,
			bool usable = false,
			bool passable = false
			)
		{
			GameObject obj = new(name.Indexed);
			obj.AddComponent<Item>();
			Item item = CreateItem(obj, name, area, type, decorative, pickable, usable, passable);
			World.Add(item);
			item.Activate();
			return item;
		}

		public static Item CreateAndActivate<T>(
			Name name,
			Rectangle area = default,
			bool decorative = false,
			bool pickable = false,
			bool usable = false,
			bool passable = false)
		{
			GameObject obj = new(name.Indexed);
			obj.AddComponent(typeof(T));
			string type = GetItemType(typeof(T));
			Item item = CreateItem(obj, name, area, type, decorative, pickable, usable, passable);
			World.Add(item);
			item.Activate();
			return item;
		}

		public static Item AddComponent(GameObject obj, string type)
		{
			Item item = null;

			if (_types.TryGetValue(type, out Type itemType))
			{
				item = obj.AddComponent(itemType) as Item;
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
			YamlItemsModel items = null;
			YamlHelper.LoadFromResources(MainScript.ItemsPath, out items);
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
		public static Item CreateItem(GameObject obj, Name name, Rectangle area, string type, bool decorative = false, bool pickable = false, bool usable = false, bool passable = false)
		{
			if (name == null)
				throw new ArgumentNullException(nameof(name));
			if (string.IsNullOrWhiteSpace(type))
				throw new ArgumentNullException(nameof(type));

			Item item = null;

			Type clrType = GetItemCLRType(type);
			if (clrType != null)
			{
				item = obj.GetComponent(clrType) as Item;
				item.Initialize(name, area, type, decorative, pickable, usable: usable, passable: passable);
				return item;
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
					passable,
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
			item.Initialize(name, area, type, decorative, pickable, usable, passable);
			return item;
		}
	}
}