using Game.Terrain;

using Luky;

using System;

namespace Game.Entities
{


    /// <summary>
    /// Defines a simple subject in game world e.g. a table, chair or bed.
    /// </summary>
    public class GameObject : MapElement
    {
        protected override void Destroy()
        {
            Locality?.Unregister(this);
            World.Remove(this);
            Disappear();
        }

        public static DumpObject CreateObject(Name name, Plane area, string type)
        {
            if (string.IsNullOrEmpty(name.Indexed))
            {
                throw new ArgumentException(nameof(name));
            }

            switch (type)
            {
                case "lavice u carsona": return CreateCarsonsBench(name, area);
                case "trezor": return CreateSafe(name, area);
                case "stůl u sweeneyho": return CreateSweeneysTable(name, area);
                case "počítač u sweeneyho": return CreateSweeneysComputer(name, area);
                case "mobil u sweeneyho": return CreateSweeneysPhone(name, area);
                case "auto vanilla crunch": return CreateVanillaCrunchCar(name, area);
                case "pinball": return CreatePinball(name, area);
                case "automat na zmrzlinu": return CreateIcecreamMachine(name, area);
                case "židle": return CreateChair(name, area);
                case "myčka": return CreateDishwasher(name, area);
                case "lednice": return CreateFridge(name, area);
                case "kávovar": return CreateCoffeemaker(name, area);
                case "umyvadlo": return CreateBathroomSink(name, area);
                case "bazének": return CreateBathroomPool(name, area);
                case "detektivovo auto": return CreateChipotlesCar(name, area);
                case "mrtvola": return CreateCorpse(name, area);
                case "prkno u bazénu": return CreatePoolsidePlank(name, area);
                case "popelnice u bazénu": return CreatePoolsideBin(name, area);
                case "lavička u bazénu": return CreatePoolsideBench(name, area);
                case "schůdky u bazénu": return CreatePoolStairs(name, area);
                case "gril u carsona": return CreateGardenGril(name, area);
                case "zahradní hadice": return CreateGardenHose(name, area);

                default: return new DumpObject(name, area);
            }
        }

        private static DumpObject CreateCarsonsBench(Name name, Plane area)
            => new CarsonsBench(name, area);
        private static DumpObject CreateSafe(Name name, Plane area)
                => new DumpObject(name, area, "trezor", null, null, null, "cs18", true);

        private static DumpObject CreateSweeneysTable(Name name, Plane area)
                => new DumpObject(name, area, "stůl u sweeneyho", null, null, null, "cs17", true);

        private static DumpObject CreateSweeneysComputer(Name name, Plane area)
                => new DumpObject(name, area, "počítač u sweeneyho", null, null, null, "cs16", true);

        private static DumpObject CreateSweeneysPhone(Name name, Plane area)
                => new DumpObject(name, area, "mobil u Sweeneyho", null, null, null, "cs15");

        private static DumpObject CreateVanillaCrunchCar(Name name, Plane area)
=> new DumpObject(name, area, "auto Vanilla crunch", null, "snd4");


        private static DumpObject CreatePinball(Name name, Plane area)
    => new DumpObject(name, area, "pinball", null, "snd15");


        private static DumpObject CreateIcecreamMachine(Name name, Plane area)
            => new IcecreamMachine(name, area);
        private static DumpObject CreateBathroomSink(Name name, Plane area)
                => new DumpObject(name, area, "umyvadlo", null, "snd8");

        private static DumpObject CreateFridge(Name name, Plane area)
=> new DumpObject(name, area, "lednice", null, "snd10");

        private static DumpObject CreateChair(Name name, Plane area)
=> new DumpObject(name, area, "židle", null, "snd12");



        private static DumpObject CreateDishwasher(Name name, Plane area)
=> new DumpObject(name, area, "", null, "snd11");


        private static DumpObject CreateCoffeemaker(Name name, Plane area)
    => new DumpObject(name, area, "kávovar", null, "snd9");

        private static DumpObject CreateBathroomPool(Name name, Plane area)
    => new DumpObject(name, area, "bazének", null, "snd7");


        public static DumpObject CreateCorpse(Name name, Plane area)
    => new DumpObject(name, area, "mrtvola", null, null, null, "cs5", true);


        public static DumpObject CreatePoolsidePlank(Name name, Plane area)
            => new DumpObject(name, area, "prkno u bazénu", null, null, null, "cs4", true);


        public static DumpObject CreatePoolsideBin(Name name, Plane area)
=> new PoolsideBin(name, area);

        public static DumpObject CreatePoolsideBench(Name name, Plane area)
            => new DumpObject(name, area, "lavička u bazénu", null, null, null, "cs1", true);

        public static DumpObject CreateChipotlesCar(Name name, Plane area)
    => new ChipotlesCar(name, area);


        public static DumpObject CreatePoolStairs(Name name, Plane area)
=> new DumpObject(name, area, "schůdky u bazénu", null, "snd3");
        public static DumpObject CreateGardenHose(Name name, Plane area)
=> new DumpObject(name, area, "zahradní hadice", null, "snd1", null);

        public static DumpObject CreateGardenGril(Name name, Plane area)
=> new CarsonsGrill(name, area);







        protected override void Disappear()
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
        public GameObject(Name name, string type, Plane area) : base(name, area)
        {
            Type = type.PrepareForIndexing();

            if (area != null)
            {
                Appear();
            }

            Locality?.Register(this);
        }

        protected void Move(Plane targetArea)
        {
            if (targetArea == null)
            {
                throw new ArgumentNullException(nameof(targetArea));
            }

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
            Locality = Area.GetLocality();
            Assert(Locality != null, "Game object must occupy just one locality.");

            Area.GetTiles().Foreach(t => t.Register(this));
        }

        public override string ToString()
            => Name.Friendly;

        public readonly string Type;

        public override int GetHashCode()
            => unchecked(3000 * (2000 + Area.GetHashCode()) * (3000 + Type.GetHashCode()) * (4000 + Locality.GetHashCode()));

    }

}
