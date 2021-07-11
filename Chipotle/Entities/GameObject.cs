using Luky;
using Game.Terrain;
using System;

namespace Game.Entities
{


	/// <summary>
	/// Defines a simple subject in game world e.g. a table, chair or bed.
	/// </summary>
	public    class GameObject: MapElement
    {
		public override void Destroy()
		{
			base.Destroy();
            Locality.Unregister(this);
        }

        public static GameObject CreateObject(Name name, Plane area, string type)
        {
            if (string.IsNullOrEmpty(name.Indexed))
                throw new ArgumentException(nameof(name));

            switch(type)
            {
                case "columbovo auto": return new DumpObject(name, area, type, null, null, "carloop");
                default: return new DumpObject(name, area);
            }
        }

		//todo Gameobject: předělat na abstraktní třídu






        protected  override void Disappear()
		{
            base.Disappear();
            _area.GetTiles().Foreach(t => t.UnregisterObject());
		}

        //todo GameObject






        /// <summary>
        /// Locality which contains this object
        /// </summary>
        public Locality Locality { get; private set; }




/// <summary>
/// Constructor
/// </summary>
/// <param name="name">Name of new object</param>
/// <param name="area">Area to be occupied with new object</param>
       public GameObject(Name name, string type, Plane area, bool editMode=false): base(name, area, editMode)
        {
            _type = type.PrepareForIndexing();
            
            if(_area!=null)
            Appear();
            Locality?.Register(this);
        }

public void Move(Plane targetArea)
		{
            Assert(_editMode, "Allowed only in edit mode");
            if (targetArea == null)
                throw new ArgumentNullException(nameof(targetArea));

            Disappear();
            _area = targetArea;
            Appear();
		}



        /// <summary>
        /// draws this object to map and pairs it with locality.
        /// </summary>
        protected override void Appear()
        {
            // Check if all the object lays in one locality
            Locality = _area.GetLocality();
            Assert(Locality!= null, "Game object must occupy just one locality.");

            _area.GetTiles().Foreach(t => t.Register(this));
        }

        public override string ToString()
            => Name.Friendly;
        protected string _type;
        public string Type
		{
            get => _type;
            set => _type= _editMode ? value : throw new InvalidOperationException("Forbidden in game mode");
        }

        public override int GetHashCode()
            => unchecked(3000 * (2000 + _area.GetHashCode()) * (3000 + _type.GetHashCode()) * (4000 + Locality.GetHashCode()));

    }
}
