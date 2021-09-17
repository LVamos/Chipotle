using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

using Game.Entities;
using Game.Messaging;
using Game.Messaging.Events;
using Game.Terrain;
using Game.UI;

using Luky;

using OpenTK;
using System.Globalization;

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
        /// Path to a map file
        /// </summary>
        private static readonly string MapPath = Path.Combine(Program.DataPath, @"Map\chipotle.xml");

        /// <summary>
        /// Information about an ongoing cutscene
        /// </summary>
        private static CutsceneBegan _cutsceneBegan;

        /// <summary>
        /// Queue of actions that should be performed at the beginning of the game loop tick
        /// </summary>
        private static Queue<Action> _delayedActions = new Queue<Action>();

        /// <summary>
        /// List of all NPCs
        /// </summary>
        private static Dictionary<string, Entity> _entities;

        /// <summary>
        /// List of all localities
        /// </summary>
        private static Dictionary<string, Locality> _localities;

        /// <summary>
        /// Map of localities and corresponding background sounds
        /// </summary>
        private static Dictionary<string, string> _localityLoops = new Dictionary<string, string>()
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
        public static TileMap Map { get; set; }

        /// <summary>
        /// Reference to the Detective Chipotle NPC (the main NPC)
        /// </summary>
        public static Entity Player { get; private set; }

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
        /// Returns an NPC found by its name.
        /// </summary>
        /// <param name="name">Inner name of the required NPC</param>
        /// <returns>The found NPC or null if nothing was found</returns>
        public static Entity GetEntity(string name)
=> _entities.TryGetValue(name, out Entity e) ? e : null;

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
        /// Enumerates all localities sorted by distance from the specified point.
        /// </summary>
        /// <param name="point">The point whose surroundings should be explored</param>
        /// <returns>Enumeration of the found localities</returns>
        public static IEnumerable<Locality> GetNearestLocalities(Vector2 point)
           => _localities.OrderBy(p => p.Value.Area.GetDistanceFrom(point)).Where(p => p.Value != Map[point]?.Locality).Select(p => p.Value);


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
            => _objects.OrderBy(o => o.Value.Area.GetDistanceFrom(point)).Where(o => o.Value != Map[point]?.Object).Select(o => o.Value);

        /// <summary>
        /// Enumerates all game objects around a tile sorted by distance.
        /// </summary>
        /// <param name="tile">A tile whose surrounding is to be explored</param>
        /// <returns>Enumeration of game objects</returns>
        public static IEnumerable<GameObject> GetNearestObjects(Tile tile)
            => GetNearestObjects(tile.Position);

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
                   => _passages.OrderBy(p => p.Value.Area.GetDistanceFrom(point)).Where(p => p.Value != Map[point]?.Passage).Select(p => p.Value);

        /// <summary>
        /// searches for a simple game object by name.
        /// </summary>
        /// <param name="name">Inner name of the required object</param>
        /// <returns>The found game object or null if nothing was found</returns>
        public static DumpObject GetObject(string name)
=> _objects.TryGetValue(name, out DumpObject o) ? o : null;

        /// <summary>
        /// Enumerates all simple game objects of the specified type
        /// </summary>
        /// <param name="type">Type of requested game objects</param>
        /// <returns>Enumeration of game objects</returns>
        public static IEnumerable<DumpObject> GetObjectsByType(string type)
            => _objects.Values.Where(o => !string.IsNullOrEmpty(o.Type) && o.Type.ToLower(CultureInfo.CurrentCulture) == type.ToLower(CultureInfo.CurrentCulture));

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
        /// Prepares the game world.
        /// </summary>
        public static void Initialize()
        {
            _objects = new Dictionary<string, DumpObject>();
            _localities = new Dictionary<string, Locality>();
            _entities = new Dictionary<string, Entity>();
            Player = null;
            _passages = new Dictionary<string, Passage>();
        }


        /// <summary>
        /// Loads the map from file.
        /// </summary>
        public static void LoadMap()
        {
            string Attribute(XElement element, string attribute, bool prepareForIndexing = true)
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
                string lIndexedName = Attribute(l, "indexedname");
                string lLoop = null;
                _localityLoops.TryGetValue(lIndexedName.ToLower(), out lLoop);

                Locality locality = new Locality(
               new Name(lIndexedName, Attribute(l, "friendlyname")),
               Attribute(l, "type").ToLocalityType(),
               int.Parse(Attribute(l, "height")),
               new Plane(Attribute(l, "coordinates")),
               Attribute(l, "defaultTerrain", false).ToTerrainType(),
lLoop);

                // Create perimeter walls if they are specified in the map
                string wallDefinition = Attribute(l, "walls", false);
                if (wallDefinition != "None")
                {
                    locality.DrawWalls(wallDefinition);
                }

                // Draw terrain
                l.Elements("panel").Foreach(p => Map.DrawTerrain(p, locality));

                // Load game objects
                foreach (XElement o in l.Elements("object"))
                {
                    Name oName = new Name(Attribute(o, "indexedname"), Attribute(o, "friendlyname"));
                    string oType = Attribute(o, "type");
                    Plane coordinates = new Plane(Attribute(o, "coordinates")).ToAbsolute(locality);
                    Add(GameObject.CreateObject(oName, coordinates, oType));
                }

                // register locality
                Add(locality);
            }

            // Place passages
            foreach (XElement p in xPassages)
            {
                Name pIndexedName = new Name(Attribute(p, "indexedname"));
                bool isDoor = Attribute(p, "door").ToBool();
                bool closed = Attribute(p, "closed").ToBool();
                bool openable = Attribute(p, "openable").ToBool();
                Plane area = new Plane(Attribute(p, "coordinates"));
                List<Locality> localities = new List<Locality> { GetLocality(Attribute(p, "from")), GetLocality(Attribute(p, "to").PrepareForIndexing()) };

                // Create and register new passage
                Add(Passage.CreatePassage(pIndexedName, area, localities, isDoor, closed, openable));
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
            Sound.StopAll();
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
            LoadMap();
            _localities.Foreach(p => p.Value.Start());
            _passages.Foreach(p => p.Value.Start());
            _objects.Foreach(p => p.Value.Start());
            Player = Entity.CreateChipotle();
            Add(Player);
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
                    Sound.GetStaticInfo(_cutsceneBegan.SoundID, out int _, out int totalSamples, out int __);
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