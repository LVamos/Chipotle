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

namespace Game
{
    /// <summary>
    /// Represents the game world.
    /// </summary>
    public static class World
    {
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
        private static readonly Dictionary<string, string> _localityLoops = new Dictionary<string, string>()
        {
            ["chodba h1"] = "CzechPubLoop",
            ["balkon p1"] = "BalconyLoop",
            ["terasa w1"] = "BalconyLoop",
            ["výčep h1"] = "CzechPubLoop",
            ["ulice h1"] = "BonitaStreetLoop",
            ["příjezdová cesta w1"] = "DriveWayLoop",
            ["bazén w1"] = "PoolLoop",
            ["zahrada c1"] = "CarsonsGardenLoop",
            ["asfaltka c1"] = "AsphaltRoadLoop",
            ["cesta c1"] = "AsphaltRoadLoop",
            ["ulice p1"] = "BelvedereStreetLoop",
            ["ulice v1"] = "GordonStreetLoop",
            ["garáž v1"] = "GarageLoop",
            ["garáž s1"] = "GarageLoop",
            ["garáž w1"] = "GarageLoop",
            ["garáž p1"] = "GarageLoop",
            ["ulice s1"] = "BonitaStreetLoop",
            ["dvorek s1"] = "DriveWayLoop",
        };

        /// <summary>
        /// Path to a map file
        /// </summary>
        private static readonly string MapPath = Path.Combine(Program.DataPath, @"Map\chipotle.xml");

        /// <summary>
        /// Information about an ongoing cutscene
        /// </summary>
        private static CutsceneBegan _cutsceneBegan;

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
        public static Entity Player => GetEntity("Chipotle");

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
        public static int GetDistance(Vector2 a, Vector2 b)
=> (int)(Math.Abs(a.X - b.X) + Math.Abs(a.Y - b.Y));

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
=>  _entities.TryGetValue(name.ToLower(), out Entity e) ? e : null;

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
            => _objects.OrderBy(o => o.Value.Area.GetDistanceFrom(point))
            .Where(o => o.Value.Name.Indexed != World.GetObject(point)?.Name.Indexed)
            .Select(o => o.Value);

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
        /// <returns>Enumeration of all passages</returns>
        public static IEnumerable<Passage> GetNearestPassages(Vector2 point)
                   => _passages.Values
            .OrderBy(p => p.Area.GetDistanceFrom(point))
            .Where(p => p != World.GetPassage(point));

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
            GameObject obj = locality?.Objects.FirstOrDefault(o => o.Area.LaysOnPlane(point));
            return obj
                ?? locality.Entities.FirstOrDefault(e => e.Area.LaysOnPlane(point));
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
            Tile tile = Map[point];
            return tile != null && tile.Permeable && !IsOccupied(point);
        }

        /// <summary>
        /// Loads a saved game state from a binary file.
        /// </summary>
        public static void LoadGame()
        {
            if (Sound == null)
            {
                Tolk.Load();
                SoundInit(Program.TolkDelegate);
            }

            // Load terrain, objects, NPCs, localities and passages.
            LoadTerrain();
            FileStream stream;
            BinaryFormatter formatter = new BinaryFormatter();
            Serializer serializer;

            using (stream = File.OpenRead(Program.SerializationPath))
            {
                serializer = (Serializer)formatter.Deserialize(stream);
            }
            stream.Close();

            _entities = serializer.Entities;
            _objects = serializer.Objects;
            _passages = serializer.Passages;
            _localities = serializer.Localities;
            Program.MainWindow.GameLoopEnabled = true;
            WindowHandler.Switch(new Game.UI.GameWindow());
            GameReloaded message = new GameReloaded();
            Player.Area.GetLocality().ReceiveMessage(message);
            _objects.Values.Foreach(o => o.ReceiveMessage(message));
        }

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
                _localityLoops.TryGetValue(A(l, "indexedname"), out string lLoop);
                Locality locality = new Locality(
           new Name(A(l, "indexedname"), A(l, "friendlyname")),
           A(l, "to"),
           A(l, "type").ToLocalityType(),
           int.Parse(A(l, "height")),
           new Plane(A(l, "coordinates")),
           A(l, "defaultTerrain", false).ToTerrainType(),
lLoop
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

            // Place passages
            foreach (XElement p in xPassages)
            {
                Name pIndexedName = new Name(A(p, "indexedname"));
                bool isDoor = A(p, "door").ToBool();
                bool closed = A(p, "closed").ToBool();
                bool openable = A(p, "openable").ToBool();
                Plane area = new Plane(A(p, "coordinates"));
                List<Locality> localities = new List<Locality> { GetLocality(A(p, "from")), GetLocality(A(p, "to").PrepareForIndexing()) };

                // Create and register new passage
                Add(Passage.CreatePassage(pIndexedName, area, localities, isDoor, closed, openable));
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

            if (_cutsceneBegan != null)
                StopCutscene(null);

            int id = Sound.Play(cutscene);
            _cutsceneBegan = new CutsceneBegan(sender, cutscene, id);
            ReceiveMessage(_cutsceneBegan);
        }

        /// <summary>
        /// Terminates the application.
        /// </summary>
        public static void QuitGame()
        {
            Program.MainWindow.GameLoopEnabled = false;
            _cutsceneBegan = null;
            Sound.FadeAndStopAll(.00008f);
            System.Threading.Thread.Sleep(1000);
            WindowHandler.MainMenu();
        }

        /// <summary>
        /// Sends a message to all NPCs.
        /// </summary>
        /// <param name="message">The message to be sent</param>
        public static void ReceiveMessage(GameMessage message)
            => _entities.Values.Foreach(e => e.ReceiveMessage(message));

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
        /// Saves sttate of the game into a binary file.
        /// </summary>
        public static void SaveGame()
        {
            Serializer serializer = new Serializer(_entities, _objects, _passages, _localities);
            BinaryFormatter formatter = new BinaryFormatter();

            FileStream stream;
            using (stream = File.Create(Program.SerializationPath))
            {
                formatter.Serialize(stream, serializer);
            }
            stream.Close();
        }

        /// <summary>
        /// Initializes the sound player.
        /// </summary>
        /// <param name="say">A delegate for text output</param>
        public static void SoundInit(Action<string> say)
        {
            Sound = SoundThread.CreateAndStartThread(Path.Combine(Program.DataPath, "Sounds"), Program.OnError, say);
            Sound.LoadSounds();
            Sound.SetGroupVolume("master", 1);
        }

        /// <summary>
        /// Starts game from the begining.
        /// </summary>
        public static void StartGame()
        {
            //Sound.SetGroupVolume("master", 0);
            //Sound.FadeMasterIn(.00001f, 3);
            LoadMap();
            _localities.Foreach(p => p.Value.Start());
            _passages.Foreach(p => p.Value.Start());
            _objects.Foreach(p => p.Value.Start());
            Add(Entity.CreateChipotle());
            Add(Entity.CreateTuttle());
            Add(Entity.CreateCarson());
            Add(Entity.CreateBartender());
            Add(Entity.CreateChristine());
            Add(Entity.CreateSweeney());
            Add(Entity.CreateMariotti());
            _entities.Foreach(p => p.Value.Start());
            Program.MainWindow.GameLoopEnabled = true;
        }

        /// <summary>
        /// Stops an ongoing audio cutscene.
        /// </summary>
        /// <param name="sender">The object or NPC which wants to stop the cutscene</param>
        public static void StopCutscene(object sender)
        {
            Sound.Stop(_cutsceneBegan.SoundID);
            ReceiveMessage(new CutsceneEnded(_cutsceneBegan.Sender, _cutsceneBegan.CutsceneName, _cutsceneBegan.SoundID));
            _cutsceneBegan = null;
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
            if (_cutsceneBegan != null)
            {
                Sound.GetDynamicInfo(_cutsceneBegan.SoundID, out SoundState state, out int sample);

                if (state != SoundState.Playing)
                {
                    Sound.GetStaticInfo(_cutsceneBegan.SoundID, out _, out int totalSamples, out _);
                    if (sample == totalSamples)
                    {
                        ReceiveMessage(new CutsceneEnded(_cutsceneBegan.Sender, _cutsceneBegan.CutsceneName, _cutsceneBegan.SoundID));
                        _cutsceneBegan = null;
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