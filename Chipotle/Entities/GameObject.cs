using System;

using Game.Terrain;

using Luky;

namespace Game.Entities
{
    /// <summary>
    /// A base class for all simple objects and NPCS
    /// </summary>
    public class GameObject : MapElement
    {
        public readonly string Type;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Name of new object</param>
        /// <param name="area">Area to be occupied with new object</param>
        public GameObject(Name name, string type, Plane area) : base(name, area)
        {
            Type = type.PrepareForIndexing();

            if (area != null)
                Appear();

            Locality?.Register(this);
        }

        /// <summary>
        /// Locality which contains this object
        /// </summary>
        public Locality Locality { get; private set; }

        public static DumpObject CreateCorpse(Name name, Plane area)
            => new Corpse(name, area);

        /// <summary> Creates new instance of the electrical box (rozvodná skříň w1) object in the
        /// sklep w1 locality. </summary> <returns><New instance of the object/returns>
        public static DumpObject CreateElectricalBox(Name name, Plane area)
=> new DumpObject(name, area, "rozvodna", null, null, "ElectricalBoxLoop");

        /// <summary> Creates new instance of the fish tank (akvárko w1) object in the ložnice w1
        /// locality. </summary> <returns><New instance of the object/returns>
        public static DumpObject CreateFishTank(Name name, Plane area)
=> new DumpObject(name, area, "akvárko", null, null, "FishTankLoop");

        public static DumpObject CreateGardenGril(Name name, Plane area)
        => new CarsonsGrill(name, area);

        public static DumpObject CreateGardenHose(Name name, Plane area)
        => new DumpObject(name, area, "zahradní hadice", null, "snd1", null);

        /// <summary> Creates new instance of a highway (dálnice) object. </summary> <returns><New
        /// instance of the object/returns>
        public static DumpObject CreateHighway(Name name, Plane area)
=> new DumpObject(name, area, "dálnice", null, null, "HighwayLoop");

        public static ChipotlesCar CreateChipotlesCar(Name name, Plane area)
            => new ChipotlesCar(name, area);

        /// <summary>
        /// Creates new instance of an object according to the specified type.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <param name="type">Type of the object</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateObject(Name name, Plane area, string type)
        {
            if (string.IsNullOrEmpty(name.Indexed))
                throw new ArgumentException(nameof(name));

            switch (type)
            {
                case "dálnice": return CreateHighway(name, area);
                case "rozvodna": return CreateElectricalBox(name, area);
                case "větrák": return CreateFan(name, area);
                case "krb": return CreateFireplace(name, area);
                case "mrazák": return CreateFreezer(name, area);
                case "akvárko": return CreateFishTank(name, area);
                case "hodiny": return CreateWallClock(name, area);
                case "kukačkové hodiny": return CreateCuckooClock(name, area);
                case "křeslo u mariottiho": return CreateMariottisChair(name, area);
                case "sweeneyho zvonek": return CreateSweeneysBell(name, area);
                case "christinin zvonek": return CreateChristinesBell(name, area);
                case "zrcadlo u kristýny": return CreateChristinesMirror(name, area);
                case "vražedné auto": return CreateKillersCar(name, area);
                case "věšák na klíče": return CreateKeyHanger(name, area);
                case "hospodský stůl": return CreatePubTable(name, area);
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

        public static DumpObject CreatePoolsideBench(Name name, Plane area)
                    => new DumpObject(name, area, "lavička u bazénu", null, null, null, "cs1", true);

        public static DumpObject CreatePoolsideBin(Name name, Plane area)
        => new PoolsideBin(name, area);

        public static DumpObject CreatePoolsidePlank(Name name, Plane area)
                    => new DumpObject(name, area, "prkno u bazénu", null, null, null, "cs4", true);

        public static DumpObject CreatePoolStairs(Name name, Plane area)
        => new DumpObject(name, area, "schůdky u bazénu", null, "snd3");

        public override int GetHashCode()
                    => unchecked(3000 * (2000 + Area.GetHashCode()) * (3000 + Type.GetHashCode()) * (4000 + Locality.GetHashCode()));

        public override string ToString()
                    => Name.Friendly;

        /// <summary>
        /// draws this object to map and pairs it with locality.
        /// </summary>
        protected override void Appear()
        {
            base.Appear();

            // Check if all the object lays in one locality
            Locality = Area.GetLocality();
            Assert(Locality != null, "Game object must occupy just one locality.");

            Area.GetTiles().Foreach(t => t.Register(this));
        }

        /// <summary>
        /// Destroys the object or NPC.
        /// </summary>
        protected override void Destroy()
        {
            Locality?.Unregister(this);
            World.Remove(this);
            Disappear();
        }

        protected override void Disappear()
        {
            base.Disappear();
            _area.GetTiles().Foreach(t => t.UnregisterObject());
        }

        //todo GameObject
        protected virtual void Move(Plane targetArea)
        {
            if (targetArea == null)
                throw new ArgumentNullException(nameof(targetArea));

            Locality sourceLocality = Locality;
            Locality targetLocality = targetArea.GetLocality();

            Disappear();
            _area = targetArea;
            Appear();

            if (sourceLocality != targetLocality)
            {
                sourceLocality.Unregister(this);
                targetLocality.Register(this);
                Locality = targetLocality;
            }
        }

        private static DumpObject CreateBathroomPool(Name name, Plane area)
            => new DumpObject(name, area, "bazének", null, "snd7");

        private static DumpObject CreateBathroomSink(Name name, Plane area)
                        => new DumpObject(name, area, "umyvadlo", null, "snd8");

        private static DumpObject CreateCarsonsBench(Name name, Plane area)
                    => new CarsonsBench(name, area);

        private static DumpObject CreateCoffeemaker(Name name, Plane area)
            => new DumpObject(name, area, "kávovar", null, "snd9");

        private static DumpObject CreateCuckooClock(Name name, Plane area)
        => new DumpObject(name, area, "kukačkové hodiny", null, null, "CuckooClockLoop");

        private static DumpObject CreateDishwasher(Name name, Plane area)
        => new DumpObject(name, area, "", null, "snd11");

        private static DumpObject CreateFan(Name name, Plane area)
        => new DumpObject(name, area, "větrák", null, null, "FanLoop");

        private static DumpObject CreateFireplace(Name name, Plane area)
        => new DumpObject(name, area, "krb", null, null, "FirePlaceLoop");

        private static DumpObject CreateFreezer(Name name, Plane area)
        => new DumpObject(name, area, "mrazák", null, null, "FreezerLoop");

        private static DumpObject CreateFridge(Name name, Plane area)
        => new DumpObject(name, area, "lednice", null, "snd10", "FridgeLoop");

        private static DumpObject CreateChair(Name name, Plane area)
        => new DumpObject(name, area, "židle", null, null, null, "snd12");

        /// <summary> Creates new instance of the Christine's door bell object in the Belvedere
        /// street (ulice p1) locality. </summary> <returns><New instance of the object/returns>
        private static DumpObject CreateChristinesBell(Name name, Plane area)
            => new ChristinesBell(name, area);

        private static DumpObject CreateChristinesMirror(Name name, Plane area)
                    => new DumpObject(name, area, "zrcadlo u Kristýny", null, null, null, "cs14", true);

        private static DumpObject CreateIcecreamMachine(Name name, Plane area)
                    => new IcecreamMachine(name, area);

        private static DumpObject CreateKeyHanger(Name name, Plane area) => new KeyHanger(name, area);

        private static KillersCar CreateKillersCar(Name name, Plane area) => new KillersCar(name, area);

        /// <summary> Creates new instance of the Mariotti's chair (křeslo v1) object in the
        /// kancelář v1 locality. </summary> <returns><New instance of the object/returns>
        private static DumpObject CreateMariottisChair(Name name, Plane area)
            => new DumpObject(name, area, "křeslo u Mariottiho", null, null, null, "cs12", true);

        private static DumpObject CreatePinball(Name name, Plane area)
            => new DumpObject(name, area, "pinball", null, "snd15");

        private static PubTable CreatePubTable(Name name, Plane area)
                        => new PubTable(name, area);

        private static DumpObject CreateSafe(Name name, Plane area)
                        => new DumpObject(name, area, "trezor", null, null, null, "cs18", true);

        /// <summary> Creates new instance of the Sweeney's door bell object in the Easterby street
        /// (ulice s1) locality. </summary> <returns><New instance of the object/returns>
        private static DumpObject CreateSweeneysBell(Name name, Plane area)
            => new SweeneysBell(name, area);

        private static DumpObject CreateSweeneysComputer(Name name, Plane area)
                => new DumpObject(name, area, "počítač u sweeneyho", null, null, "ComputerLoop", "cs16", true);

        private static DumpObject CreateSweeneysPhone(Name name, Plane area)
                => new DumpObject(name, area, "mobil u Sweeneyho", null, null, null, "cs15");

        private static DumpObject CreateSweeneysTable(Name name, Plane area)
                                => new DumpObject(name, area, "stůl u sweeneyho", null, null, null, "cs17", true);

        private static DumpObject CreateVanillaCrunchCar(Name name, Plane area)
=> new VanillaCrunchCar(name, area);

        private static DumpObject CreateWallClock(Name name, Plane area)
        => new DumpObject(name, area, "hodiny", null, null, "WallClockLoop");
    }
}