using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using DavyKager;

using Game.Entities;
using Game.Messaging;
using Game.Messaging.Events;
using Game.Terrain;
using Game.UI;

using Luky;

using OpenTK;

using ProtoBuf;
using ProtoBuf.Meta;
using System.Windows.Forms;
using Microsoft.VisualBasic;
using Game.Messaging.Commands;

namespace Game
{
	/// <summary>
	/// Represents the game world.
	/// </summary>
	public static class World
	{
		private static readonly float _cutsceneVolume = .5f;

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
			if (_cutscene.message == null || !_cutscene.paused)
				return;

			// Rewind the scene about 2 secs back.
			int position = _cutscene.position - 192000; // 2 secs
			if (position < 0)
				position = 0;

			// Play it
			int id = Sound.Play(_cutscene.message.CutsceneName, null, false, PositionType.None, Vector3.Zero, false, 0, null, 1, position);
			Sound.FadeSource(id, FadingType.In, .0001f, 1, false);
			// Refresh information about paused cutscene.
			_cutscene.message = new CutsceneBegan(null, _cutscene.message.CutsceneName, id);
			_cutscene.paused = false;
			_cutscene.position = 0;


		}

		/// <summary>
		/// Pauses an ongoing cutscene.
		/// </summary>
		public static void PauseCutscene()
		{
			if (_cutscene.message == null || _cutscene.paused)
				return;

			_cutscene.paused = true;
			Sound.GetDynamicInfo(_cutscene.message.SoundID, out SoundState _, out _cutscene.position);
			Sound.FadeSource(_cutscene.message.SoundID, FadingType.Out, .00001f, 0, true);
		}

		/// <summary>
		/// Detects acoustic obstacles between the player and the specified map element.
		/// </summary>
		/// <param name="area">The map element to be checked</param>
		/// <returns>The corresponding obstacle type</returns>
		public static ObstacleType DetectAcousticObstacles(Plane area)
		{
			Locality playersLocality = Player.Locality;
			Locality otherLocality = area.GetLocality();
			bool neighbour = playersLocality.IsNeighbour(otherLocality);
			bool accessible = otherLocality.IsAccessible(playersLocality);

			// Are the regions in inadjecting localities?
			if (GetDistance(area.Center, Player.Area.Center) > 100 ||
				(playersLocality != otherLocality && (!neighbour || (neighbour && !accessible))))
				return ObstacleType.Far;  // Inaudible

			// Adjecting localities
			Plane path = new Plane(area.GetClosestPoint(Player.Area.Center), Player.Area.Center);
			ObstacleType obstacle = DetectObstacles(path);

			if (neighbour && accessible)
			{
				Passage atPassage = Player.Locality.IsAtPassage(area.Center);

				if (obstacle == ObstacleType.IndirectPath && atPassage == null)
					return ObstacleType.Wall;

				if (atPassage != null)
				{
					if (atPassage.State == PassageState.Closed)
						return ObstacleType.Door;

					return ObstacleType.None;
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
		public static ObstacleType DetectObstacles(Plane path)
		{
			bool Intersects(Plane area)
				=> area.LaysOnPlane(path.UpperLeftCorner) || area.LaysOnPlane(path.LowerRightCorner);

			// Is it a line?
			if (!path.IsLine)
				return ObstacleType.IndirectPath;

			// Detect doors.
			if (
			path.GetIntersectingPassages()
			.Any(p => !Intersects(p.Area) && p.State == PassageState.Closed))
				return ObstacleType.Door;

			// Detect walls
			if (
				path.GetTiles()
				.Where(t => !Intersects(new Plane(t.position)))
				.Any(t => t.tile.Terrain == TerrainType.Wall)
				|| path.GetIntersectingObjects().Any(o => !Intersects(o.Area) && (o.Type == "zeď" || o.Name.Friendly == "zeď"))
				)
				return ObstacleType.Wall;

			// Detect objects
			if (path.GetIntersectingObjects().Any(o => !Intersects(o.Area)))
				return ObstacleType.Object;

			return ObstacleType.None;
		}

		/// <summary>
		/// Enumerates all localities.
		/// </summary>
		/// <returns>Enumeration of all localities</returns>
		public static IEnumerable<Locality> GetLocalities()
=> _localities.Values;
		/// <summary>
		/// Interval between game loop ticks
		/// </summary>
		public const int DeltaTime = 1000 / FramesPerSecond;

		/// <summary>
		/// sets speed of the game loop.
		/// </summary>
		public const int FramesPerSecond = 100;

		/// <summary>
		/// Reference to a sound player
		/// </summary>
		public static SoundThread Sound;

		/// <summary>
		/// Queue of actions that should be performed at the beginning of the game loop tick
		/// </summary>
		private static readonly Queue<Action> _delayedActions = new Queue<Action>();

		/// <summary>
		/// Map of localities and corresponding background sounds
		/// </summary>
		private static readonly Dictionary<string, (string sound, float volume)> _localityLoops = new Dictionary<string, (string sound, float volume)>
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
		/// Path to a map file
		/// </summary>
		private static readonly string MapPath = Path.Combine(Program.DataPath, @"Map\chipotle.xml");

		/// <summary>
		/// Information about an ongoing cutscene
		/// </summary>
		private static (CutsceneBegan message, bool paused, int position) _cutscene;

		/// <summary>
		/// List of all NPCs
		/// </summary>
		private static Dictionary<string, Entity> _entities;

		/// <summary>
		/// List of all localities
		/// </summary>
		private static Dictionary<string, Locality> _localities;

		/// <summary>
		/// List of all simple game objects
		/// </summary>
		private static Dictionary<string, DumpObject> _objects;

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
		public static Entity Player
=> GetEntity("Chipotle");

		/// <summary>
		/// Registers an entity.
		/// </summary>
		/// <param name="e">The entity to be registered</param>
		public static void Add(Entity e)
		{
			// Do a null check and look if entity isn't already registered.
			if (e == null)
				throw new ArgumentNullException(nameof(e));

			if (_entities.ContainsKey(e.Name.Indexed))
				throw new ArgumentException("entity already registered");

			_entities.Add(e.Name.Indexed, e);
		}

		/// <summary>
		/// Registers a game object.
		/// </summary>
		/// <param name="o">The game object to be added</param>
		public static void Add(DumpObject o)
		{
			// Do a null check and look if object isn't already registered.
			if (o == null)
				throw new ArgumentNullException(nameof(o));

			if (_objects.ContainsKey(o.Name.Indexed))
				throw new ArgumentException("Object already registered");

			_objects.Add(o.Name.Indexed, o); // Added to dictionary
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
		/// Builds walls around the given region.
		/// </summary>
		/// <param name="walls">Specifies on which sides of the region the walls are to be built.</param>
		public static void DrawWalls(Plane area, string walls)
		{
			IEnumerable<Vector2> wallCoordinates;

			if (walls == "All")
				wallCoordinates = area.GetPerimeterPoints();
			else
			{
				List<Direction> directions = new List<Direction>();

				foreach (string word in Regex.Split(walls, @", +"))
				{
					switch (word.ToLower())
					{
						case "left": directions.Add(Direction.Left); break;
						case "front": directions.Add(Direction.Up); break;
						case "right": directions.Add(Direction.Right); break;
						case "back": directions.Add(Direction.Down); break;
						default: throw new ArgumentException(nameof(word));
					}
				}

				wallCoordinates = area.GetPerimeterPoints(directions);
			}

			wallCoordinates.Foreach(c => World.Map[c].Register(TerrainType.Wall, false));
		}

		/// <summary>
		/// Calculates an angle between two point acording to the specified orientation.
		/// </summary>
		/// <param name="a">The first point</param>
		/// <param name="b">The second point</param>
		/// <param name="orientation">The orientation according to which the angle should be calculated</param>
		/// <returns>Compass degrees</returns>
		public static double GetAngle(Vector2 a, Vector2 b, Orientation2D orientation)
		{
			double x = a.X - b.X;
			double y = a.Y - b.Y;
			double z = Math.Round(Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)));
			Angle angle = new Angle(Math.Atan2(y, x)) + Angle.FromCartesianDegrees(orientation.Angle.CompassDegrees);
			return Math.Round(angle.CompassDegrees);
		}

		/// <summary>
		/// Computes cartesian distance between two points.
		/// </summary>
		/// <param name="a">First point</param>
		/// <param name="b">Second point</param>
		/// <returns>Rounded distance between two given points</returns>
		public static float GetDistance(Vector2 a, Vector2 b)
=> (float)(Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y));


		/// <summary>
		/// Computes cartesian distance between two 3d points.
		/// </summary>
		/// <param name="a">First point</param>
		/// <param name="b">Second point</param>
		/// <returns>Distance between two given points</returns>
		public static float GetDistance(Vector3 a, Vector3 b)
=> (float)(Math.Pow(Math.Abs(a.X - b.X), 2) + Math.Pow(Math.Abs(a.Y - b.Y), 2) + Math.Pow(Math.Abs(a.Z - b.Z), 2)) * 0.5f;

		/// <summary>
		/// Returns an entity that stands on the given position.
		/// </summary>
		/// <param name="point"></param>
		/// <returns>Reference to the entity if there's any</returns>
		public static Entity GetEntity(Vector2 point)
		{
			if (GetObject(point) is Entity entity)
				return entity;
			return null;
		}

		/// <summary>
		/// Returns an NPC found by its name.
		/// </summary>
		/// <param name="name">Inner name of the required NPC</param>
		/// <returns>The found NPC or null if nothing was found</returns>
		public static Entity GetEntity(string name)
		{
			if (_entities != null && _entities.TryGetValue(name.ToLower(), out Entity e))
				return e;
			return null;
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
		public static Locality GetLocality(Plane area)
			=> _localities.Values.FirstOrDefault(l => l.Area.Contains(area));

		/// <summary>
		/// Returns a locality which intersects with the given point.
		/// </summary>
		/// <param name="point">The point tto be checked</param>
		/// <returns>The intersecting locality</returns>
		public static Locality GetLocality(Vector2 point)
			=> _localities.Values
			.FirstOrDefault(l => l.Area.LaysOnPlane(point));

		/// <summary>
		/// Enumerates all localities sorted by distance from the specified point.
		/// </summary>
		/// <param name="point">The point whose surroundings should be explored</param>
		/// <returns>Enumeration of the found localities</returns>
		public static IEnumerable<Locality> GetNearestLocalities(Vector2 point)
		   => _localities.OrderBy(p => p.Value.Area.GetDistanceFrom(point))
			.Where(p => p.Value != GetLocality(point))
			.Select(p => p.Value);

		/// <summary>
		/// Returns the locality nearest from the specified point.
		/// </summary>
		/// <param name="point">The point shose surroundings is to be searched</param>
		/// <returns>The found locality</returns>
		public static Locality GetNearestLocality(Vector2 point)
					=> GetNearestLocalities(point).First();

		/// <summary>
		/// Returns a game object closest to the specified point.
		/// </summary>
		/// <param name="point">The point whose surroundings is to be searched</param>
		/// <returns>The found game object</returns>
		public static GameObject GetNearestObject(Vector2 point)
			=> GetNearestObjects(point).FirstOrDefault();

		/// <summary>
		/// Enumerates all game objects around a point sorted by distance.
		/// </summary>
		/// <param name="point">A point whose surroundings are to be searched</param>
		/// <returns>Enumeration of game objects</returns>
		public static IEnumerable<GameObject> GetNearestObjects(Vector2 point)
			=> _objects.Values.OrderBy(o => o.Area.GetDistanceFrom(point))
			.Where(o => !o.Area.LaysOnPlane(point));

		/// <summary>
		/// Returns the passage closest to the specified point.
		/// </summary>
		/// <param name="point">The point whose surrounding is to be searched</param>
		/// <returns>The found passage</returns>
		public static Passage GetNearestPassage(Vector2 point)
		=> GetNearestPassages(point).FirstOrDefault();

		/// <summary>
		/// Enumerates all passages sorted by distance from the specified point.
		/// </summary>
		/// <param name="point">The point whose surrounding is to be searched</param>
		/// <param name="doors">Specifies if the result should should be narrowed to doors only</param>
		/// <returns>Enumeration of all passages</returns>
		public static IEnumerable<Passage> GetNearestPassages(Vector2 point, bool doors = false)
		{
			return
				   from p in _passages.Values
				   let d = p.Area.GetDistanceFrom(point)
				   orderby d
				   where !p.Area.LaysOnPlane(point) &&
							   ((doors && p is Door) || (!doors && p is Passage))
				   select p;
		}

		/// <summary>
		/// Enumerates all passages sorted by distance from the specified point.
		/// </summary>
		/// <param name="point">The point whose surrounding is to be searched</param>
		/// <param name="maxDistance">Max allowed distance from the specified point</param>
		/// <returns>Enumeration of all passages</returns>
		public static IEnumerable<Passage> GetNearestPassages(Vector2 point, bool doors, float maxDistance)
			=> GetNearestPassages(point, doors).Where(p => p.Area.GetDistanceFrom(point) <= maxDistance);

		/// <summary>
		/// searches for a simple game object by name.
		/// </summary>
		/// <param name="name">Inner name of the required object</param>
		/// <returns>The found game object or null if nothing was found</returns>
		public static DumpObject GetObject(string name)
=> _objects.TryGetValue(name, out DumpObject o) ? o : null;

		/// <summary>
		/// Returns an NPC or game object the tile intersects.
		/// </summary>
		public static GameObject GetObject(Vector2 point)
		{
			Locality locality = GetLocality(point);
			if (locality == null)
				return null;

			DumpObject obj = locality.Objects.FirstOrDefault(o => o.Area.LaysOnPlane(point));
			if (obj != null)
				return obj;

			Entity entity = locality.Entities.FirstOrDefault(e => e.Area.LaysOnPlane(point));
			return entity;
		}

		/// <summary>
		/// Returns all game objects that intersect with the given plane.
		/// </summary>
		/// <param name="area">The plane to be checked.</param>
		/// <returns>Enumeration of intersecting objects</returns>
		public static IEnumerable<DumpObject> GetObjects(Plane area)
			=> _objects.Values.Where(o => o.Area.Intersects(area));

		/// <summary>
		/// Enumerates all simple game objects of the specified type
		/// </summary>
		/// <param name="type">Type of requested game objects</param>
		/// <returns>Enumeration of game objects</returns>
		public static IEnumerable<DumpObject> GetObjectsByType(string type)
			=> _objects.Values.Where(o => !string.IsNullOrEmpty(o.Type) && o.Type.ToLower(CultureInfo.CurrentCulture) == type.ToLower(CultureInfo.CurrentCulture));

		/// <summary>
		/// Returns a passage the tile intersects.
		/// </summary>
		/// <param name="point">The point to be checked</param>
		/// <returns>The passage if there's any</returns>
		public static Passage GetPassage(Vector2 point)
		{
			Locality locality = GetLocality(point);
			return locality?.Passages.FirstOrDefault(p => p.Area.LaysOnPlane(point));
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
		public static IEnumerable<Passage> GetPassages(Plane area)
					=> _passages.Values.Where(p => p.Area.Intersects(area));

		/// <summary>
		/// Prepares the game world.
		/// </summary>
		public static void Initialize()
		{
			_objects = new Dictionary<string, DumpObject>();
			_localities = new Dictionary<string, Locality>();
			_entities = new Dictionary<string, Entity>();
			_passages = new Dictionary<string, Passage>();
		}

		/// <summary>
		/// Indicates if a tile on the specified position is occupied by an NPC or game object.
		/// </summary>
		/// <param name="point"></param>
		/// <returns>True if there's an object or NPC on the specified position</returns>
		public static bool IsOccupied(Vector2 point)
			=> GetObject(point) != null || GetEntity(point) != null;

		/// <summary>
		/// Checks if the tile is walkable for NPCs.
		/// </summary>
		public static bool IsWalkable(Vector2 point)
		{
			Tile t = Map[point];

			Passage p = GetPassage(point);
			return
				t != null && t.Walkable && !IsOccupied(point)
				&& (p == null || (p != null && p.State == PassageState.Open));
		}
		
		/// <summary>
		/// Creates a predefined game state save. Asks for its name.
		/// </summary>
		/// <returns>True on success</returns>
		public static bool CreatePredefinedSave()
		{
			string name =
				Interaction.InputBox(String.Empty, "Zadej název sejvu");
			if (string.IsNullOrEmpty(name))
				return false;

			if (!Directory.Exists(Program.PredefinedSavesPath))
				Directory.CreateDirectory(Program.PredefinedSavesPath);
			string path = Path.Combine(Program.PredefinedSavesPath, name);
			if (!Directory.Exists(path))
				Directory.CreateDirectory(path);

			path = Path.Combine(path, "game.sav");
			SaveGame(path);
			return true;
		}

		/// <summary>
		/// Starts the menu for selecting a predefined save and loads the selected save into memory.
		/// </summary>
		/// <returns>True if a save was selected.</returns>
		/// <remarks>for testing purposes only.</remarks>
		public static bool LoadPredefinedSave()
		{
			string[] saves =
				Directory.GetDirectories(Program.PredefinedSavesPath);

			string[] items =
				saves.Select(s => Path.GetFileName(s))
				.ToArray<string>();

			int i = 
				WindowHandler.Menu(items, "Kterej sejv chceš načíst?", true);
			if (i == -1)
				return false;

			string path = Path.Combine(saves[i], "game.sav");
			LoadGame(path);
			return true;
		}

		/// <summary>
		/// Loads a saved game state from default path.
		/// </summary>
		public static void LoadGame()
			=> LoadGame(Program.SerializationPath);

		/// <summary>
		/// Loads a saved game state from the specified binary file.
		/// </summary>
		/// <param name="path">Path to the saved game state</param>
		/// <remarks>Used just for testing purposes. Allows opening predefined saves.</remarks>
		public static void LoadGame(string path)
		{
			if (Sound == null)
			{
				Tolk.Load();
				SoundInit(Program.TolkDelegate);
			}

			// Load terrain, objects, NPCs, localities and passages.
			LoadTerrain();
			FileStream stream = null;
			SerializerHelper helper;

			using (stream = File.OpenRead(path))
			{
				helper = Serializer.Deserialize<SerializerHelper>(stream);
			}
			stream?.Close();

			_entities = helper.Entities;
			_objects = helper.Objects;
			_passages = helper.Passages;
			_localities = helper.Localities;
			WindowHandler.Switch(new Game.UI.GameWindow());
			GameReloaded message = new GameReloaded();
			_entities.Values.Foreach(e => e.TakeMessage(message));
			_localities.Values.Foreach(l => l.TakeMessage(message));
			_objects.Values.Foreach(o => o.TakeMessage(message));
			_passages.Values.Foreach(p => p.TakeMessage(message));
			Program.MainWindow.GameLoopEnabled = true;
		}

		/// <summary>
		/// Static constructor
		/// </summary>
		static World()
				=> RuntimeTypeModel.Default.Add(typeof(Vector2), false).Add("X", "Y");

		/// <summary>
		/// Loads the map from file.
		/// </summary>
		public static void LoadMap()
		{
			string A(XElement element, string attribute, bool prepareForIndexing = true)
				=> prepareForIndexing ? element.Attribute(attribute).Value.PrepareForIndexing() : element.Attribute(attribute).Value;

			XDocument xDocument = XDocument.Load(MapPath);
			XElement root = xDocument.Root;
			IEnumerable<XElement> xLocalities = root.Element("localities").Elements("locality");
			IEnumerable<XElement> xPassages = root.Element("passages").Elements("passage");

			Initialize(); // Prepare data structures for objects, entities, localities etc.

			// Create map
			Map = new TileMap(MapPath);

			// Load localities
			foreach (XElement l in xLocalities)
			{
				(string sound, float volume) lBackgroundInfo;
				_localityLoops.TryGetValue(A(l, "indexedname"), out lBackgroundInfo);
				Locality locality = new Locality(
		   new Name(A(l, "indexedname"), A(l, "friendlyname")),
		   A(l, "to"),
		   A(l, "type") == "indoor" ? Locality.LocalityType.Indoor : Locality.LocalityType.Outdoor,
		   int.Parse(A(l, "height")),
		   new Plane(A(l, "coordinates")),
		   A(l, "defaultTerrain", false).ToTerrainType(),
lBackgroundInfo.sound,
lBackgroundInfo.volume
);
				Add(locality);

				// Create perimeter walls if they are specified in the map
				string wallDefinition = A(l, "walls", false);

				if (wallDefinition != "None")
					DrawWalls(locality.Area, wallDefinition);

				// Draw terrain
				l.Elements("panel").Foreach(p => Map.DrawTerrain(p, locality.Area));

				// Load game objects
				foreach (XElement o in l.Elements("object"))
				{
					Add(GameObject.CreateObject(
						new Name(A(o, "indexedname"), A(o, "friendlyname")),
						new Plane(A(o, "coordinates")).ToAbsolute(locality.Area),
						A(o, "type"),
						A(o, "decorative").ToBool()
						));
				}
			}

			// Load passages
			foreach (XElement p in xPassages)
			{
				Name pIndexedName = new Name(A(p, "indexedname"));
				bool isDoor = A(p, "door").ToBool();
				PassageState state = A(p, "closed").ToBool() ? PassageState.Closed : PassageState.Open;
				bool openable = A(p, "openable").ToBool();
				Plane area = new Plane(A(p, "coordinates"));
				string[] localities = new string[]
				{ A(p, "from"),
					A(p, "to")
				};
				Door.DoorType dType = A(p, "type") == "door" ? Door.DoorType.Door : Door.DoorType.Gate;

				// Create and register new passage
				Add(Passage.CreatePassage(pIndexedName, area, localities, isDoor, state, openable, dType));
			}
		}

		public static void LoadTerrain()
		{
			string Attribute(XElement element, string attribute, bool prepareForIndexing = true)
	=> prepareForIndexing ? element.Attribute(attribute).Value.PrepareForIndexing() : element.Attribute(attribute).Value;

			XElement root = XDocument.Load(MapPath).Root;
			IEnumerable<XElement> xLocalities = root.Element("localities").Elements("locality");

			// Create map
			Map = new TileMap(MapPath);

			// Load localities
			foreach (XElement l in xLocalities)
			{
				Plane area = new Plane(Attribute(l, "coordinates"));

				// Draw terrain
				TerrainType terrain = Attribute(l, "defaultTerrain", false).ToTerrainType();
				Map.DrawTerrain(area, terrain, true);
				l.Elements("panel")
					.Foreach(p => Map.DrawTerrain(p, area));

				// Create perimeter walls if they are specified in the map
				string wallDefinition = Attribute(l, "walls", false);

				if (wallDefinition != "None")
					DrawWalls(area, wallDefinition);

				// Load terrain panels
				l.Elements("panel").Foreach(p => Map.DrawTerrain(p, area));
			}
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

			if (_cutscene.message != null)
				StopCutscene(null);

			int id = Sound.Play(Sound.GetRandomSoundStream(cutscene), null, false, PositionType.None, Vector3.Zero, false, _cutsceneVolume);
			_cutscene.message = new CutsceneBegan(sender, cutscene, id);
			TakeMessage(_cutscene.message);
		}

		/// <summary>
		/// Terminates the application.
		/// </summary>
		public static void QuitGame()
		{
			Program.MainWindow.GameLoopEnabled = false;
			_cutscene.message = null;
			Sound.FadeAndStopAll(.0002f);
			SaveGame();
			System.Threading.Thread.Sleep(1000);
			WindowHandler.MainMenu();
		}

		/// <summary>
		/// Sends a message to all NPCs.
		/// </summary>
		/// <param name="message">The message to be sent</param>
		public static void TakeMessage(GameMessage message)
			=> _entities.Values.Foreach(e => e.TakeMessage(message));

		/// <summary>
		/// Unregisters the specified locality.
		/// </summary>
		/// <param name="l">The locality to be removed</param>
		public static void Remove(Locality l)
					=> _delayedActions.Enqueue(() => _localities.Remove(l.Name.Indexed));

		/// <summary>
		/// Unregisters the specified passage.
		/// </summary>
		/// <param name="p">The passage to be removed</param>
		public static void Remove(Passage p)
					=> _delayedActions.Enqueue(() => _passages.Remove(p.Name.Indexed));

		/// <summary>
		/// Unregisters the specified object.
		/// </summary>
		/// <param name="o">The object to be removed</param>
		public static void Remove(GameObject o)
					=> _delayedActions.Enqueue(() => _objects.Remove(o.Name.Indexed));

		/// <summary>
		/// Saves the game state to the default file.
		/// </summary>
		public static void SaveGame()
			=> SaveGame(Program.SerializationPath);

		/// <summary>
		/// Saves sttate of the game into a specified binary file.
		/// </summary>
		/// <param name="path">Location of the save</param>
		/// <remarks>Used for testing purposes only. Allows creation of predefined saves.</remarks>
		public static void SaveGame(string path)
		{
			SerializerHelper helper = new SerializerHelper(_entities, _objects, _passages, _localities);
			FileStream stream = null;
			using (stream = File.Create(path))
			{
				Serializer.Serialize(stream, helper);
			}
			stream?.Close();
		}

		/// <summary>
		/// Initializes the sound player.
		/// </summary>
		/// <param name="say">A delegate for text output</param>
		public static void SoundInit(Action<string> say)
		{
			Sound = SoundThread.CreateAndStartThread(Path.Combine(Program.DataPath, "Sounds"), Program.OnError, say);
			Sound.LoadSounds();
			Sound.SetGroupVolume("master", Sound.DefaultMasterVolume);
		}

		/// <summary>
		/// Starts game from the begining.
		/// </summary>
		public static void StartGame()
		{
			Sound.SetGroupVolume("master", Sound.DefaultMasterVolume);
			LoadMap();
			Add(Entity.CreateChipotle());
			_localities.Foreach(p => p.Value.Start());
			_passages.Foreach(p => p.Value.Start());
			_objects.Foreach(p => p.Value.Start());
			Add(Entity.CreateTuttle());
			Add(Entity.CreateCarson());
			Add(Entity.CreateBartender());
			Add(Entity.CreateChristine());
			Add(Entity.CreateSweeney());
			Add(Entity.CreateMariotti());

			// start entities
			foreach (Entity e in _entities.Values)
				e.Start();

			Program.MainWindow.GameLoopEnabled = true;

			// Play the first cutscene
			PlayCutscene(null, "cs6");

			if (!Program.Settings.PlayCutscenes)
				StopCutscene(null);
		}

		/// <summary>
		/// A test mode method to copy friendly names of all objects to clipboard.
		/// </summary>
		private static void CopyObjectsToClipboard()
		{
			IEnumerable<string> objects = _objects.Values.OrderBy(o => o.Name.Friendly).Select(o => o.Name.Friendly).Distinct();
			string text = string.Join(Environment.NewLine, objects.ToArray<string>());
			Clipboard.SetText(text);
		}


		/// <summary>
		/// Stops an ongoing audio cutscene.
		/// </summary>
		/// <param name="sender">The object or NPC which wants to stop the cutscene</param>
		public static void StopCutscene(object sender)
		{
			if (_cutscene.message == null)
				return;

			Sound.FadeSource(_cutscene.message.SoundID, FadingType.Out, .0001f, 0);
			TakeMessage(new CutsceneEnded(_cutscene.message.Sender, _cutscene.message.CutsceneName, _cutscene.message.SoundID));
			_cutscene.message = null;
			_cutscene.paused = false;
		}

		/// <summary>
		/// Updates whole game world.
		/// </summary>
		public static void Update()
		{
			PerformDelayedActions();
			_localities.Foreach(v => v.Value.Update());
			_passages.Foreach(v => v.Value.Update());
			_objects.Foreach(v => v.Value.Update());
			_entities.Foreach(v => v.Value.Update());
			HandleCutscene();
		}

		/// <summary>
		/// Watches an ongoing audio cutscene and informs the world when it's completed.
		/// </summary>
		private static void HandleCutscene()
		{
			if (_cutscene.message != null)
			{
				Sound.GetDynamicInfo(_cutscene.message.SoundID, out SoundState state, out int sample);

				if (state != SoundState.Playing)
				{
					Sound.GetStaticInfo(_cutscene.message.SoundID, out _, out int totalSamples, out _);

					if (sample == totalSamples && !_cutscene.paused)
					{
						TakeMessage(new CutsceneEnded(_cutscene.message.Sender, _cutscene.message.CutsceneName, _cutscene.message.SoundID));
						_cutscene.message = null;
					}
				}
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