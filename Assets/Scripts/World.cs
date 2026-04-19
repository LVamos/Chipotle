using Assets.Scripts;
using Assets.Scripts.Spatial;

using Game.Audio;
using Game.Entities;
using Game.Entities.Characters;
using Game.Entities.Items;
using Game.Mapping.Saves;
using Game.Messaging.Events.GameManagement;
using Game.Serialization.Protobuf;
using Game.Serialization.Protobuf.Snapshots.Characters;
using Game.Serialization.Protobuf.Snapshots.Entities.Items.Items;
using Game.Serialization.Protobuf.Snapshots.Spatial;
using Game.Serialization.Protobuf.Snapshots.Spatial.Passages;
using Game.Terrain;
using Game.UI;

using ProtoBuf.Meta;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;

using UnityEditor;

using UnityEngine;

using Message = Game.Messaging.Message;
using Rectangle = Game.Terrain.Rectangle;

namespace Game
{
	/// <summary>
	/// Represents the game world.
	/// </summary>
	public static class World
	{
		public static GameSave CreateSave()
		{
			return new(
								CameraManager.GetOrientation()
								.ToVector3Save(),
				_characters.Values.Select(c => c.Export()).ToHashSet(),
				_items.Values.Select(i => i.Export()).ToHashSet(),
				_passages.Values.Select(p => p.Export()).ToHashSet(),
				_zones.Values.Select(z => z.Export()).ToHashSet()
				);
		}

		public static bool ZoneHasPath(Zone start, Zone goal)
		{
			if (start == goal)
				return true;
			if (start == null)
				throw new ArgumentNullException(nameof(start));
			if (goal == null)
				throw new ArgumentNullException(nameof(goal));

			Queue<Zone> queue = new Queue<Zone>();
			HashSet<Zone> visited = new HashSet<Zone>();

			queue.Enqueue(start);
			visited.Add(start);

			while (queue.Count > 0)
			{
				Zone current = queue.Dequeue();

				if (current == goal)
					return true;

				foreach (Passage passage in current.Exits)
				{
					if (!passage.Open)
						continue;

					Zone neighbor = passage.AnotherZone(current);
					if (neighbor == null || visited.Contains(neighbor))
						continue;

					visited.Add(neighbor);
					queue.Enqueue(neighbor);
				}
			}

			return false;
		}


		/// <summary>
		/// Generates a text representation of the specified distance in Czech.
		/// </summary>
		/// <param name="distance">The distance in meters to be described</param>
		/// <returns>The text representation of the specified distance</returns>
		public static string GetDistanceDescription(float distance, float stepLength)
		{
			if (distance <= stepLength)
				return string.Empty;

			// Round the distance so that its value corresponds to a multiple of 0.5.
			int steps = (int)Mathf.Round(distance / stepLength);

			// Compose output
			if (steps == 1)
				return "jeden krok";
			if (steps is > 1 and < 5)
				return $"{steps} kroky";
			return $"{steps} kroků";
		}

		/// <summary>
		/// Check if two map elements are within a specified radius of each other.
		/// </summary>
		/// <returns>True if the two elements are within the specified radius of each other</returns></returns>
		public static bool IsInRange(MapElement a, MapElement b, float radius) => GetDistance(a, b) <= radius;

		/// <summary>
		/// Determines if element 'a' is closer than element 'b'.
		/// </summary>
		/// <param name="referenceElement">The element to which the distance should be calculated</param>
		/// <param name="a">First map element to compare</param>
		/// <param name="b">Second map element to compare</param>
		/// <returns>True if element a is closer than element b</returns>
		public static bool IsCloser(MapElement referenceElement, MapElement a, MapElement b) => GetDistance(referenceElement, a) <= GetDistance(referenceElement, b);

		/// <summary>
		/// Finds the closest element in a collection to a given element.
		/// </summary>
		/// <param name="elements">The collection of elements to be searched</param>
		/// <param name="element">The element to which the closest element should be found</param>
		/// <returns>The closest element</returns>
		public static MapElement GetClosestElement(IEnumerable<MapElement> elements, MapElement element)
		{
			return
				(from e in elements
				 let distance = GetDistance(element, e)
				 orderby distance
				 select e)
				.First();
		}

		/// <summary>
		/// Returns a text description for the specified object.
		/// </summary>
		/// <param name="object">A character or object</param>
		/// <param name="id">Numeric identifier of the requested caption</param>
		/// <returns>A string containing the requested description</returns>
		public static string GetObjectDescription(Entity @object, int id)
		{
			string[] result = null;
			if (_objectDescriptions.TryGetValue(@object.Type, out result))
				return result[id];

			return "popis chybí";
		}

		/// <summary>
		/// Contains descriptions for object templates.
		/// </summary>
		private static Dictionary<string, string[]> _objectDescriptions = new();

		/// <summary>
		/// Enumerates all zones in an area specified by the area code.
		/// </summary>
		/// <param name="areaCode">Part of zone indexed name that specifies the containing area</param>
		/// <returns>enumeration of zones</returns>
		public static IEnumerable<Zone> GetZonesInArea(string areaCode)
		{
			return
				from l in GetZones()
				let name = l.Name.Inner
				let position = name.Length - 2
				where name.Substring(position, 1).ToLower() == "h"
				select l;
		}

		private static readonly float _cutsceneVolume = 1;

		/// <summary>
		/// Replays the last cutscene
		/// </summary>
		public static void RepeatCutscene()
		{

		}

		/// <summary>
		/// Enumerates all zones.
		/// </summary>
		/// <returns>Enumeration of all zones</returns>
		public static IEnumerable<Zone> GetZones() => _zones.Values.AsEnumerable();

		/// <summary>
		/// Enumerates all zones intersecting with the speciifed area.
		/// </summary>
		/// <param name="area">The area the zones should intersect with</param>
		/// <returns>enumeration of zones</returns>
		public static IEnumerable<Zone> GetZones(Rectangle area)
		{
			IEnumerable<Zone> zones = _zones.Values
				.Where(l => l.Area.Value.Intersects(area))
				.Distinct();
			return zones;
		}

		/// <summary>
		/// Interval between game loop ticks
		/// </summary>
		public static int DeltaTime => (int)(1000 * Time.deltaTime);

		/// <summary>
		/// sets speed of the game loop.
		/// </summary>
		public const int FramesPerSecond = 33;

		/// <summary>
		/// Queue of actions that should be performed at the beginning of the game loop tick
		/// </summary>
		private static readonly Queue<Action> _delayedActions = new();

		/// <summary>
		/// List of all NPCs
		/// </summary>
		public static Dictionary<string, Character> _characters;

		/// <summary>
		/// List of all zones
		/// </summary>
		private static Dictionary<string, Zone> _zones;

		/// <summary>
		/// List of all simple game objects
		/// </summary>
		private static Dictionary<string, Item> _items;

		/// <summary>
		/// List of all passages
		/// </summary>
		private static Dictionary<string, Passage> _passages;

		/// <summary>
		/// The game map
		/// </summary>
		public static TileMap Map { get; private set; }

		/// <summary>
		/// Reference to the Detective Chipotle NPC (the main NPC)
		/// </summary>
		public static Character Player;

		/// <summary>
		/// Registers an entity.
		/// </summary>
		/// <param name="c">The entity to be registered</param>
		public static void Add(Character c)
		{
			// Do a null check and look if entity isn't already registered.
			if (c == null)
				throw new ArgumentNullException(nameof(c));

			if (_characters.ContainsKey(c.Name.Inner))
				throw new ArgumentException("entity already registered");

			_characters.Add(c.Name.Inner, c);
		}

		/// <summary>
		/// Registers a game object.
		/// </summary>
		/// <param name="o">The game object to be added</param>
		public static void Add(Item o)
		{
			// Do a null check and look if object isn't already registered.
			if (o == null)
				throw new ArgumentNullException(nameof(o));

			if (_items.ContainsKey(o.Name.Inner))
				throw new ArgumentException("Object already registered");

			_items.Add(o.Name.Inner, o);
		}

		/// <summary>
		/// Registers a zone.
		/// </summary>
		/// <param name="zone">The zone to be registered</param>
		public static void Add(Zone zone)
		{
			// null check
			if (zone == null)
				throw new ArgumentNullException(nameof(zone));

			// Isn't the zone already registered?
			if (_zones.ContainsKey(zone.Name.Inner))
				throw new ArgumentException("Zone already registered");

			_zones[zone.Name.Inner] = zone;
			Map.RegisterZone(zone);
		}

		/// <summary>
		/// Registers a passage.
		/// </summary>
		/// <param name="p">The passage to be registered</param>
		public static void Add(Passage p)
		{
			if (p == null)
				throw new ArgumentNullException(nameof(p));

			if (_passages.ContainsKey(p.Name.Inner))
				throw new ArgumentException("Passage already registered");

			_passages.Add(p.Name.Inner, p);
		}

		/// <summary>
		/// Calculates an angle between two point acording to the specified orientation.
		/// </summary>
		/// <param name="a">The first point</param>
		/// <param name="b">The second point</param>
		/// <param name="orientation">The orientation according to which the angle should be calculated</param>
		/// <returns>Angle</returns>
		public static Angle GetAngle(Vector2 a, Vector2 b, Orientation2D orientation)
		{
			double x = a.x - b.x;
			double y = a.y - b.y;
			double z = Math.Round(Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)));
			Angle angle = new Angle(Math.Atan2(y, x)) + Angle.FromCartesianDegrees(orientation.Angle.CompassDegrees);
			return angle;
		}

		/// <summary>
		/// Calculates the distance between two MapElements.
		/// </summary>
		/// <param name="element1">The first MapElement.</param>
		/// <param name="element2">The second MapElement.</param>
		/// <returns>Distance between two map elements in meters</returns>
		public static float GetDistance(MapElement element1, MapElement element2) => element1.Area.Value.GetDistanceFrom(element2.Area.Value);

		/// <summary>
		/// Computes cartesian distance between two points.
		/// </summary>
		/// <param name="a">First point</param>
		/// <param name="b">Second point</param>
		/// <returns>Rounded distance between two given points</returns>
		public static float GetDistance(Vector2 a, Vector2 b) => Vector2.Distance(a, b);

		/// <summary>
		/// Checks if the squared distance between two points is less than or equal to a given distance.
		/// </summary>
		public static bool IsWithinDistance(Vector2 a, Vector2 b, float distance)
		{
			float squaredDistance = GetSquareDistance(a, b);
			return squaredDistance <= distance * distance;
		}

		public static float GetSquareDistance(Vector2 a, Vector2 b)
		{
			Vector2 delta = a - b;
			return delta.sqrMagnitude;  // dx*dx + dy*dy

		}

		/// <summary>
		/// Computes cartesian distance between two 3d points.
		/// </summary>
		/// <param name="a">First point</param>
		/// <param name="b">Second point</param>
		/// <returns>Distance between two given points</returns>
		public static float GetDistance(Vector3 a, Vector3 b) => (float)(Math.Pow(Math.Abs(a.x - b.x), 2) + Math.Pow(Math.Abs(a.y - b.y), 2) + Math.Pow(Math.Abs(a.z - b.z), 2)) * 0.5f;

		/// <summary>
		/// Returns an entity that stands on the given position.
		/// </summary>
		/// <param name="point"></param>
		/// <returns>Reference to the entity if there's any</returns>
		public static Character GetCharacter(Vector2 point)
		{
			Zone zone = GetZone(point);
			return zone == null
				? null
				: (
					from e in zone.Characters
					where e.Area != null && e.Area.Value.Contains(point)
					select e)
				.FirstOrDefault();
		}

		/// <summary>
		/// Enumerates all entities that intersect with the given plane.
		/// </summary>
		/// <param name="area">The plane to be checked.</param>
		/// <returns>Enumeration of intersecting entities</returns>
		public static List<Character> GetCharacters(Rectangle area)
			=> _characters.Values
			.Where(o => o.Area != null && o.Area.Value.Intersects(area))
			.ToList();

		/// <summary>
		/// Returns an NPC found by its name.
		/// </summary>
		/// <param name="name">Inner name of the required NPC</param>
		/// <returns>The found NPC or null if nothing was found</returns>
		public static Character GetCharacter(string name)
		{
			if (_characters.TryGetValue(name, out Character c))
				return c;
			return null;
		}

		/// <summary>
		/// Returns a zone found by its name.
		/// </summary>
		/// <param name="name">Inner name of the required zone</param>
		/// <returns>The found zone or null if nothing was found</returns>
		public static Zone GetZone(string name)
		{

			_zones.TryGetValue(name, out Zone zone);
			return zone;
		}

		/// <summary>
		/// Returns a zone that fully intersects with the given plane.
		/// </summary>
		/// <param name="area">The point tto be checked</param>
		/// <returns>The intersecting zone</returns>
		public static Zone GetZone(Rectangle area) => _zones.Values.FirstOrDefault(l => l.Area.Value.Contains(area));

		/// <summary>
		/// Returns a zone which intersects with the given point.
		/// </summary>
		/// <param name="point">The point tto be checked</param>
		/// <returns>The intersecting zone</returns>
		public static Zone GetZone(Vector2 point)
		{
			Zone zone = _zones.Values.FirstOrDefault(z => matches(point, z));
			return zone;

			bool matches(Vector2 point, Zone zone) => zone.Area.Value.Contains(point);
		}

		/// <summary>
		/// Enumerates all zones sorted by distance from the specified point.
		/// </summary>
		/// <param name="point">The point whose surroundings should be explored</param>
		/// <returns>Enumeration of the found zones</returns>
		public static List<Zone> GetNearestZones(Vector2 point, float? radius = null, bool includeDefaultZone = false)
		{
			Zone defaultZone = GetZone(point);

			List<Zone> zones =
	(from zone in _zones.Values
	 where zone != defaultZone
	 let distance = zone.Area.Value.GetDistanceFrom(point)
	 where radius == null || distance <= radius.Value
	 orderby distance
	 select zone)
	.ToList();

			if (includeDefaultZone)
				zones.Add(defaultZone);

			return zones;
		}

		/// <summary>
		/// Returns the zone nearest from the specified point.
		/// </summary>
		/// <param name="point">The point shose surroundings is to be searched</param>
		/// <returns>The found zone</returns>
		public static Zone GetNearestZone(Vector2 point) => GetNearestZones(point).First();

		/// <summary>
		/// Returns a game object closest to the specified point.
		/// </summary>
		/// <param name="point">The point whose surroundings is to be searched</param>
		/// <returns>The found game object</returns>
		public static Entity GetNearestObject(Vector2 point) => GetNearestObjects(point).FirstOrDefault();

		/// <summary>
		/// Enumerates all game objects around a point sorted by distance.
		/// </summary>
		/// <param name="point">A point whose surroundings are to be searched</param>
		/// <returns>Enumeration of game objects</returns>
		public static IEnumerable<Item> GetNearestObjects(Vector2 point)
		{
			return
				(from o in _items.Values
				 where o.Area != null && !o.Area.Value.Contains(point)
				 orderby o.Area.Value.GetDistanceFrom(point)
				 select o)
				.Distinct();
		}

		/// <summary>
		/// Enumerates all game objects around a point sorted by distance in the specified radius.
		/// </summary>
		/// <param name="point">A point whose surroundings are to be searched</param>
		/// <param name="radius">specifies maximum distance between the objects and the specified point.</param>
		/// <param name="includeDecoaration">Specifies if decorative objects should be included</param>
		/// <returns>Enumeration of game objects</returns>
		public static IEnumerable<Item> GetNearestObjects(Vector2 point, int radius, bool includeDecoaration = true)
		{
			return
				(from o in _items.Values
				 let distance = o.Area.Value.GetDistanceFrom(point)
				 orderby distance
				 where o.Area != null && o.Decorative == includeDecoaration && !o.Area.Value.Contains(point) && distance <= radius
				 select o)
				.Distinct();
		}

		/// <summary>
		/// Returns the passage closest to the specified point.
		/// </summary>
		/// <param name="point">The point whose surrounding is to be searched</param>
		/// <returns>The found passage</returns>
		public static Passage GetNearestPassage(Vector2 point) => GetNearestPassages(point).FirstOrDefault();

		/// <summary>
		/// Enumerates all passages sorted by distance from the specified point.
		/// </summary>
		/// <param name="point">The point whose surrounding is to be searched</param>
		/// <param name="doors">Specifies if the result should should be narrowed to doors only</param>
		/// <returns>Enumeration of all passages</returns>
		public static IEnumerable<Passage> GetNearestPassages(Vector2 point)
		{
			return
				(from p in _passages.Values
				 let distance = p.Area.Value.GetDistanceFrom(point)
				 where !p.Area.Value.Contains(point)
				 orderby distance
				 select p);
		}

		/// <summary>
		/// Enumerates all passages sorted by distance from the specified point.
		/// </summary>
		/// <param name="point">The point whose surrounding is to be searched</param>
		/// <param name="maxDistance">Max allowed distance from the specified point</param>
		/// <returns>Enumeration of all passages</returns>
		public static IEnumerable<Passage> GetNearestPassages(Vector2 point, float maxDistance)
		{
			return GetNearestPassages(point)
						.Where(p => p.Area.Value.GetDistanceFrom(point) <= maxDistance);
		}

		/// <summary>
		/// Enumerates nearest doors around the specified point sorted by distance.
		/// </summary>
		/// <param name="point">The point whose surrounding is to be searched</param>
		/// <param name="maxDistance">Max allowed distance from the specified point</param>
		/// <returns>Enumeration of doors</returns>
		public static IEnumerable<Character> GetNearestCharacters(Vector2 point, float maxDistance)
		{
			IEnumerable<Character> result =
							from character in _characters.Values
							let distance = character.Area.Value.GetDistanceFrom(point)
							where distance <= maxDistance
							orderby distance
							select character;
			return result;
		}

		/// <summary>
		/// Enumerates nearest doors around the specified point sorted by distance.
		/// </summary>
		/// <param name="point">The point whose surrounding is to be searched</param>
		/// <param name="maxDistance">Max allowed distance from the specified point</param>
		/// <returns>Enumeration of doors</returns>
		public static List<Door> GetNearestDoors(Vector2 point, float maxDistance)
		{
			List<Door> doors = GetNearestPassages(point)
				.OfType<Door>()
				.ToList();
			IEnumerable<Door> result =
							from door in doors
							let distance = door.Area.Value.GetDistanceFrom(point)
							where distance <= maxDistance
							select door;
			return result.ToList();
		}

		/// <summary>
		/// searches for a simple game object by name.
		/// </summary>
		/// <param name="name">Inner name of the required object</param>
		/// <returns>The found game object or null if nothing was found</returns>
		public static Item GetItem(string name) => _items.TryGetValue(name, out Item o) ? o : null;

		/// <summary>
		/// Returns an NPC or game object the tile intersects.
		/// </summary>
		public static Item GetItem(Vector2 point)
		{
			Zone zone = GetZone(point);

			if (zone == null
				|| zone.IsWalkable(point))
				return null;

			return
				 (from o in zone.Items
				  let notHidden = o.Area != null
				  let contains = o.Area.Value.Contains(point)
				  where notHidden && contains
				  select o)
				.FirstOrDefault();
		}

		/// <summary>
		/// Returns all game objects that intersect with the given plane.
		/// </summary>
		/// <param name="area">The plane to be checked.</param>
		/// <returns>Enumeration of intersecting objects</returns>
		public static List<Item> GetItems(Rectangle area) =>
			_items.Values
				.Where(item => item.Area != null && item.Area.Value.Intersects(area))
				.ToList();

		/// <summary>
		/// Enumerates all simple game objects of the specified type
		/// </summary>
		/// <param name="type">Type of requested game objects</param>
		/// <returns>Enumeration of game objects</returns>
		public static IEnumerable<Item> GetItemsByType(string type) => _items.Values.Where(o => !string.IsNullOrEmpty(o.Type) && o.Type.ToLower(CultureInfo.CurrentCulture) == type.ToLower(CultureInfo.CurrentCulture));

		/// <summary>
		/// Returns a passage the tile intersects.
		/// </summary>
		/// <param name="point">The point to be checked</param>
		/// <returns>The passage if there's any</returns>
		public static Passage GetPassage(Vector2 point)
		{
			Zone zone = GetZone(point);
			return zone?.Exits.FirstOrDefault(p => p.Area.Value.Contains(point));
		}

		/// <summary>
		/// Searches for a passage by name.
		/// </summary>
		/// <param name="name">Inner name of the required passage</param>
		/// <returns>The found passage or null if nothing was found</returns>
		public static Passage GetPassage(string name)
		{
			_passages.TryGetValue(name, out Passage passage);
			return passage;
		}

		/// <summary>
		/// Returns all passages intersecting with the plane.
		/// </summary>
		public static IEnumerable<Passage> GetPassages(Rectangle area) => _passages.Values.Where(p => p.Area.Value.Intersects(area));

		/// <summary>
		/// Prepares the game world.
		/// </summary>
		public static void Init()
		{
			var comparer = StringComparer.OrdinalIgnoreCase;
			_items = new(comparer);
			_zones = new(comparer);
			_characters = new(comparer);
			_passages = new(comparer);
			Map = null;
			InitPlayerCameraController();
		}

		public static CollisionDetector Collisions { get; private set; } = new();

		public static PlacementFinder Placements { get; private set; } = new();

		/// <summary>
		/// Indicates if a tile on the specified position is occupied by an NPC or game object.
		/// </summary>
		/// <param name="point"></param>
		/// <returns>True if there's an object or NPC on the specified position</returns>
		public static bool IsOccupied(Vector2 point) => GetItem(point) != null || GetCharacter(point) != null;

		/// <summary>
		/// Checks if the tile is walkable for NPCs.
		/// </summary>
		public static bool IsWalkable(Vector2 point)
		{
			Tile t = Map[point];

			Passage p = GetPassage(point);
			return
				t is { Walkable: true } && !IsOccupied(point)
										&& (p == null || p is { State: PassageState.Open });
		}

		public static void ApplySave(GameSave game, TileMap map)
		{
			if (game == null)
				throw new ArgumentException(nameof(game));

			Init();
			Map = map;

			// Restore zones
			foreach (ZoneSave save in game.Zones)
			{
				Zone zone = ZoneFactory.Create(true, save.Name.ToName());
				zone.Restore(save);
				Add(zone);
			}

			// Restore characters
			if (game.Characters.IsNullOrEmpty())
				throw new InvalidOperationException("No characters");

			foreach (CharacterSave save in game.Characters)
			{
				Vector2 position = save.Area.ToRectangle().Value.Center;
				Vector2 orientation = save.Orientation.ToVector2();
				Vector3 dimensions = save.Dimensions.ToVector3();
				Character character = CharacterFactory.Create(
					save.Type,
					position,
					orientation,
					dimensions);
				character.Restore(save);
				Add(character);
			}

			Player = GetCharacter("chipotle")
				?? throw new InvalidOperationException("Player nto found");

			// Restore items
			foreach (ItemSave save in game.Items)
			{
				Item item = ItemFactory.Create(
					true,
					save.Name.ToName(),
					default,
					save.Type
					);
				item.Restore(save);
				Add(item);
			}

			// Restore passages
			foreach (PassageSave save in game.Passages)
			{
				Passage passage = PassageFactory.Create(
					true,
					save.Name.ToName(),
					default,
					save.Zones.ToList()
					);
				passage.Restore(save);
				Add(passage);
			}

			// Restore camera
			CameraManager.SetOrientation(game.CameraOrientation.ToVector3());
			// Activate world
			ActivateWorld();
			GameManager.StartGame();
			Reloaded message = new();
			MessageCharacters(message);
			MessageZones(message);
			MessageItems(message);
			MessagePassages(message);
		}

		private static void MessagePassages(Message message)
		{
			foreach (Passage p in _passages.Values)
				p.TakeMessage(message);
		}

		private static void MessageItems(Message message)
		{
			foreach (Item i in _items.Values)
				i.TakeMessage(message);
		}

		/// <summary>
		/// Static constructor
		/// </summary>
		static World()
		{
			RuntimeTypeModel.Default.Add(typeof(Vector2), false).Add("x", "y");
			RuntimeTypeModel.Default.Add(typeof(Vector3), false).Add("x", "y", "z");
		}

		private static string GetAttribute(XElement element, string attribute, bool prepareForIndexing = true) => prepareForIndexing ? element.Attribute(attribute)?.Value.Sanitize() : element?.Attribute(attribute)?.Value;

		private static Dictionary<string, GameObject> _zoneObjects;
		private static Dictionary<string, GameObject> _itemObjects;
		private static Dictionary<string, GameObject> _passageObjects;


		/// <summary>
		/// Loads the map from file.
		/// </summary>
		public static void CreateGame()
		{
			XElement root = GamePersistence.OpenMap().Root;
			Init();
			List<XElement> zoneNodes = root.Element("localities").Elements("locality").ToList();

			Map = TileMap.Create(zoneNodes);
			LoadItemDescriptions(root);
			LoadZonesAndItems(zoneNodes);
			LoadPassages(root);
		}

		private static void LoadPassages(XElement root)
		{
			List<XElement> xPassages = root.Element("passages").Elements("passage").ToList();
			foreach (XElement passageNode in xPassages)
			{
				Passage passage = PassageFactory.Create(passageNode);
				Add(passage);
			}
		}

		private static void LoadZonesAndItems(List<XElement> zoneNodes)
		{
			foreach (XElement zoneNode in zoneNodes)
			{
				Zone zone = ZoneFactory.Create(zoneNode);
				Add(zone);
				LoadItems(zoneNode);
			}
		}

		private static void LoadItems(XElement zoneNode)
		{
			List<XElement> items = zoneNode.Elements("object").ToList();
			foreach (XElement itemNode in items)
			{
				Item item = ItemFactory.Create(itemNode, zoneNode);
				Add(item);
			}
		}

		private static void LoadItemDescriptions(XElement root)
		{
			IEnumerable<XElement> descriptionsNode = root.Element("objectdescriptions").Elements("object");
			foreach (XElement element in descriptionsNode)
			{
				string descriptions = GetAttribute(element, "descriptions", false);
				_objectDescriptions[GetAttribute(element, "type")] = descriptions.Split(new char[] { '|' });
			}
		}

		private const float _gameQuittingFadingDuration = 1;

		/// <summary>
		/// Terminates the application.
		/// </summary>
		public static void QuitGame()
		{
			GameManager.PauseGame();
			Cutscene.Init();
			Action onDone = () => WindowHandler.MainMenu();
			Sounds.StopAllSounds(_gameQuittingFadingDuration, onDone);
			GamePersistence.SaveGame();
		}

		/// <summary>
		/// Sends a message to all NPCs.
		/// </summary>
		/// <param name="message">The message to be sent</param>
		public static void TakeMessage(Message message)
		{
			if (!WorldActive)
				return;

			foreach (Character c in _characters.Values)
				c.TakeMessage(message);
			foreach (Passage p in _passages.Values)
			{
				if (p is Door)
					p.TakeMessage(message);
			}

			foreach (Zone z in _zones.Values)
				z.TakeMessage(message);
		}

		/// <summary>
		/// Unregisters the specified zone.
		/// </summary>
		/// <param name="l">The zone to be removed</param>
		public static void Remove(Zone l) => _delayedActions.Enqueue(() => _zones.Remove(l.Name.Inner));

		/// <summary>
		/// Unregisters the specified passage.
		/// </summary>
		/// <param name="p">The passage to be removed</param>
		public static void Remove(Passage p) => _delayedActions.Enqueue(() => _passages.Remove(p.Name.Inner));

		/// <summary>
		/// Unregisters the specified object.
		/// </summary>
		/// <param name="o">The object to be removed</param>
		public static void Remove(Entity o) => _delayedActions.Enqueue(() => _items.Remove(o.Name.Inner));

		/// <summary>
		/// Starts game from the begining.
		/// </summary>
		public static void StartGame()
		{
			try
			{
				CreateGame();
			}
			catch (Exception)
			{
				throw;
			}
			MainScript.GameLoaded = true;
			CameraManager.InitYaw();

			CreateCharacters();
			ActivateWorld();

			GameManager.StartGame();
			Cutscene.Init();
			Cutscene.Play(null, "cs6");
		}

		private static void ActivateWorld()
		{
			_playerCameraController.Activate();

			foreach (Zone zone in _zones.Values)
				zone.Activate();

			foreach (Passage passage in _passages.Values)
				passage.Activate();

			foreach (Character character in _characters.Values)
				character.Activate();

			foreach (Item item in _items.Values)
				item.Activate();

			WorldActive = true;
		}

		public static bool WorldActive { get; private set; }
		private static void CreateCharacters()
		{
			Vector3 dimensions = new(.4f, 2, .4f);
			Vector2 orientation = new(0, 1);


			// Chipotle
			Vector2 chipotlePosition = Settings.TestChipotleStartPosition ?? new(1032, 1034);
			Player = CharacterFactory.Create(
				"chipotle",
				chipotlePosition,
				orientation,
				dimensions);
			Add(Player);

			// Carson
			Vector2 carsonPosition = new(1225, 1019.4f);
			Add(CharacterFactory.Create(
				"carson",
				carsonPosition,
				orientation,
				dimensions));

			// Christine
			Vector2 christinePosition = new(1775.8f, 1114.7f);
			Add(CharacterFactory.Create(
				"christine",
				christinePosition,
				orientation,
				dimensions));

			// Mariotti
			Vector2 mariottiPosition = new(2013.3f, 1129.1f);
			Add(CharacterFactory.Create(
				"mariotti",
				mariottiPosition,
				orientation,
				dimensions));

			// Sweeney
			Vector2 sweeneyPosition = new(1402.3f, 955.7f);
			Add(CharacterFactory.Create(
				"sweeney",
				sweeneyPosition,
				orientation,
				dimensions));

			// Tuttle
			Vector2 tuttlePosition;
			if (Settings.AllowTuttlesCustomPosition && Settings.TuttleTestStart.HasValue)
				tuttlePosition = Settings.TuttleTestStart.Value;
			else tuttlePosition = new(1031.8f, 1035.5f);
			Add(CharacterFactory.Create(
				"tuttle",
				tuttlePosition,
				orientation,
				dimensions));

			// Bartender
			Vector2 bartenderPosition = new(1556.9f, 1073.2f);
			Add(CharacterFactory.Create(
				"bartender",
				bartenderPosition,
				orientation,
				dimensions));
		}

		/// <summary>
		/// Updates whole game world.
		/// </summary>
		public static void UpdateGame()
		{
			if (GameManager.State != GameState.Playing)
				return;

			PerformDelayedActions();
			foreach (Zone zone in _zones.Values)
				zone.GameUpdate();

			foreach (Passage passage in _passages.Values)
				passage.GameUpdate();

			foreach (Item item in _items.Values)
				item.GameUpdate();

			foreach (Character character in _characters.Values)
				character.GameUpdate();

			_playerCameraController.GameUpdate();

			WindowHandler.ActiveWindow.GameUpdate();
		}

		/// <summary>
		/// Performs all planned actions.
		/// </summary>
		private static void PerformDelayedActions()
		{
			while (!_delayedActions.IsNullOrEmpty())
				_delayedActions.Dequeue()();
		}

		public static void MessageCharacters(Message message)
		{
			IEnumerable<object> characters = _characters.Values.Except(new[] { message.Sender });
			foreach (Character character in characters)
				character.TakeMessage(message);
		}

		public static void MessageZones(Message message)
		{
			foreach (Zone zone in _zones.Values)
				zone.TakeMessage(message);
		}

		public static void MessagePlayerCameraController(Message message)
			=> _playerCameraController.TakeMessage(message);

		private static void InitPlayerCameraController()
		{
			GameObject obj = new(nameof(PlayerCameraController));
			_playerCameraController = obj.AddComponent<PlayerCameraController>();
		}
		private static PlayerCameraController _playerCameraController;
	}
}