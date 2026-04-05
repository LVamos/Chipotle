// No changes needed as the file already includes `using Assets.Scripts.Entities.Items`.
using Game.Models;
using Game.Serialization;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

using UnityEngine;

using Rectangle = Game.Terrain.Rectangle;

namespace Game.Entities.Items
{
	public class ItemFactory
	{
		private static string GetAttribute(XElement element, string attribute, bool prepareForIndexing = true) => prepareForIndexing ? element.Attribute(attribute)?.Value.Sanitize() : element?.Attribute(attribute)?.Value;

		private static GameObject GetHostObject(Name name, bool create)
		{
			return !create
				? SceneObjects.GetItem(name.Inner)
				: SceneObjects.GetOrCreateItem(name.Inner);
		}

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
			GameObject obj = SceneObjects.Create(name.Inner, "Item");
			obj.AddComponent<Item>();
			Item item = Create(false, name, area, type, decorative, pickable, usable, passable);
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
			GameObject obj = new(name.Inner);
			obj.AddComponent(typeof(T));
			string type = GetItemType(typeof(T));
			Item item = Create(obj, name, area, type, decorative, pickable, usable, passable);
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
		public static void Init()
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

		public static Item Create(XElement itemNode, XElement zoneNode, bool createGameObject = false)
		{
			Rectangle zoneArea = new Rectangle(GetAttribute(zoneNode, "coordinates"));
			Name name = new(
				Extract("indexedname"),
				Extract("friendlyname"));
			Rectangle area = new Rectangle(Extract("coordinates")).ToAbsolute(zoneArea);
			string type = Extract("type");
			bool decorative = Extract("decorative").ToBool();
			bool pickable = Extract("pickable").ToBool();
			bool passable = Extract("passable") != null;
			bool usable = Extract("usable").ToBool();

			return Create(
				createGameObject,
				name,
				area,
				type,
				decorative,
				pickable,
				usable,
				passable);

			string Extract(string attributeName, bool sanitize = true)
					=> GetAttribute(itemNode, attributeName, sanitize);
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
		public static Item Create(
			bool createGameObject = false,
			Name name = null,
			Rectangle area = default,
			string type = null,
			bool decorative = false,
			bool pickable = false,
			bool usable = false,
			bool passable = false)
		{
			if (createGameObject)
			{
				if (name == null)
					throw new ArgumentNullException(nameof(name));
				if (string.IsNullOrWhiteSpace(type))
					throw new ArgumentNullException(nameof(type));
			}

			GameObject obj = GetHostObject(name.Inner, createGameObject);
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
				item = obj.GetComponent<Item>() as Item
				?? throw new InvalidOperationException($"Item creation failed: {type}");

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