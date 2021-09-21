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
        /// <summary>
        /// Type of the object; it allows grouping objects with tha same behavior.
        /// </summary>
        public readonly string Type;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="type">Type of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
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

        /// <summary>
        /// Creates new instance of the pool (bazének w1) object in the Walsch's bathroom (koupelna
        /// w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateBathroomPool(Name name, Plane area)
            => new DumpObject(name, area, "bazének", null, "snd7");

        /// <summary>
        /// Creates new instance of the sink (umyvadlo w1) object in the Walsch's bathroom (koupelna
        /// w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateBathroomSink(Name name, Plane area)
                        => new DumpObject(name, area, "umyvadlo", null, "snd8");

        /// <summary>
        /// Creates new instance of the bench (lavička c1) object in the Carson's garden (zahrada
        /// c1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateCarsonsBench(Name name, Plane area)
                    => new CarsonsBench(name, area);

        /// <summary>
        /// Creates new instance of the coffee maker (kávovar) object.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateCoffeemaker(Name name, Plane area)
            => new DumpObject(name, area, "kávovar", null, "snd9");

        /// <summary>
        /// Creates new instance of the corpse (tělo w1) object in the poolside (bazén w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateCorpse(Name name, Plane area)
            => new Corpse(name, area);

        /// <summary>
        /// Creates new instance of the electrical box (rozvodna w1) object in the basement (sklep
        /// w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateElectricalBox(Name name, Plane area)
=> new DumpObject(name, area, "rozvodna", null, null, "ElectricalBoxLoop");

        /// <summary>
        /// Creates new instance of the fish tank (akvárko w1) object in the basement (sklep w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateFishTank(Name name, Plane area)
=> new DumpObject(name, area, "akvárko", null, null, "FishTankLoop");

        /// <summary>
        /// Creates new instance of the garden grill (gril c1) object in the Sweeney's garden
        /// (zahrada c1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateGardenGril(Name name, Plane area)
        => new CarsonsGrill(name, area);

        /// <summary>
        /// Creates new instance of the garden hose (hadice c1) object in the Sweeney's
        /// garden(zahrda c1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateGardenHose(Name name, Plane area)
        => new DumpObject(name, area, "zahradní hadice", null, "snd1", null);

        /// <summary>
        /// Creates new instance of a high way (dálnice w1) object.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateHighway(Name name, Plane area)
=> new DumpObject(name, area, "dálnice", null, null, "HighwayLoop");

        /// <summary>
        /// Creates new instance of the Detective Chipotle's car (detektivovo auto) object in the
        /// Walsch's drive way (příjezdová cesta w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
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

        /// <summary>
        /// Creates new instance of the bench (lavička w1) object in the Walsch's pool (bazén w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreatePoolsideBench(Name name, Plane area)
                    => new DumpObject(name, area, "lavička u bazénu", null, null, null, "cs1", true);

        /// <summary>
        /// Creates new instance of the bin (popelnice w1) object in the Walsch's pool (bazén w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreatePoolsideBin(Name name, Plane area)
        => new PoolsideBin(name, area);

        /// <summary>
        /// Creates new instance of the plank (prkno w1) object in the Walsch's pool (bazén w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreatePoolsidePlank(Name name, Plane area)
                    => new DumpObject(name, area, "prkno u bazénu", null, null, null, "cs4", true);

        /// <summary>
        /// Creates new instance of the pool steps (schůdky w1) object in the Walsch's pool (bazén
        /// w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreatePoolStairs(Name name, Plane area)
        => new DumpObject(name, area, "schůdky u bazénu", null, "snd3");

        /// <summary>
        /// Returns the hash code for this object.
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
                    => unchecked(3000 * (2000 + Area.GetHashCode()) * (3000 + Type.GetHashCode()) * (4000 + Locality.GetHashCode()));

        /// <summary>
        /// Returns public name of the object.
        /// </summary>
        /// <returns>public name of the object</returns>
        public override string ToString()
                    => Name.Friendly;

        /// <summary>
        /// Displays the object in the game world and assigns it to the appropriate locality.
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

        /// <summary>
        /// Erases the object or NPC from the game world.
        /// </summary>
        protected override void Disappear()
        {
            base.Disappear();
            _area.GetTiles().Foreach(t => t.UnregisterObject());
        }

        /// <summary>
        /// Moves the object to the specified coordinates.
        /// </summary>
        /// <param name="targetArea">Target coordinates</param>
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

        /// <summary>
        /// Creates new instance of the cuckoo clock (kukačkové hodiny w1) object in the Walsch's
        /// dining room (jídelna w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateCuckooClock(Name name, Plane area)
        => new DumpObject(name, area, "kukačkové hodiny", null, null, "CuckooClockLoop");

        /// <summary>
        /// Creates new instance of the dish washer (myčka) object.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateDishwasher(Name name, Plane area)
        => new DumpObject(name, area, "", null, "snd11");

        /// <summary>
        /// Creates new instance of the fan (větrák p1) object in the Christine's bed room (ložnice
        /// p1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateFan(Name name, Plane area)
        => new DumpObject(name, area, "větrák", null, null, "FanLoop");

        /// <summary>
        /// Creates new instance of the fire place (krb) object.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateFireplace(Name name, Plane area)
        => new DumpObject(name, area, "krb", null, null, "FirePlaceLoop");

        /// <summary>
        /// Creates new instance of the freezer (mrazák v1) object in the Mariotti!s office
        /// (kancelář v1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateFreezer(Name name, Plane area)
        => new DumpObject(name, area, "mrazák", null, null, "FreezerLoop");

        /// <summary>
        /// Creates new instance of the fridge (lednice) object.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateFridge(Name name, Plane area)
        => new DumpObject(name, area, "lednice", null, "snd10", "FridgeLoop");

        /// <summary>
        /// Creates new instance of the chair (židle) object.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateChair(Name name, Plane area)
        => new DumpObject(name, area, "židle", null, null, null, "snd12");

        /// <summary>
        /// Creates new instance of the Christine's door bell object in the Belvedere street (ulice
        /// p1) locality.
        /// </summary>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateChristinesBell(Name name, Plane area)
            => new ChristinesBell(name, area);

        /// <summary>
        /// Creates new instance of the mirror (zrcadlo p1) object in the Christine's bed room
        /// (ložnice p1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateChristinesMirror(Name name, Plane area)
                    => new DumpObject(name, area, "zrcadlo u Kristýny", null, null, null, "cs14", true);

        /// <summary>
        /// Creates new instance of the icecream machine (automat v1) object in the hall of Vanilla
        /// crunch company (hala v1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateIcecreamMachine(Name name, Plane area)
                    => new IcecreamMachine(name, area);

        /// <summary>
        /// Creates new instance of the key hanger (věšák v1) object in the garage of Vanilla crunch
        /// company (garáž v1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateKeyHanger(Name name, Plane area) => new KeyHanger(name, area);

        /// <summary>
        /// Creates new instance of the Killer's car (zabijákovo auto v1) object in the garage of
        /// Vanilla crunch company (garáž v1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static KillersCar CreateKillersCar(Name name, Plane area) => new KillersCar(name, area);

        /// <summary>
        /// Creates new instance of the Mariotti's chair (křeslo v1) object in the kancelář v1 locality.
        /// </summary>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateMariottisChair(Name name, Plane area)
            => new DumpObject(name, area, "křeslo u Mariottiho", null, null, null, "cs12", true);

        /// <summary>
        /// Creates new instance of the pinball table (pinball h1) object in the pub (výčep h1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreatePinball(Name name, Plane area)
            => new DumpObject(name, area, "pinball", null, "snd15");

        /// <summary>
        /// Creates new instance of the table (stůl) object in the pub (výčep h1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static PubTable CreatePubTable(Name name, Plane area)
                        => new PubTable(name, area);

        /// <summary>
        /// Creates new instance of the safe (trezor s1) object in the Sweeney's bed room (pokoj s1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateSafe(Name name, Plane area)
                        => new DumpObject(name, area, "trezor", null, null, null, "cs18", true);

        /// <summary>
        /// Creates new instance of the Sweeney's door bell object in the Easterby street (ulice s1) locality.
        /// </summary>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateSweeneysBell(Name name, Plane area)
            => new SweeneysBell(name, area);

        /// <summary>
        /// Creates new instance of the computer (počítač s1) object in the Sweeney's bed room
        /// (pokoj s1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateSweeneysComputer(Name name, Plane area)
                => new DumpObject(name, area, "počítač u sweeneyho", null, null, "ComputerLoop", "cs16", true);

        /// <summary>
        /// Creates new instance of the cell phone (mobil s1) object in the Sweeney's bed room
        /// (pokoj s1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateSweeneysPhone(Name name, Plane area)
                => new DumpObject(name, area, "mobil u Sweeneyho", null, null, null, "cs15");

        /// <summary>
        /// Creates new instance of the table (stůl) object in the Sweeney's bed room (pokoj s1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateSweeneysTable(Name name, Plane area)
                                => new DumpObject(name, area, "stůl u sweeneyho", null, null, null, "cs17", true);

        /// <summary>
        /// Creates new instance of the car (auto) object in the garage of Vanilla crunch company
        /// (garáž v1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateVanillaCrunchCar(Name name, Plane area)
=> new VanillaCrunchCar(name, area);

        /// <summary>
        /// Creates new instance of the wall clock (hodiny) object.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateWallClock(Name name, Plane area)
        => new DumpObject(name, area, "hodiny", null, null, "WallClockLoop");
    }
}