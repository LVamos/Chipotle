using Game.Entities;
using Game.Messaging;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Game
{
    /// <summary>
    /// 
    /// </summary>
    public static class World
    {
        public const int FramesPerSecond = 66;
        public const int DeltaTime = 1000 / FramesPerSecond;

        /// <summary>
        /// Enumerates all game objects around a point sorted by distance.
        /// </summary>
        /// <param name="point">A point on the map whose surroundings are to be explored</param>
        /// <returns>Enumeration of game objects</returns>
        public static IEnumerable<GameObject> GetNearestObjects(Vector2 point)
            => _objects.OrderBy(o => o.Value.Area.GetDistanceFrom(point)).Where(o => o.Value != Map[point]?.Object).Select(o => o.Value);

        /// <summary>
        /// Enumerates all localities sroted by distance from default point.
        /// </summary>
        /// <param name="point">Coordinates of the point whose surroundings should be explored</param>
        /// <returns>Enumeration of localities</returns>
        public static IEnumerable<Locality> GetNearestLocalities(Vector2 point)
           => _localities.OrderBy(p => p.Value.Area.GetDistanceFrom(point)).Where(p => p.Value != Map[point]?.Locality).Select(p => p.Value);


        public static Locality GetNearestLocality(Vector2 point)
            => GetNearestLocalities(point).First();

        /// <summary>
        /// Returns one game object closest to given point.
        /// </summary>
        /// <param name="defaultPoint">Coordinates of the defualt point</param>
        /// <returns>Game object</returns>
        public static GameObject GetNearestObject(Vector2 defaultPoint)
            => GetNearestObjects(defaultPoint).FirstOrDefault();

        /// <summary>
        /// Enumerates all game objects around a point sorted by distance.
        /// </summary>
        /// <param name="point">A point on the map whose surroundings are to be explored</param>
        /// <returns>Enumeration of game objects</returns>
        public static IEnumerable<GameObject> GetNearestObjects(Tile point)
            => GetNearestObjects(point.Position);


        /// <summary>
        /// 
        /// </summary>
        public static void Update()
        {
            _localities.Foreach(v => v.Value.Update());
            _passages.Foreach(v => v.Value.Update());
            _objects.Foreach(v => v.Value.Update());
            _entities.Foreach(v => v.Value.Update());
            HandleCutscene();
        }

        private static void HandleCutscene()
        {
            if (_cutsceneBegan!=null)
            {
                Sound.GetDynamicInfo(_cutsceneBegan.SoundID, out SoundState state, out int _);

                if (state != SoundState.Playing)
                {
                    ReceiveMessage(new CutsceneEnded(_cutsceneBegan.Sender, _cutsceneBegan.CutsceneName, _cutsceneBegan.SoundID));
                    _cutsceneBegan = null;
                }
            }
        }

        public static void ReceiveMessage(GameMessage message)
            => _entities.Values.Foreach(e => e.ReceiveMessage(message));

        public static void PlayCutscene(object sender, string cutscene)
        {
            if (string.IsNullOrEmpty(cutscene))
            {
                throw new ArgumentNullException(nameof(cutscene));
            }

             int id = Sound.Play(cutscene);
            _cutsceneBegan = new CutsceneBegan(sender, cutscene, id);
            ReceiveMessage(_cutsceneBegan);
        }

        public static TileMap Map { get; set; }

        private static Dictionary<string, DumpObject> _objects;
        private static Dictionary<string, Entity> _entities;
        private static Dictionary<string, Passage> _passages;
        public static Entity Player;

        public static void StopCutscene(object sender)
        {
            Sound.Stop(_cutsceneBegan.SoundID);
            ReceiveMessage(new CutsceneEnded(_cutsceneBegan.Sender, _cutsceneBegan.CutsceneName, _cutsceneBegan.SoundID));
            _cutsceneBegan = null;
        }

        public static void RenameLocality(string indexedName, string newName)
        {
            Locality value = _localities[indexedName];
            _localities.Remove(indexedName);
            _localities[newName] = value;
        }

        public static Locality GetLocality(string name)
        {
            _localities.TryGetValue(name.PrepareForIndexing(), out Locality locality);
            return locality;
        }

        public static Passage GetPassage(string name)
        {
            _passages.TryGetValue(name, out Passage passage);
            return passage;
        }

        public static bool Passageexists(string name)
            => _passages.ContainsKey(name);


        /// <summary>
        /// Registers an entity.
        /// </summary>
        /// <param name="e">The entity to be registered</param>
        public static void Add(Entity e)
        {
            // Do a null check and look if entity isn't already registered.
            if (e == null)
            {
                throw new ArgumentNullException(nameof(e));
            }

            if (_entities.ContainsKey(e.Name.Indexed))
            {
                throw new ArgumentException("entity already registered");
            }

            _entities.Add(e.Name.Indexed, e);
        }

        /// <summary>
        /// Returns first entity of given name. It's suitable in case there's just one entity of given name.
        /// </summary>
        /// <param name="name">Name of desired entity</param>
        /// <returns>The entity or null</returns>
        public static Entity GetEntity(string name)
=> _entities.TryGetValue(name, out Entity e) ? e : null;





        /// <summary>
        /// Returns first game object of given name. It's suitable in case there's just one entity of given name.
        /// </summary>
        /// <param name="name">Name of desired object</param>
        /// <returns>The game object or null</returns>
        public static DumpObject GetObject(string name)
=> _objects.TryGetValue(name, out DumpObject o) ? o : null;






        private static Dictionary<string, Locality> _localities;
        private static XDocument _map;

        /// <summary>
        /// Registers a game object.
        /// </summary>
        /// <param name="o">The game object to be added</param>
        public static void Add(DumpObject o)
        {
            // Do a null check and look if object isn't already registered.
            if (o == null)
            {
                throw new ArgumentNullException(nameof(o));
            }

            if (_objects.ContainsKey(o.Name.Indexed))
            {
                throw new ArgumentException("Object already registered");
            }

            _objects.Add(o.Name.Indexed, o); // Added to dictionary
        }



        /// <summary>
        /// Adds a locality into list and dictionary.
        /// </summary>
        /// <param name="l">Locality instance</param>
        public static void Add(Locality l)
        {
            // null check
            if (l == null)
            {
                throw new ArgumentNullException(nameof(l));
            }

            // Isn't the locality already registered?
            if (_localities.ContainsKey(l.Name.Indexed))
            {
                throw new ArgumentException("Locality already registered");
            }

            _localities.Add(l.Name.Indexed, l);
        }


        /// <summary>
        /// Adds a passage into list
        /// </summary>
        /// <param name="p">Passage instance</param>
        public static void Add(Passage p)
        {
            if (p == null)
            {
                throw new ArgumentNullException(nameof(p));
            }

            if (_passages.ContainsKey(p.Name.Indexed))
            {
                throw new ArgumentException("Passage already registered");
            }

            _passages.Add(p.Name.Indexed, p);
        }


        /// <summary>
        /// Prepares the game world for game start.
        /// </summary>
        public static void Initialize()
        {
            _objects = new Dictionary<string, DumpObject>();
            _localities = new Dictionary<string, Locality>();
            _entities = new Dictionary<string, Entity>();
            _passages = new Dictionary<string, Passage>();
        }

        public static SoundThread Sound;
        private static CutsceneBegan _cutsceneBegan;

        /// <summary>
        /// Starts game from begining.
        /// </summary>
        public static void StartGame()
        {

            // Sound
            Sound = SoundThread.CreateAndStartThread(Program.OnError);
            Sound.LoadSounds();
            Sound.SetGroupVolume("master", 1);

            _map = LoadMap();
            _localities.Foreach(p => p.Value.Start());
            _passages.Foreach(p => p.Value.Start());
            _objects.Foreach(p => p.Value.Start());
            Player = Entity.CreateChipotle();
            Add(Player);
            Add(Entity.CreateTuttle());
            _entities.Foreach(p => p.Value.Start());
            Program.MainWindow.GameLoopEnabled = true;
        }

        public static bool LocalityExists(string name)
            => _localities.ContainsKey(name.PrepareForIndexing());




        public static XDocument LoadMap()
            => LoadMap(DebugSO.MapPath);

        /// <summary>
        /// Loads map from file
        /// </summary>
        /// <param name="fileName">Name of the map file</param>
        public static XDocument LoadMap(string fileName)
        {
            string Attribute(XElement element, string attribute, bool prepareForIndexing = true)
                => prepareForIndexing ? element.Attribute(attribute).Value.PrepareForIndexing() : element.Attribute(attribute).Value;

            XDocument xDocument = XDocument.Load(fileName);
            XElement root = xDocument.Root;
            IEnumerable<XElement> xLocalities = root.Element("localities").Elements("locality");
            IEnumerable<XElement> xPassages = root.Element("passages").Elements("passage");

            Initialize(); // Prepare data structures for objects, entities, localities etc.

            // Get map size
            IEnumerable<Plane> areas =
                from l in xLocalities
                select new Plane(Attribute(l, "coordinates"));

            int leftBorder = (int)(from a in areas select a.UpperLeftCorner.X).Min();
            int rightBorder = (int)(from a in areas select a.LowerRightCorner.X).Max();
            int topBorder = (int)(from a in areas select a.UpperLeftCorner.Y).Max();
            int bottomBorder = (int)(from a in areas select a.LowerRightCorner.Y).Min();

            // Create map
            Map = new TileMap(fileName);

            // Load localities
            foreach (XElement l in xLocalities)
            { // Create locality
                Locality locality = new Locality(
               new Name(Attribute(l, "indexedname"), Attribute(l, "friendlyname")),
               Attribute(l, "type").ToLocalityType(),
               int.Parse(Attribute(l, "height")),
               new Plane(Attribute(l, "coordinates")),
               Attribute(l, "defaultTerrain", false).ToTerrainType(),
null);

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
                Plane area = new Plane(Attribute(p, "coordinates"));
                List<Locality> localities = new List<Locality> { GetLocality(Attribute(p, "from")), GetLocality(Attribute(p, "to").PrepareForIndexing()) };

                // Create and register new passage
                Add(Passage.CreatePassage(pIndexedName, area, localities, isDoor, closed));
            }

            return xDocument;
        }

        public static IEnumerable<Passage> GetNearestPassages(Vector2 point)
           => _passages.OrderBy(p => p.Value.Area.GetDistanceFrom(point)).Where(p => p.Value != Map[point]?.Passage).Select(p => p.Value);

        public static void Remove(Locality l)
        {
            _localities.Remove(l.Name.Indexed);
            l.Destroy();
        }

        public static void Remove(Passage p)
        {
            _passages.Remove(p.Name.Indexed);
            p.Destroy();
        }

        public static void Remove(GameObject o)
        {
            _objects.Remove(o.Name.Indexed);
            o.Destroy();
        }



        public static bool ObjectExists(string indexedName)
=> _objects.ContainsKey(indexedName);

        public static IEnumerable<string> GetObjectTypes()
=> _objects.Where(p => !string.IsNullOrEmpty(p.Value.Type)).Select(p => p.Value.Type);

        public static Passage GetNearestPassage(Vector2 point)
=> GetNearestPassages(point).FirstOrDefault();

        public static void RenameObject(string indexedName, string newName)
        {
            DumpObject value = _objects[indexedName];
            _objects.Remove(indexedName);
            _objects[newName] = value;
        }

        public static void RenamePassage(string indexedName, string newName)
        {
            Passage value = _passages[indexedName];
            _passages.Remove(indexedName);
            _passages[newName] = value;
        }
    }

}
