using Assets.Scripts;
using Assets.Scripts.Models;

using DavyKager;

using Game.Audio;
using Game.Entities;
using Game.Entities.Characters;
using Game.Entities.Items;
using Game.Messaging.Events.GameManagement;
using Game.Messaging.Events.Sound;
using Game.Models;
using Game.Terrain;
using Game.UI;

using ProtoBuf.Meta;

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using UnityEngine;
using UnityEngine.SceneManagement;

using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

using Message = Game.Messaging.Message;
using Random = System.Random;
using Rectangle = Game.Terrain.Rectangle;

namespace Game
{
	/// <summary>
	/// Represents the game world.
	/// </summary>
	public static class World
	{

		/// <summary>
		/// Indicates if the game is in progress.
		/// </summary>
		public static bool GameInProgress;
		private static CutsceneBegan _cutSceneMessage;

		/// <summary>
		/// Check if two map elements are within a specified radius of each other.
		/// </summary>
		/// <returns>True if the two elements are within the specified radius of each other</returns></returns>
		public static bool IsInRange(MapElement a, MapElement b, float radius)
		{
			return GetDistance(a, b) <= radius;
		}

		/// <summary>
		/// Determines if element 'a' is closer than element 'b'.
		/// </summary>
		/// <param name="referenceElement">The element to which the distance should be calculated</param>
		/// <param name="a">First map element to compare</param>
		/// <param name="b">Second map element to compare</param>
		/// <returns>True if element a is closer than element b</returns>
		public static bool IsCloser(MapElement referenceElement, MapElement a, MapElement b)
		{
			return GetDistance(referenceElement, a) <= GetDistance(referenceElement, b);
		}

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

		public const float CollisionDetectionResolution = .1f;

		/// <summary>
		/// Detects collisions on the given track area in the specified direction for the given length.
		/// </summary>
		/// <param name="area">The rectangle representing the track area.</param>
		/// <param name="direction">The direction in which to detect collisions.</param>
		/// <param name="length">The length for which to detect collisions in meters</param>
		/// <returns>A list of MapElements representing the obstacles detected on the track or null</returns>
		/// <remarks>Divides the track to little segments and in every position checks all objects, closed passages and characters in intersecting localities for collision. The search ends at the position where collisions were detected.</remarks>
		public static CollisionsModel DetectCollisionsOnTrack(MapElement ignoredElement, Vector2 direction, float length)
		{
			int steps = (int)Math.Ceiling(length / CollisionDetectionResolution);
			Rectangle area = new(ignoredElement.Area.Value);

			for (int i = 0; i < steps; i++)
			{
				area.Move(direction, CollisionDetectionResolution);
				CollisionsModel result = DetectCollisions(ignoredElement, area);
				if (result.OutOfMap)
					return result;
				if (!result.Obstacles.IsNullOrEmpty())
					return result;
			}
			return new(null, false);
		}

		/// <summary>
		/// Detects collisions between the specified map element and other map elements.
		/// </summary>
		/// <param name="element">The element to be moved.</param>
		/// <param name="area">The map element to detect collisions for.</param>
		/// <returns>List of MapElements or null</returns>
		public static CollisionsModel DetectCollisions(MapElement element, Rectangle area)
		{
			List<object> obstacles = new();
			List<Locality> localities = area.GetLocalities().ToList();

			// Detect inaccesible terrain.
			List<TileInfo> enumerable = area.GetTiles();
			List<TileInfo> inaccessibleTiles = enumerable
				.Where(t => !t.Tile.Walkable)
				.Distinct().ToList();

			if (inaccessibleTiles.Any())
				obstacles.AddRange(inaccessibleTiles.Cast<object>());

			// Check all objects, passages and characters in localities the given element intersects. Skip the given element.
			foreach (Locality locality in localities)
			{
				void DetectCollision(IEnumerable<MapElement> elements)
				{
					IEnumerable<MapElement> newObstacles = elements
						.Where(e => e != element && e.Area != null)
						.Where(e => e.Area.Value.Intersects(area) || e.Area.Value.Contains(area) || area.Contains(e.Area.Value));

					if (newObstacles.Any())
						obstacles.AddRange(newObstacles);
				}

				DetectCollision(locality.Objects);
				DetectCollision(locality.Characters);
				DetectCollision(locality.Passages.Where(p => p is Door d && (d.State == PassageState.Closed || d.State == PassageState.Locked)));
			}

			bool outOfMap = area.IsOutOfMap();

			if (!obstacles.Any())
				obstacles = null;
			return new(obstacles, outOfMap);
		}

		/// <summary>
		/// Returns a text description for the specified object.
		/// </summary>
		/// <param name="object">A character or object</param>
		/// <param name="id">Numeric identifier of the requested caption</param>
		/// <returns>A string containing the requested description</returns>
		public static string GetObjectDescription(Entity @object, int id)
		{
			return _objectDescriptions.ContainsKey(@object.Type) ? _objectDescriptions[@object.Type][id] : "popis chybí";
		}

		/// <summary>
		/// Contains descriptions for object templates.
		/// </summary>
		private static Dictionary<string, string[]> _objectDescriptions = new();

		/// <summary>
		/// Enumerates all localities in an area specified by the area code.
		/// </summary>
		/// <param name="areaCode">Part of locality indexed name that specifies the containing area</param>
		/// <returns>enumeration of localities</returns>
		public static IEnumerable<Locality> GetLocalitiesInArea(string areaCode)
		{
			return
				from l in GetLocalities()
				let name = l.Name.Indexed
				let position = name.Length - 2
				where name.Substring(position, 1).ToLower() == "h"
				select l;
		}

		private static PathFinder _pathFinder = new();

		/// <summary>
		/// Constructs the shortest possible path between two points.
		/// </summary>
		/// <param name="start">The initial point fo path finding.</param>
		/// <param name="goal">The goal of the path finding</param>
		/// <param name="throughObjects">Specifies if tiles with objects should be included.</param>
		/// <param name="throughClosedDoors"Specifies if tiles on closed doors should be included.></param>
		/// <param name="throughImpermeableTerrain">Specifies if tiles with impermeable terrain should be included.</param>
		/// <param name="sameLocality">Specifies if different localities than locality of the initial point should be included</param>
		/// <param name="throughStart">Specifies if the start point should be considered walkable.</param>
		/// <param name="throughGoal">Specifies if the goal should be considered walkable</param>
		/// <param name="maxDistance">Maximum allowed distance from the initial point</param>
		/// <returns>
		/// A list of points leading from start to the end or null if no possible path exists
		/// </returns>
		public static Queue<Vector2> FindPath(Vector2 start, Vector2 goal, bool throughObjects = false, bool throughClosedDoors = true, bool throughImpermeableTerrain = false, bool sameLocality = false, bool throughStart = false, bool throughGoal = false, int maxDistance = 300)
		{
			return _pathFinder.FindPath(start, goal, throughObjects, throughClosedDoors, throughImpermeableTerrain, sameLocality, throughStart, throughGoal, maxDistance);
		}

		/// <summary>
		/// Searches nearest surrounding of the specified map element and selects a random walkable tile.
		/// </summary>
		/// <param name="element">Specifies the map element around which to search</param>
		/// <param name="minDistance">Specifies minimum distance oif the points from the surroundings.</param>
		/// <param name="maxDistance">Specifies maximum distance fo the points in the surroundings.</param>
		/// <returns>Coordinates of the found walkable tile or null if nothing was found</returns>
		public static Vector2? GetRandomWalkablePoint(MapElement element, int minDistance, int maxDistance)
		{
			return GetRandomWalkablePoint(element.Area.Value, minDistance, maxDistance);
		}

		/// <summary>
		/// Searches nearest surrounding of the specified point and selects a random walkable tile.
		/// </summary>
		/// <param name="point">Specifies the point around which to search</param>
		/// <param name="minDistance">Specifies minimum distance oif the points from the surroundings.</param>
		/// <param name="maxDistance">Specifies maximum distance fo the points in the surroundings.</param>
		/// <returns>Coordinates of the found walkable tile or null if nothing was found</returns>
		public static Vector2? GetRandomWalkablePoint(Vector2 point, int minDistance, int maxDistance)
		{
			return GetRandomWalkablePoint(new Rectangle(point), minDistance, maxDistance);
		}

		/// <summary>
		/// Searches nearest surrounding of plane and selects a random walkable tile.
		/// </summary>
		/// <param name="maxDistance">Specifies width of the searched perimeter around the plane</param>
		/// <returns>Coordinates of the found walkable tile or null if nothing was found</returns>
		public static Vector2? GetRandomWalkablePoint(Rectangle area, int minDistance, int maxDistance)
		{
			IEnumerable<Vector2> walkables =
				area.GetWalkableSurroundingPoints(minDistance, maxDistance);

			if (walkables.IsNullOrEmpty())
				return null;

			// Select a random point.
			int index = (new Random())
				.Next(walkables.Count());

			return (Vector2?)walkables.ElementAt(index);
		}

		/// <summary>
		/// Finds the nearest walkable tile in surroundings of this plane.
		/// </summary>
		/// <param name="element">The map element around which to search.</param>
		/// <param name="minDistance">Minimum distance from center</param>
		/// <param name="maxDistance">Maximum distance from center</param>
		/// <returns>Coordinates of the found walkable tile or null if nothing was found</returns>
		public static Vector2? GetNearestWalkableTile(MapElement element, int minDistance, int maxDistance)
		{
			return GetNearestWalkableTile(element.Area.Value, minDistance, maxDistance);
		}

		/// <summary>
		/// Finds the nearest walkable tile in surroundings of this plane.
		/// </summary>
		/// <param name="point">The point around which to search.</param>
		/// <param name="minDistance">Minimum distance from center</param>
		/// <param name="maxDistance">Maximum distance from center</param>
		/// <returns>Coordinates of the found walkable tile or null if nothing was found</returns>
		public static Vector2? GetNearestWalkableTile(Vector2 point, int minDistance, int maxDistance)
		{
			return GetNearestWalkableTile(new Rectangle(point), minDistance, maxDistance);
		}

		/// <summary>
		/// Finds the nearest walkable tile in surroundings of this plane.
		/// </summary>
		/// <param name="area">The plane around which to search.</param>
		/// <param name="minDistance">Minimum distance from center</param>
		/// <param name="maxDistance">Maximum distance from center</param>
		/// <returns>Coordinates of the found walkable tile or null if nothing was found</returns>
		public static Vector2? GetNearestWalkableTile(Rectangle area, int minDistance, int maxDistance)
		{
			return
				(from p in area.GetWalkableSurroundingPoints(minDistance, maxDistance)
				 let distance = GetDistance(area.GetClosestPoint(p), p)
				 orderby distance
				 select p)
				.FirstOrDefault();
		}

		private static readonly float _cutsceneVolume = 1;

		/// <summary>
		/// Replays the last cutscene
		/// </summary>
		public static void RepeatCutscene()
		{

		}

		/// <summary>
		/// Resumes a paused cutscene.
		/// </summary>
		public static void ResumeCutscene()
		{
			_cutScenePlayer.Revind(2);
			_cutScenePlayer.Resume();
			_cutScenePlayer.FadeIn(.5f, Sounds.DefaultMasterVolume);
		}

		/// <summary>
		/// Pauses an ongoing cutscene.
		/// </summary>
		public static void PauseCutscene()
		{
			_cutScenePlayer.Pause();
		}

		/// <summary>
		/// Detects acoustic obstacles between the player and the specified map element.
		/// </summary>
		/// <param name="area">The map element to be checked</param>
		/// <returns>The corresponding obstacle type</returns>
		public static ObstacleType DetectAcousticObstacles(Rectangle area)
		{
			// If it's an object that is held by an 

			Locality playersLocality = Player.Locality;
			Vector2 closest = area.GetClosestPoint(World.Player.Area.Value.Center);
			Locality otherLocality = World.GetLocality(closest);
			bool neighbour = playersLocality.IsNeighbour(otherLocality);
			bool accessible = otherLocality.IsAccessible(playersLocality);

			// Are the regions in inadjecting localities?
			if (GetDistance(area.Center, Player.Area.Value.Center) > 100
				|| (playersLocality != otherLocality && (!neighbour || (neighbour && !accessible))))
				return ObstacleType.Far;  // Inaudible

			// Adjecting localities
			Rectangle path = new(area.GetClosestPoint(Player.Area.Value.Center), Player.Area.Value.Center);
			ObstacleType obstacle = DetectObstacles(path);

			if (neighbour && accessible)
			{
				Passage atPassage = Player.Locality.IsAtPassage(area.Center);

				if (obstacle == ObstacleType.IndirectPath && atPassage == null)
					return ObstacleType.Wall;

				if (atPassage != null)
				{
					return atPassage.State == PassageState.Closed ? ObstacleType.Door : ObstacleType.None;
				}
			}

			return obstacle != ObstacleType.Wall ? obstacle : ObstacleType.Object;
		}

		/// <summary>
		/// Checks if there are some obstacles between the specified points. The edge points arn't included.
		/// </summary>
		/// <param name="p1">First point</param>
		/// <param name="p2">Second point</param>
		/// <returns>ObstacleType</returns>
		public static ObstacleType DetectObstacles(Rectangle path)
		{
			bool Intersects(Rectangle area)
				=> area.Intersects(path.UpperLeftCorner) || area.Intersects(path.LowerRightCorner);

			// Is it a line?
			if (!path.IsLine)
				return ObstacleType.IndirectPath;

			// Detect doors.
			if (
				path.GetPassages()
				.Any(p => !Intersects(p.Area.Value) && p.State == PassageState.Closed))
				return ObstacleType.Door;

			// Detect walls
			if (
				path.GetTiles()
					.Where(t => !Intersects(new(t.Position)))
					.Any(t => t.Tile.Terrain == TerrainType.Wall)
				|| path.GetObjects().Any(o => !Intersects(o.Area.Value) && (o.Type == "zeď" || o.Name.Friendly == "zeď"))
			)
				return ObstacleType.Wall;

			// Detect objects
			return path.GetObjects().Any(o => !Intersects(o.Area.Value)) ? ObstacleType.Object : ObstacleType.None;
		}

		/// <summary>
		/// Enumerates all localities.
		/// </summary>
		/// <returns>Enumeration of all localities</returns>
		public static IEnumerable<Locality> GetLocalities()
		{
			return _localities.Values.AsEnumerable();
		}

		/// <summary>
		/// Enumerates all localities intersecting with the speciifed area.
		/// </summary>
		/// <param name="area">The area the localities should intersect with</param>
		/// <returns>enumeration of localities</returns>
		public static IEnumerable<Locality> GetLocalities(Game.Terrain.Rectangle area)
		{
			return
				from l in GetLocalities()
				where l.Area.Value.Intersects(area)
				select l;
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
		/// Map of localities and corresponding background sounds
		/// </summary>
		private static readonly Dictionary<string, (string sound, float volume)> _localityLoops = new()
		{
			["chodba h1"] = ("ElectricalBoxLoop", 1),
			["balkon p1"] = ("BelvedereStreetLoop", 1),
			["terasa w1"] = ("PoolLoop", .4f),
			["výčep h1"] = ("CzechPubLoop", .7f),
			["ulice h1"] = ("BonitaStreetLoop", .5f),
			["příjezdová cesta w1"] = ("DriveWayLoop", 1),
			["bazén w1"] = ("PoolLoop", 1),
			["zahrada c1"] = ("CarsonsGardenLoop", .1f),
			["asfaltka c1"] = ("AsphaltRoadLoop", .5f),
			["cesta c1"] = ("AsphaltRoadLoop", .5f),
			["ulice p1"] = ("BelvedereStreetLoop", 1.5f),
			["ulice v1"] = ("GordonStreetLoop", 1),
			["garáž v1"] = ("GarageLoop", 1),
			["garáž s1"] = ("GarageLoop", 1),
			["garáž w1"] = ("GarageLoop", 1),
			["garáž p1"] = ("GarageLoop", 1),
			["ulice s1"] = ("BonitaStreetLoop", .7f),
			["dvorek s1"] = ("DriveWayLoop", 1),
		};

		/// <summary>
		/// List of all NPCs
		/// </summary>
		private static Dictionary<string, Character> _characters;

		/// <summary>
		/// List of all localities
		/// </summary>
		private static Dictionary<string, Locality> _localities;

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
		/// <param name="e">The entity to be registered</param>
		public static void Add(Character e)
		{
			// Do a null check and look if entity isn't already registered.
			if (e == null)
				throw new ArgumentNullException(nameof(e));

			if (_characters.ContainsKey(e.Name.Indexed))
				throw new ArgumentException("entity already registered");

			_characters.Add(e.Name.Indexed, e);
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

			if (_items.ContainsKey(o.Name.Indexed))
				throw new ArgumentException("Object already registered");

			_items.Add(o.Name.Indexed, o); // Added to dictionary
		}

		/// <summary>
		/// Registers a locality.
		/// </summary>
		/// <param name="l">The locality to be registered</param>
		public static void Add(Locality l)
		{
			// null check
			if (l == null)
				throw new ArgumentNullException(nameof(l));

			// Isn't the locality already registered?
			if (_localities.ContainsKey(l.Name.Indexed))
				throw new ArgumentException("Locality already registered");

			_localities.Add(l.Name.Indexed, l);
		}

		/// <summary>
		/// Registers a passage.
		/// </summary>
		/// <param name="p">The passage to be registered</param>
		public static void Add(Passage p)
		{
			if (p == null)
				throw new ArgumentNullException(nameof(p));

			if (_passages.ContainsKey(p.Name.Indexed))
				throw new ArgumentException("Passage already registered");

			_passages.Add(p.Name.Indexed, p);
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
		public static float GetDistance(MapElement element1, MapElement element2)
		{
			Vector2 point1 = element1.Area.Value.GetClosestPoint(element2.Area.Value.Center);
			Vector2 point2 = element2.Area.Value.GetClosestPoint(point1);

			// Verify that elements do not intersect.
			return element1.Area.Value.Intersects(element2.Area.Value) ? 0 : GetDistance(point1, point2);
		}

		/// <summary>
		/// Computes cartesian distance between two points.
		/// </summary>
		/// <param name="a">First point</param>
		/// <param name="b">Second point</param>
		/// <returns>Rounded distance between two given points</returns>
		public static float GetDistance(Vector2 a, Vector2 b)
		{
			return (float)(Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y));
		}

		/// <summary>
		/// Computes cartesian distance between two 3d points.
		/// </summary>
		/// <param name="a">First point</param>
		/// <param name="b">Second point</param>
		/// <returns>Distance between two given points</returns>
		public static float GetDistance(Vector3 a, Vector3 b)
		{
			return (float)(Math.Pow(Math.Abs(a.x - b.x), 2) + Math.Pow(Math.Abs(a.y - b.y), 2) + Math.Pow(Math.Abs(a.z - b.z), 2)) * 0.5f;
		}

		/// <summary>
		/// Returns an entity that stands on the given position.
		/// </summary>
		/// <param name="point"></param>
		/// <returns>Reference to the entity if there's any</returns>
		public static Character GetCharacter(Vector2 point)
		{
			Locality locality = GetLocality(point);
			return locality == null
				? null
				: (
					from e in locality.Characters
					where e.Area != null && e.Area.Value.Intersects(point)
					select e)
				.FirstOrDefault();
		}

		/// <summary>
		/// Enumerates all entities that intersect with the given plane.
		/// </summary>
		/// <param name="area">The plane to be checked.</param>
		/// <returns>Enumeration of intersecting entities</returns>
		public static IEnumerable<Character> GetEntities(Rectangle area)
		{
			return _characters.Values.Where(o => o.Area != null && o.Area.Value.Intersects(area));
		}

		/// <summary>
		/// Returns an NPC found by its name.
		/// </summary>
		/// <param name="name">Inner name of the required NPC</param>
		/// <returns>The found NPC or null if nothing was found</returns>
		public static Character GetCharacter(string name)
		{
			return _characters != null && _characters.TryGetValue(name.ToLower(), out Character e) ? e : null;
		}

		/// <summary>
		/// Returns a locality found by its name.
		/// </summary>
		/// <param name="name">Inner name of the required locality</param>
		/// <returns>The found locality or null if nothing was found</returns>
		public static Locality GetLocality(string name)
		{
			_localities.TryGetValue(name.PrepareForIndexing(), out Locality locality);
			return locality;
		}

		/// <summary>
		/// Returns a locality that fully intersects with the given plane.
		/// </summary>
		/// <param name="area">The point tto be checked</param>
		/// <returns>The intersecting locality</returns>
		public static Locality GetLocality(Rectangle area)
		{
			return _localities.Values.FirstOrDefault(l => l.Area.Value.Contains(area));
		}

		/// <summary>
		/// Returns a locality which intersects with the given point.
		/// </summary>
		/// <param name="point">The point tto be checked</param>
		/// <returns>The intersecting locality</returns>
		public static Locality GetLocality(Vector2 point)
		{
			return _localities.Values
						.FirstOrDefault(l => l.Area.Value.Intersects(point));
		}

		/// <summary>
		/// Enumerates all localities sorted by distance from the specified point.
		/// </summary>
		/// <param name="point">The point whose surroundings should be explored</param>
		/// <returns>Enumeration of the found localities</returns>
		public static IEnumerable<Locality> GetNearestLocalities(Vector2 point)
		{
			return
				(from l in _localities.Values
				 where l != GetLocality(point)
				 orderby l.Area.Value.GetDistanceFrom(point)
				 select l)
				.Distinct();
		}

		/// <summary>
		/// Returns the locality nearest from the specified point.
		/// </summary>
		/// <param name="point">The point shose surroundings is to be searched</param>
		/// <returns>The found locality</returns>
		public static Locality GetNearestLocality(Vector2 point)
		{
			return GetNearestLocalities(point).First();
		}

		/// <summary>
		/// Returns a game object closest to the specified point.
		/// </summary>
		/// <param name="point">The point whose surroundings is to be searched</param>
		/// <returns>The found game object</returns>
		public static Entity GetNearestObject(Vector2 point)
		{
			return GetNearestObjects(point).FirstOrDefault();
		}

		/// <summary>
		/// Enumerates all game objects around a point sorted by distance.
		/// </summary>
		/// <param name="point">A point whose surroundings are to be searched</param>
		/// <returns>Enumeration of game objects</returns>
		public static IEnumerable<Item> GetNearestObjects(Vector2 point)
		{
			return
				(from o in _items.Values
				 where o.Area != null && !o.Area.Value.Intersects(point)
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
				 where o.Area != null && o.Decorative == includeDecoaration && !o.Area.Value.Intersects(point) && distance <= radius
				 select o)
				.Distinct();
		}

		/// <summary>
		/// Returns the passage closest to the specified point.
		/// </summary>
		/// <param name="point">The point whose surrounding is to be searched</param>
		/// <returns>The found passage</returns>
		public static Passage GetNearestPassage(Vector2 point)
		{
			return GetNearestPassages(point).FirstOrDefault();
		}

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
				 where !p.Area.Value.Intersects(point)
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
		public static IEnumerable<Door> GetNearestDoors(Vector2 point, float maxDistance)
		{
			return
				from passage in GetNearestPassages(point)
				where passage is Door
				let door = passage as Door
				let distance = door.Area.Value.GetDistanceFrom(point)
				where distance <= maxDistance
				select door;
		}
		/// <summary>
		/// searches for a simple game object by name.
		/// </summary>
		/// <param name="name">Inner name of the required object</param>
		/// <returns>The found game object or null if nothing was found</returns>
		public static Item GetItem(string name)
		{
			return _items.TryGetValue(name, out Item o) ? o : null;
		}

		/// <summary>
		/// Returns an NPC or game object the tile intersects.
		/// </summary>
		public static Item GetItem(Vector2 point)
		{
			Locality locality = GetLocality(point);
			return locality == null
				? null
				: (from o in locality.Objects
				   where o.Area != null && o.Area.Value.Intersects(point)
				   select o)
				.FirstOrDefault();
		}

		/// <summary>
		/// Returns all game objects that intersect with the given plane.
		/// </summary>
		/// <param name="area">The plane to be checked.</param>
		/// <returns>Enumeration of intersecting objects</returns>
		public static IEnumerable<Item> GetItems(Rectangle area)
		{
			return
				from o in _items.Values
				where o.Area != null && o.Area.Value.Intersects(area)
				select o;
		}

		/// <summary>
		/// Enumerates all simple game objects of the specified type
		/// </summary>
		/// <param name="type">Type of requested game objects</param>
		/// <returns>Enumeration of game objects</returns>
		public static IEnumerable<Item> GetItemsByType(string type)
		{
			return _items.Values.Where(o => !string.IsNullOrEmpty(o.Type) && o.Type.ToLower(CultureInfo.CurrentCulture) == type.ToLower(CultureInfo.CurrentCulture));
		}

		/// <summary>
		/// Returns a passage the tile intersects.
		/// </summary>
		/// <param name="point">The point to be checked</param>
		/// <returns>The passage if there's any</returns>
		public static Passage GetPassage(Vector2 point)
		{
			Locality locality = GetLocality(point);
			return locality?.Passages.FirstOrDefault(p => p.Area.Value.Intersects(point));
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
		public static IEnumerable<Passage> GetPassages(Rectangle area)
		{
			return _passages.Values.Where(p => p.Area.Value.Intersects(area));
		}

		/// <summary>
		/// Prepares the game world.
		/// </summary>
		public static void Initialize()
		{
			_items = new();
			_localities = new();
			_characters = new();
			_passages = new();
			Sounds.Initialize();
			GameObject obj = new();
			_cutScenePlayer = obj.AddComponent<CutScenePlayer>();
		}

		/// <summary>
		/// Indicates if a tile on the specified position is occupied by an NPC or game object.
		/// </summary>
		/// <param name="point"></param>
		/// <returns>True if there's an object or NPC on the specified position</returns>
		public static bool IsOccupied(Vector2 point)
		{
			return GetItem(point) != null || GetCharacter(point) != null;
		}

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

		/// <summary>
		/// Creates a predefined game state save. Asks for its name.
		/// </summary>
		/// <returns>True on success</returns>
		public static bool CreatePredefinedSave()
		{
			throw new NotImplementedException();
			string name = null;
			//Interaction.InputBox(String.Empty, "Zadej název sejvu");
			if (string.IsNullOrEmpty(name))
			{
				Tolk.Speak("Tak nic");
				return false;
			}

			if (!Directory.Exists(MainScript.PredefinedSavesPath))
				Directory.CreateDirectory(MainScript.PredefinedSavesPath);
			string path = Path.Combine(MainScript.PredefinedSavesPath, name);
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			path = Path.Combine(path, "game.sav");
			SaveGame(path);
			Tolk.Speak("uloženo");
			return true;
		}

		/// <summary>
		/// Starts the menu for selecting a predefined save and loads the selected save into memory.
		/// </summary>
		/// <returns>True if a save was selected.</returns>
		/// <remarks>for testing purposes only.</remarks>
		public static bool LoadPredefinedSave()
		{
			string[] saves = null;

			if (Directory.Exists(MainScript.PredefinedSavesPath))
			{
				saves =
					Directory.GetDirectories(MainScript.PredefinedSavesPath);
			}

			if (saves.IsNullOrEmpty())
			{
				Tolk.Speak("Žádný sejvy tady nevidim.");
				return false;
			}

			List<List<string>> items =
				saves.Select(s => new List<string> { Path.GetFileName(s) })
					.ToList();

			int i = WindowHandler.Menu(new(items, "Kterej sejv chceš načíst?"));

			if (i == -1)
			{
				Tolk.Speak("Tak nic");
				return false;
			}

			// Stop all sounds
			Sounds.StopAll();

			string path = Path.Combine(saves[i], "game.sav");
			LoadGame(path);
			Tolk.Speak("Načteno.");
			return true;
		}

		/// <summary>
		/// Loads a saved game state from default path.
		/// </summary>
		public static void LoadGame()
		{
			LoadGame(MainScript.SerializationPath);
		}

		/// <summary>
		/// Loads a saved game state from the specified binary file.
		/// </summary>
		/// <param name="path">Path to the saved game state</param>
		/// <remarks>Used just for testing purposes. Allows opening predefined saves.</remarks>
		public static void LoadGame(string path)
		{
			// Pause game if in progress.
			bool resumeGame = GameInProgress;
			GameInProgress = false;

			// Deserialize data from file
			SerializerHelper helper = null;

			try
			{
				if (!File.Exists(path))
					MainScript.Terminate($"Nevidím soubor {MainScript.SerializationPath}. Že ty ses v tom hrabal?");

				// Load terrain, objects, NPCs, localities and passages.
				LoadTerrain(OpenMap().Root);
				FileStream stream = null;

				using (stream = File.OpenRead(path))
				{
					helper = ProtoBuf.Serializer.Deserialize<SerializerHelper>(stream);
				}
				stream?.Close();
			}
			catch (ProtoBuf.ProtoException)
			{
				MainScript.Terminate($"Nepodařilo se načíst hru. Soubor {MainScript.SerializationPath} je v nesprávném formátu.");
			}
			catch (Exception e)
			{
				MainScript.OnError(e);
			}

			_characters = helper.Entities;
			_items = helper.Objects;
			_passages = helper.Passages;
			_localities = helper.Localities;
			WindowHandler.Switch(GameWindow.CreateInstance());
			Reloaded message = new();

			foreach (Character c in _characters.Values)
				c.TakeMessage(message);

			foreach (Locality l in _localities.Values)
				l.TakeMessage(message);

			foreach (Item i in _items.Values)
				i.TakeMessage(message);

			foreach (Passage p in _passages.Values)
				p.TakeMessage(message);

			// Resume the game
			if (resumeGame)
				GameInProgress = true;
		}

		/// <summary>
		/// Static constructor
		/// </summary>
		static World()
		{
			RuntimeTypeModel.Default.Add(typeof(Vector2), false).Add("x", "y");
			RuntimeTypeModel.Default.Add(typeof(Vector3), false).Add("x", "y", "z");
		}

		private static CutScenePlayer _cutScenePlayer;

		private static string GetAttribute(XElement element, string attribute, bool prepareForIndexing = true)
		{
			return prepareForIndexing ? element.Attribute(attribute).Value.PrepareForIndexing() : element.Attribute(attribute).Value;
		}

		private static List<XElement> _localityNodes;

		/// <summary>
		/// Loads the map from file.
		/// </summary>
		public static void LoadMap()
		{
			// gather precreated game objects.
			Dictionary<string, GameObject> localityObjects = new();
			Dictionary<string, GameObject> itemObjects = new();
			Dictionary<string, GameObject> passageObjects = new();
			Scene scene = SceneManager.GetActiveScene();
			GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
			foreach (GameObject obj in allObjects)
			{
				switch (obj.tag)
				{
					case "Locality": localityObjects[obj.name] = obj; break;
					case "Item": itemObjects[obj.name] = obj; break;
					case "Passage": passageObjects[obj.name] = obj; break;
				}
			}

			if (localityObjects.Count == 0)
				throw new InvalidOperationException("No geometry for localities found in the scene.");
			if (itemObjects.Count == 0)
				throw new InvalidOperationException("No geometry for items found in the scene.");
			if (passageObjects.Count == 0)
				throw new InvalidOperationException("No geometry for localities found in the scene.");

			XElement root = OpenMap().Root;
			Initialize();
			_localityNodes = root.Element("localities").Elements("locality").ToList();

			LoadTerrain(root);
			LoadItemDescriptions(root);
			LoadLocalitiesAndItems(root, localityObjects, itemObjects);
			LoadPassages(root, passageObjects);
		}

		private static void LoadPassages(XElement root, Dictionary<string, GameObject> passageObjects)
		{
			PassageFactory.LoadPassages();

			List<XElement> xPassages = root.Element("passages").Elements("passage").ToList();
			foreach (XElement passageNode in xPassages)
			{
				Name name = new(GetAttribute(passageNode, "indexedname"));
				bool isDoor = GetAttribute(passageNode, "door").ToBool();
				bool closed = GetAttribute(passageNode, "closed").ToBool();
				bool openable = GetAttribute(passageNode, "openable").ToBool();

				// Determine the state of the passage.
				PassageState state = PassageState.Open;
				if (!openable)
					state = PassageState.Locked;
				else if (openable && closed)
					state = PassageState.Closed;

				Rectangle area = new(GetAttribute(passageNode, "coordinates"));
				string[] localities = new string[]
				{ GetAttribute(passageNode, "from"),
					GetAttribute(passageNode, "to")
				};
				Door.DoorType dType = GetAttribute(passageNode, "type") == "door" ? Door.DoorType.Door : Door.DoorType.Gate;

				GameObject obj = null;
				if (!passageObjects.TryGetValue(name.Indexed, out obj))
					throw new InvalidOperationException("No geometry found for the passage {name.Indexed}.");

				Passage passage = PassageFactory.CreatePassage(obj, name, area, localities, isDoor, state, dType);
				Add(passage);
			}
		}

		private static Dictionary<string, LocalityMaterialsDefinitionModel> LoadLocalityMaterials()
		{
			IDeserializer deserializer = new DeserializerBuilder()
				.WithNamingConvention(PascalCaseNamingConvention.Instance)
				.Build();

			string yaml = File.ReadAllText(MainScript.MaterialsPath);
			Dictionary<string, LocalityMaterialsDefinitionModel> materials = deserializer.Deserialize<Dictionary<string, LocalityMaterialsDefinitionModel>>(yaml);
			return new Dictionary<string, LocalityMaterialsDefinitionModel>(materials, StringComparer.OrdinalIgnoreCase);
		}

		private static void LoadLocalitiesAndItems(XElement root, Dictionary<string, GameObject> localityObjects, Dictionary<string, GameObject> itemObjects)
		{
			ItemFactory.LoadItems();
			Dictionary<string, LocalityMaterialsDefinitionModel> materials = LoadLocalityMaterials();
			foreach (XElement localityNode in _localityNodes)
			{
				Locality locality = LoadLocality(localityNode, localityObjects, materials);
				LoadItems(localityNode, locality.Area.Value, itemObjects);
			}
		}

		private static Locality LoadLocality(XElement localityNode, Dictionary<string, GameObject> localitiObjects, Dictionary<string, LocalityMaterialsDefinitionModel> materials)
		{
			(string sound, float volume) lBackgroundInfo;
			_localityLoops.TryGetValue(GetAttribute(localityNode, "indexedname"), out lBackgroundInfo);
			Name name = new(GetAttribute(localityNode, "indexedname"), GetAttribute(localityNode, "friendlyname"));
			string description = GetAttribute(localityNode, "description");
			string to = GetAttribute(localityNode, "to");
			Locality.LocalityType type = GetAttribute(localityNode, "type").ToLocalityType();
			float height = int.Parse(GetAttribute(localityNode, "height"));
			Rectangle area = new(GetAttribute(localityNode, "coordinates"));
			TerrainType defaultTerrain = GetAttribute(localityNode, "defaultTerrain", false).ToTerrainType();

			if (type == Locality.LocalityType.Outdoor)
				height = 6;

			GameObject obj = null;
			if (!localitiObjects.TryGetValue(name.Indexed, out obj))
				throw new InvalidOperationException($"No geometry found for the locality {name.Indexed}");

			Locality locality = obj.GetComponent<Locality>();

			locality.Initialize(
				name,
				description,
				to,
				type,
				height,
				area,
				defaultTerrain,
				lBackgroundInfo.sound,
				lBackgroundInfo.volume,
				materials[name.Indexed]
			);
			Add(locality);
			return locality;
		}

		private static void LoadItems(XElement localityNode, Rectangle localityArea, Dictionary<string, GameObject> itemObjects)
		{
			List<XElement> items = localityNode.Elements("object").ToList();
			foreach (XElement itemNode in items)
			{
				Name name = new(GetAttribute(itemNode, "indexedname"), GetAttribute(itemNode, "friendlyname"));
				Rectangle area = new Rectangle(GetAttribute(itemNode, "coordinates")).ToAbsolute(localityArea);
				string type = GetAttribute(itemNode, "type");
				bool decorative = GetAttribute(itemNode, "decorative").ToBool();
				bool pickable = GetAttribute(itemNode, "pickable").ToBool();
				bool usable = GetAttribute(itemNode, "usable").ToBool();

				GameObject obj = null;
				if (!itemObjects.TryGetValue(name.Indexed, out obj))
					throw new InvalidOperationException($"No geometry found for the item {name.Indexed}");
				Item item = ItemFactory.CreateItem(obj, name, area, type, decorative, pickable, usable);
				Add(item);
			}
		}

		private static void LoadItemDescriptions(XElement root)
		{
			IEnumerable<XElement> descriptionsNode = root.Element("objectdescriptions").Elements("object");
			foreach (XElement element in descriptionsNode)
			{
				string descriptions = GetAttribute(element, "descriptions");
				_objectDescriptions[GetAttribute(element, "type")] = descriptions.Split(new char[] { '|' });
			}
		}

		private static XDocument OpenMap()
		{
			string mapPath = Path.Combine(MainScript.MapPath, Settings.MapName + ".xml");
			return XDocument.Load(mapPath);
		}

		public static void LoadTerrain(XElement root)
		{
			Map = new(MainScript.MapPath);

			foreach (XElement locality in _localityNodes)
				Map.SaveTerrain(locality);
		}

		/// <summary>
		/// Plays the specified audio cutscene.
		/// </summary>
		/// <param name="sender">An object or NPC which wants to play the cutscene</param>
		/// <param name="cutscene">Name of the soudn file to be played</param>
		public static void PlayCutscene(object sender, string cutscene)
		{
			if (string.IsNullOrEmpty(cutscene))
				throw new ArgumentNullException(nameof(cutscene));

			_cutScenePlayer.Play(cutscene);
			_cutSceneMessage = new(null, cutscene);
			TakeMessage(_cutSceneMessage);

			// Stop it if cutscenes are forbidden for debugging purposes.
			//if (!Settings.PlayCutscenes)
			//	StopCutscene(null);
		}

		/// <summary>
		/// Terminates the application.
		/// </summary>
		public static void QuitGame()
		{
			GameInProgress = false;
			_cutSceneMessage = null;
			Sounds.MasterFadeOut(.5f);
			SaveGame();
			System.Threading.Thread.Sleep(1000);
			WindowHandler.MainMenu();
		}

		/// <summary>
		/// Sends a message to all NPCs.
		/// </summary>
		/// <param name="message">The message to be sent</param>
		public static void TakeMessage(Message message)
		{
			foreach (Character c in _characters.Values)
				c.TakeMessage(message);
		}

		/// <summary>
		/// Unregisters the specified locality.
		/// </summary>
		/// <param name="l">The locality to be removed</param>
		public static void Remove(Locality l)
		{
			_delayedActions.Enqueue(() => _localities.Remove(l.Name.Indexed));
		}

		/// <summary>
		/// Unregisters the specified passage.
		/// </summary>
		/// <param name="p">The passage to be removed</param>
		public static void Remove(Passage p)
		{
			_delayedActions.Enqueue(() => _passages.Remove(p.Name.Indexed));
		}

		/// <summary>
		/// Unregisters the specified object.
		/// </summary>
		/// <param name="o">The object to be removed</param>
		public static void Remove(Entity o)
		{
			_delayedActions.Enqueue(() => _items.Remove(o.Name.Indexed));
		}

		/// <summary>
		/// Saves the game state to the default file.
		/// </summary>
		public static void SaveGame()
		{
			SaveGame(MainScript.SerializationPath);
		}

		/// <summary>
		/// Saves sttate of the game into a specified binary file.
		/// </summary>
		/// <param name="path">Location of the save</param>
		/// <remarks>Used for testing purposes only. Allows creation of predefined saves.</remarks>
		public static void SaveGame(string path)
		{
			return; // todo nefunguje
			SerializerHelper helper = new(_characters, _items, _passages, _localities);
			FileStream stream = null;
			using (stream = File.Create(path))
			{
				ProtoBuf.Serializer.Serialize(stream, helper);
			}
			stream?.Close();
		}

		/// <summary>
		/// Starts game from the begining.
		/// </summary>
		public static void StartGame()
		{
			try
			{
				LoadMap();
			}
			catch (Exception e)
			{
				MainScript.OnError(e);
			}

			Player = CharacterFactory.CreateChipotle();
			Add(Player);

			foreach (Locality l in _localities.Values)
				l.Activate();

			foreach (Passage p in _passages.Values)
				p.Activate();

			foreach (Item i in _items.Values)
				i.Activate();

			Add(CharacterFactory.CreateTuttle());
			Add(CharacterFactory.CreateCarson());
			Add(CharacterFactory.CreateBartender());
			Add(CharacterFactory.CreateChristine());
			Add(CharacterFactory.CreateSweeney());
			Add(CharacterFactory.CreateMariotti());

			// start entities
			foreach (Character c in _characters.Values)
				c.Activate();

			//Play the first cutscene
			GameInProgress = true;
			PlayCutscene(null, "cs6");
		}

		/// <summary>
		/// Stops an ongoing audio cutscene.
		/// </summary>
		/// <param name="sender">The object or NPC which wants to stop the cutscene</param>
		public static void StopCutscene(object sender)
		{
			if (_cutSceneMessage == null)
				return;

			TakeMessage(new CutsceneEnded(_cutSceneMessage.Sender, _cutSceneMessage.CutsceneName));
			_cutScenePlayer.Stop();
			_cutSceneMessage = null;
		}

		/// <summary>
		/// Updates whole game world.
		/// </summary>
		public static void UpdateGame()
		{
			string[] playingLocalities = _localities.Values.Where(l => l.GetAmbientSource() != null).Select(l => l.Name.Indexed).ToArray();
			if (!GameInProgress)
				return;

			PerformDelayedActions();
			foreach (Locality locality in _localities.Values)
				locality.GameUpdate();

			foreach (Passage passage in _passages.Values)
				passage.GameUpdate();

			foreach (Item item in _items.Values)
				item.GameUpdate();

			foreach (Character character in _characters.Values)
				character.GameUpdate();

			HandleCutscene();
			WindowHandler.ActiveWindow.GameUpdate();
		}

		/// <summary>
		/// Watches an ongoing audio cutscene and informs the world when it's completed.
		/// </summary>
		private static void HandleCutscene()
		{
			if (_cutSceneMessage != null && _cutScenePlayer.Finished)
			{
				TakeMessage(new CutsceneEnded(_cutSceneMessage.Sender, _cutSceneMessage.CutsceneName));
				_cutSceneMessage = null;
			}
		}

		/// <summary>
		/// Performs all planned actions.
		/// </summary>
		private static void PerformDelayedActions()
		{
			while (!_delayedActions.IsNullOrEmpty())
				_delayedActions.Dequeue()();
		}
	}
}