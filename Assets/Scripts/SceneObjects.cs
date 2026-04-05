using System;
using System.Collections.Generic;

using UnityEngine;

namespace Game
{
	public static class SceneObjects
	{
		public static GameObject Create(string name, string tag)
		{
			GameObject obj = new(name);
			obj.tag = tag;
			_items[name] = obj;
			return obj;
		}

		private static readonly Dictionary<string, GameObject> _zones = new();
		private static readonly Dictionary<string, GameObject> _items = new();
		private static readonly Dictionary<string, GameObject> _passages = new();

		public static void Init()
		{
			_zones.Clear();
			_items.Clear();
			_passages.Clear();

			GameObject[] gameObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
			foreach (GameObject obj in gameObjects)
			{
				string name = obj.name;
				switch (obj.tag)
				{
					case "Locality": _zones[name] = obj; break;
					case "Item": _items[name] = obj; break;
					case "Passage": _passages[name] = obj; break;
				}
			}

			ValidateObjectsFound(_zones, "zones");
			ValidateObjectsFound(_items, "items");
			ValidateObjectsFound(_passages, "passages");
		}

		public static GameObject GetZone(string name) => GetObject(_zones, name, "Locality");

		public static GameObject GetOrCreateZone(string name) => GetOrCreateObject(_zones, name, "Locality");

		public static GameObject GetItem(string name) => GetObject(_items, name, "Item");

		public static GameObject GetOrCreateItem(string name) => GetOrCreateObject(_items, name, "Item");

		public static GameObject GetPassage(string name) => GetObject(_passages, name, "Passage");

		public static GameObject GetOrCreatePassage(string name) => GetOrCreateObject(_passages, name, "Passage");

		private static void ValidateObjectsFound(Dictionary<string, GameObject> objects, string objectType)
		{
			if (objects.Count == 0)
				throw new InvalidOperationException($"No geometry for {objectType} found in the scene.");
		}

		private static GameObject GetObject(Dictionary<string, GameObject> objects, string name, string objectType)
		{
			if (!objects.TryGetValue(name, out GameObject obj))
				throw new Exception($"{objectType} not found: {name}");

			return obj;
		}

		private static GameObject GetOrCreateObject(Dictionary<string, GameObject> objects, string name, string tag)
		{
			if (!objects.TryGetValue(name, out GameObject obj))
			{
				obj = new(name);
				obj.tag = tag;
				objects[name] = obj;
			}

			return obj;
		}
	}
}
