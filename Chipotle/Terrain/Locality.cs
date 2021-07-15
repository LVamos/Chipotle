using System.Text.RegularExpressions;
using Luky;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Game.Entities;
using System.Collections.ObjectModel;

namespace Game.Terrain
{
/// <summary>
    /// Defines a locality object e.g. a room or some place outside like meadow, park or yard.
    /// </summary>
  public     class Locality: MapElement
    {


        private int _ceiling;

        /// <summary>
        /// Height of ceiling (is 0 in case of outdoor localities)
        /// </summary>
        public int Ceiling
		{
            get => _ceiling;
            set=> _ceiling = _editMode ? value : throw new InvalidOperationException("Forbidden in game mode");
        }


        private const int MinimumHeight = 3;
        private const int MinimumWidth = 3;


		protected override void Appear()
		{
            foreach(var point in _area.GetPoints())
                World.Map[point] = World.Map[point] == null ? new Tile(DefaultTerrain, point, this) : throw new InvalidOperationException("Tile must be empty");
		}

        protected override void Disappear()
		{
            if(_objects.IsNotEmpty())
                new List<GameObject>(_objects).ForEach(o => World.Remove(o));

            if (_passages.IsNotEmpty())
                new List<Passage>(_passages).ForEach(p => World.Remove(p));

            if (_entities.IsNotEmpty())
                new List<Entity>(_entities).ForEach(e => World.Remove(e));

            _area.GetPoints().Foreach(p => World.Map[p] = null);
		}

		public override void ReceiveMessage(Message message)
		{
			base.ReceiveMessage(message);

            MessageObjects(message);
            MessageEntities(message);

		}

        private void MessageEntities(Message message)
=> _entities.ForEach(e => e.ReceiveMessage(message));

        protected void MessageObjects(Message message)
=> _objects.ForEach(o => o.ReceiveMessage(message));

		/// <summary>
		/// Checks if a game object is present in this locality in the moment.
		/// </summary>
		/// <param name="o">The object to be checked</param>
		/// <returns>True if the object is here</returns>
		public bool IsItHere(GameObject o) => _objects.Contains(o);

        /// <summary>
        /// Checks if an entity is present in this locality in the moment.
        /// </summary>
        /// <param name="e">The entity to be checked</param>
        /// <returns>True if the entity is here</returns>
        public bool IsItHere(Entity e) => _entities.Contains(e);

        public bool IsItHere(Passage p) => _passages.Contains(p);

        public void Register(Passage p)
		{
            // Check if exit isn't already in list
            Assert(!IsItHere(p), "exit already registered");
            _passages.Add(p);

            if (_messagingEnabled && !_editMode)
                ReceiveMessage(new PassageAppearedMessage(this, p));
        }

        /// <summary>
        /// Adds a game object to list of present objects.
        /// </summary>
        /// <param name="o">The object ot be added</param>
        public void Register(GameObject o)
        {
            Assert(!IsItHere(o), "Object already registered.");
            
            _objects.Add(o);
            if (_messagingEnabled && !_editMode)

            if (_messagingEnabled && !_editMode)
                    ReceiveMessage(new ObjectAppearedMessage(this, o));
        }

        /// <summary>
        /// Adds an entity to list of present entities.
        /// </summary>
        /// <param name="e">The entity ot be added</param>
        public void Register(Entity e)
        {
            Assert(!IsItHere(e), "Entity already registered.");
            _objects.Add(e);

            if (_messagingEnabled && !_editMode)
                ReceiveMessage(new EntityAppeared(this, e));
        }

        /// <summary>
        /// Immediately removes a game object from list of present objects.
        /// </summary>
        /// <param name="o"></param>
        public void Unregister(GameObject o)
		{
_objects.Remove(o);
            ReceiveMessage(new ObjectDisappeared(this, o));
}

        /// <summary>
        /// Immediately removes an entity from list of present entities.
        /// </summary>
        /// <param name="e">The entity to be removed</param>
        public void Unregister(Entity e)
		{
_entities.Remove(e);
            ReceiveMessage(new EntityDisappearedMessage(this, e));
		}

        public void Unregister(Passage p)
		{
            _passages.Remove(p);
            ReceiveMessage(new PassageDisappearedMessage(this, p));
		}


        private List<GameObject> _objects;

		public ReadOnlyCollection<GameObject> Objects { get; }

		private List<Entity> _entities;

		public ReadOnlyCollection<Entity> Entities { get; }









        /// <summary>
        /// All exits from the locality
        /// </summary>
        public   readonly IReadOnlyList<Passage> Passages;
		protected readonly string _backgroundSound;

		/// <summary>
		///  Defines the lowest layer of terrain
		/// </summary>
		public TerrainType DefaultTerrain { get; }





        /// <summary>
        /// All neighbour localities in given direction set.
        /// </summary>
        /// <param name="directions">wanted directions</param>
        /// <returns>locality list</returns>
        public IEnumerable<Locality> NeighbourLocalities(Directions directions)
        {
            //todo Location.Neighbours
            throw new NotImplementedException();
        }



        /// <summary>
        /// Checks if 
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        protected Locality NeighbourLocality(Direction direction)
        {
            //todo NeighbourLocality
            throw new NotImplementedException();
        }


        /// <summary>
        ///  Specifies if the locality is a room or an outdoor place
        /// </summary>
        public LocalityType Type 
        { 
            get=>_type; 
             set=> _type = _editMode ? value : throw new InvalidOperationException("Forbidden in game mode"); 
        }

        private LocalityType _type;

        private List<Passage> _passages;
		protected int _backgroundSoundId;

		/// <summary>
		///  Draws walls around the locality
		/// </summary>
		/// <param name="walls">Specifies which walls should be drawn</param>
		public void DrawWalls(string walls)
        {
            IEnumerable<Vector2> wallCoordinates;

            if (walls == "All")
                wallCoordinates = Area.GetPerimeterPoints();
            else
            {
                List<Direction> directions=new List<Direction>();

                foreach (var word in    Regex.Split(walls, @", +"))
                {
                    switch (word.ToLower())
                    {
                        case "left": directions.Add(Direction.Left); break;
                        case "front": directions.Add(Direction.Up); break;
                        case "right": directions.Add(Direction.Right);    break;
                        case "back": directions.Add(Direction.Down); break;
                        default: throw new ArgumentException(nameof(word)); break;
                    }
                }

                wallCoordinates = Area.GetPerimeterPoints(directions);
            }

            wallCoordinates.Foreach(c=> World.Map[c].Register(TerrainType.Wall, false));
        }

        public override void Start()
        {
            base.Start();

            RegisterMessageHandlers(new Dictionary<Type, Action<Message>>() 
            { 
                [typeof(LocalityEntered)] = (m) => OnLocalityEntered((LocalityEntered)m), 
                [typeof(LocalityLeft)] = (m) => OnLocalityLeft((LocalityLeft)m)
            });
        }

		private void OnLocalityLeft(LocalityLeft m)
		{
            if (_backgroundSoundId > 0)
                World.Sound.Stop(_backgroundSoundId);
		}

		private void OnLocalityEntered(LocalityEntered m)
        {
            if(m.Entity is Chipotle && !string.IsNullOrEmpty(_backgroundSound))
                _backgroundSoundId = World.Sound.Play(_backgroundSound, null, true, PositionType.None, Area.Center, true);
        }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">Name of the locality</param>
		/// <param name="type">Is it a rom or an outdoor locality?</param>
		/// <param name="defaultTerrain">Lowest layer of the terrain</param>
		/// <param name="ceiling">Ceiling height (should be 0 for outdoor localities)</param>
		/// <param name="area">Area occupied with the locality</param>
		public Locality(Name name, LocalityType type,  int ceiling,  Plane area, TerrainType defaultTerrain, string backgroundSound=null, bool editMode=false):base(name, area, editMode)
        {
            _type= type;
            _ceiling= Type == LocalityType.Outdoor && ceiling <= 2 ? 0 : ceiling;
            _ceiling     = type == LocalityType.Outdoor ? 0 : ceiling;
            _passages = new List<Passage>();
            Passages = _passages.AsReadOnly();
            _entities = new List<Entity>();
            Entities = _entities.AsReadOnly();
            _objects = new List<GameObject>();
            Objects = _objects.AsReadOnly();
            DefaultTerrain = defaultTerrain;
            _area.MinimumHeight = MinimumHeight;
            _area.MinimumWidth = MinimumWidth;
            _backgroundSound = backgroundSound;

            Appear();
        }


    }
}
