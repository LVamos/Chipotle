using Assets.Scripts.Models;

using Game.Serialization;

using System;
using System.Collections.Generic;
using System.Xml.Linq;

using UnityEngine;

namespace Game.Terrain
{
	public static class ZoneFactory
	{
		private static string GetAttribute(XElement element, string attribute, bool prepareForIndexing = true) => prepareForIndexing ? element.Attribute(attribute)?.Value.Sanitize() : element?.Attribute(attribute)?.Value;

		public static Zone Create(XElement zoneNode, bool createGameObject = false)
		{
			Name name = new(
				Extract("indexedname"),
				Extract("friendlyname"));
			string description = Extract("description", false);
			string to = Extract("to");
			ZoneType type = Extract("type").ToZoneType();
			float height = int.Parse(Extract("height"));
			Rectangle area = new(Extract("coordinates"));
			TerrainType defaultTerrain = Extract("defaultTerrain", false).ToTerrainType();

			if (type == ZoneType.Outdoor)
				height = 6;

			Zone zone = ZoneFactory.Create(createGameObject,
								name,
				description,
				to,
				type,
				height,
				area,
				defaultTerrain
				);
			return zone;

			string Extract(string attributeName, bool sanitize = true)
				=> GetAttribute(zoneNode, attributeName, sanitize);
		}

		/// <summary>
		/// Map of zones and corresponding background sounds
		/// </summary>
		private static Dictionary<string, ZoneLoopInfo> _zoneLoops;

		private static void LoadLoops()
		{
			Dictionary<string, ZoneLoopInfo> loops = null;
			YamlHelper.LoadFromResources(MainScript.ZoneLoopsPath, out loops);
			_zoneLoops = new Dictionary<string, ZoneLoopInfo>(loops, StringComparer.OrdinalIgnoreCase);
		}

		public static Zone Create(
				bool createGameObject = false,
			Name name = null,
				string description = null,
				string to = null,
				ZoneType type = default,
				float height = 0,
				Rectangle area = default,
				TerrainType defaultTerrain = default)
		{
			GameObject gameObject = GetHostObject(name, createGameObject);
			Zone zone = gameObject.GetComponent<Zone>();
			ZoneMaterials zoneMaterials = null;
			_materials.TryGetValue(name.Inner, out zoneMaterials);
			ZoneLoopInfo loopInfo;
			_zoneLoops.TryGetValue(name.Inner, out loopInfo);

			zone.Initialize(
				name,
				description,
				to,
				type,
				height,
				area,
				defaultTerrain,
				loopInfo,
				zoneMaterials
			);
			return zone;
		}

		private static GameObject GetHostObject(Name name, bool create)
		{
			return !create
				? SceneObjects.GetZone(name.Inner)
				: SceneObjects.GetOrCreateZone(name.Inner);
		}

		public static void Init()
		{
			LoadZoneMaterials();
			LoadLoops();
		}

		private static Dictionary<string, ZoneMaterials> _materials;

		private static void LoadZoneMaterials()
		{
			Dictionary<string, ZoneMaterials> materials = null;
			YamlHelper.LoadFromResources(MainScript.MaterialsPath, out materials);
			_materials = new(materials, StringComparer.OrdinalIgnoreCase);
		}

	}
}
