using ProtoBuf;
using System;

using Game.Terrain;

using Luky;

namespace Game.Entities
{
    /// <summary>
    /// A base class for all simple objects and NPCS
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    [ProtoInclude(100, typeof(DumpObject))]
    [ProtoInclude(101, typeof(Entity))]
    public class GameObject : MapElement
    {
        /// <summary>
        /// Checks if a specified object is located in the same locality as this object.
        /// </summary>
        /// <param name="o">The object to be checked</param>
        /// <returns>True if the specified object is located in the same locality as this object.</returns>
        public bool IsInSameLocality(DumpObject o)
            => Locality.IsItHere(o);

        /// <summary>
        /// Checks if a specified NPC is located in the same locality as this object.
        /// </summary>
        /// <param name="e">The NPC to be checked</param>
        /// <returns>True if the specified NPC is located in the same locality as this object.</returns>
        public bool IsInSameLocality(Entity e)
            => Locality.IsItHere(e);

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

            if (_area!= null)
            {
                Appear();
                _locality = _area.GetLocality();
            }

            if (_locality != null)
            {
                if (this is DumpObject d)
                    _locality.Register(d);
                else _locality.Register(this as Entity);
            }
        }

        /// <summary>
        /// Locality which contains this object
        /// </summary>
[ProtoIgnore]
        public Locality Locality
        {
            get
            {
				if (_area == null)
					return null;

				if (_locality == null)
                    _locality = _area.GetLocality();
                return _locality;
            }
        }

        /// <summary>
        /// Backing field for Locality property.
        /// </summary>
[ProtoIgnore]
        protected Locality _locality;

        /// <summary>
        /// Creates new instance of the pool (bazének w1) object in the Walsch's bathroom (koupelna
        /// w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateBathroomPool(Name name, Plane area, bool decorative, bool pickable)
            => new DumpObject(name, area, "bazének", decorative, pickable: pickable, null, "snd7", stopWhenPlayerMoves: true);

        /// <summary>
        /// Creates new instance of the sink (umyvadlo w1) object in the Walsch's bathroom (koupelna
        /// w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateBathroomSink(Name name, Plane area, bool decorative, bool pickable)
                        => new DumpObject(name, area, "umyvadlo", decorative, pickable: pickable, null, "snd8", stopWhenPlayerMoves: true);

        /// <summary>
        /// Creates new instance of the bench (lavička c1) object in the Carson's garden (zahrada
        /// c1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateCarsonsBench(Name name, Plane area, bool decorative, bool pickable)
                    => new CarsonsBench(name, area, decorative, pickable: pickable);

        /// <summary>
        /// Creates new instance of the coffee maker (kávovar) object.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateCoffeemaker(Name name, Plane area, bool decorative, bool pickable)
            => new DumpObject(name, area, "kávovar", decorative, pickable: pickable, null, "ActCoffeeMaker", stopWhenPlayerMoves: true);

        /// <summary>
        /// Creates new instance of the corpse (tělo w1) object in the poolside (bazén w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateCorpse(Name name, Plane area, bool decorative, bool pickable)
            => new Corpse(name, area, decorative, pickable: pickable);

        /// <summary>
        /// Creates new instance of the electrical box (rozvodna w1) object in the basement (sklep
        /// w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateElectricalBox(Name name, Plane area, bool decorative, bool pickable)
=> new DumpObject(name, area, "rozvodna", decorative, pickable: pickable, null, null, "ElectricalBoxLoop", volume: .5f);

        /// <summary>
        /// Creates new instance of the fish tank (akvárko w1) object in the basement (sklep w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateFishTank(Name name, Plane area, bool decorative, bool pickable)
=> new DumpObject(name, area, "akvárko", decorative, pickable: pickable, null, null, "FishTankLoop");

        /// <summary>
        /// Creates new instance of the garden grill (gril c1) object in the Sweeney's garden
        /// (zahrada c1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateGardenGril(Name name, Plane area, bool decorative, bool pickable)
        => new CarsonsGrill(name, area, decorative, pickable: pickable);

        /// <summary>
        /// Creates new instance of the garden hose (hadice c1) object in the Sweeney's
        /// garden(zahrda c1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateGardenHose(Name name, Plane area, bool decorative, bool pickable)
        => new DumpObject(name, area, "zahradní hadice", decorative, pickable: pickable, null, "snd1", null, stopWhenPlayerMoves: true);

        /// <summary>
        /// Creates new instance of a high way (dálnice w1) object.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateHighway(Name name, Plane area, bool decorative, bool pickable)
=> new DumpObject(name, area, "dálnice", decorative, pickable: pickable, null, null, "HighwayLoop");

        /// <summary>
        /// Creates new instance of the Detective Chipotle's car (detektivovo auto) object in the
        /// Walsch's drive way (příjezdová cesta w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static ChipotlesCar CreateChipotlesCar(Name name, Plane area, bool decorative, bool pickable)
            => new ChipotlesCar(name, area, decorative, pickable: pickable);

        /// <summary>
        /// Creates new instance of an object according to the specified type.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <param name="type">Type of the object</param>
        /// <param name="decorative">Specifies if the object works as a decorator.</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateObject(Name name, Plane area, string type, bool decorative = false, bool pickable = false)
        {
            if (string.IsNullOrEmpty(name.Indexed))
                throw new ArgumentException(nameof(name));

            switch (type)
            {
                case "zapnutý sporák": return CreateWorkingOven(name, area, decorative, pickable);
                case "vana": return CreateTub(name, area, decorative, pickable);
                case "toaleťák": return CreateToiletPaper(name, area, decorative, pickable: pickable);
                case "police": return CreateShelf(name, area, decorative, pickable);
                case "záchod": return CreateToiletBowl(name, area, decorative   , pickable);
                case "mikrovlnka": return CreateMicrowave(name, area, decorative, pickable);
                case "elektrický gril": return CreateElectricGrill(name, area, decorative, pickable);
                case "vysavač": return CreateVacuumCleaner(name, area, decorative, pickable);
                case "dvířka": return CreateLittleDoor(name, area, decorative, pickable);
                case "koš": return CreateTrashCan(name, area, decorative, pickable);
                case "dřez": return CreateKitchenSink(name, area, decorative, pickable);
                case "linka": return CreateKitchenCounter(name, area, decorative, pickable);
                case "stůl": return CreateTable(name, area, decorative, pickable);
                case "dálnice": return CreateHighway(name, area, decorative, pickable);
                case "rozvodna": return CreateElectricalBox(name, area, decorative, pickable);
                case "větrák": return CreateFan(name, area, decorative, pickable);
                case "krb": return CreateFireplace(name, area, decorative, pickable);
                case "mrazák": return CreateFreezer(name, area, decorative, pickable);
                case "akvárko": return CreateFishTank(name, area, decorative, pickable);
                case "hodiny": return CreateWallClock(name, area, decorative, pickable);
                case "kukačkové hodiny": return CreateCuckooClock(name, area, decorative, pickable);
                case "křeslo u mariottiho": return CreateMariottisChair(name, area, decorative, pickable);
                case "sweeneyho zvonek": return CreateSweeneysBell(name, area, decorative, pickable);
                case "christinin zvonek": return CreateChristinesBell(name, area, decorative, pickable);
                case "zrcadlo u kristýny": return CreateChristinesMirror(name, area, decorative, pickable);
                case "vražedné auto": return CreateKillersCar(name, area, decorative, pickable);
                case "věšák na klíče": return CreateKeyHanger(name, area, decorative, pickable);
                case "hospodská lavice": return CreatePubBench(name, area, decorative, pickable);
                case "lavice u carsona": return CreateCarsonsBench(name, area, decorative, pickable);
                case "trezor": return CreateSafe(name, area, decorative, pickable);
                case "stůl u sweeneyho": return CreateSweeneysTable(name, area, decorative, pickable);
                case "počítač u sweeneyho": return CreateSweeneysComputer(name, area, decorative, pickable);
                case "mobil u sweeneyho": return CreateSweeneysPhone(name, area, decorative, pickable);
                case "auto vanilla crunch": return CreateVanillaCrunchCar(name, area, decorative, pickable);
                case "pinball": return CreatePinball(name, area, decorative, pickable);
                case "automat na zmrzlinu": return CreateIcecreamMachine(name, area, decorative, pickable);
                case "židle": return CreateChair(name, area, decorative, pickable);
                case "myčka": return CreateDishwasher(name, area, decorative, pickable);
                case "lednice": return CreateFridge(name, area, decorative, pickable);
                case "kávovar": return CreateCoffeemaker(name, area, decorative, pickable);
                case "umyvadlo": return CreateBathroomSink(name, area, decorative, pickable);
                case "bazének": return CreateBathroomPool(name, area, decorative, pickable);
                case "detektivovo auto": return CreateChipotlesCar(name, area, decorative, pickable);
                case "mrtvola": return CreateCorpse(name, area, decorative, pickable);
                case "prkno u bazénu": return CreatePoolsidePlank(name, area, decorative, pickable);
                case "popelnice u bazénu": return CreatePoolsideBin(name, area, decorative, pickable);
                case "lavička u bazénu": return CreatePoolsideBench(name, area, decorative, pickable);
                case "schůdky u bazénu": return CreatePoolStairs(name, area, decorative, pickable);
                case "gril u carsona": return CreateGardenGril(name, area, decorative, pickable);
                case "zahradní hadice": return CreateGardenHose(name, area, decorative, pickable);

                default: return new DumpObject(name, area, string.Empty, decorative, pickable);
            }
        }

        /// <summary>
        /// Creates new instance of a oven turned on.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateWorkingOven(Name name, Plane area, bool decorative, bool pickable)
                    => new DumpObject(name: name, area: area, type: "zapnutý sporák", decorative: decorative, pickable: pickable, loopSound: "LoopOven");

        /// <summary>
        /// Creates new instance of a tub.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateTub(Name name, Plane area, bool decorative, bool pickable)
                    => new DumpObject(name: name, area: area, type: "vana", decorative: decorative, pickable: pickable, collisionSound: "HitTub");

        /// <summary>
        /// Creates new instance of toilet paper.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateToiletPaper(Name name, Plane area, bool decorative, bool pickable)
                    => new DumpObject(name: name, area: area, type: "mikrovlnka", decorative: decorative, pickable: pickable, collisionSound: "HitToiletPaper");

        /// <summary>
        /// Creates new instance of a shelf.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateShelf(Name name, Plane area, bool decorative, bool pickable)
                    => new DumpObject(name: name, area: area, type: "police", decorative: decorative, pickable: pickable, collisionSound: "HitShelf");

        /// <summary>
        /// Creates new instance of a toilet bowl.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateToiletBowl(Name name, Plane area, bool decorative, bool pickable)
                    => new DumpObject(name: name, area: area, type: "záchod", decorative: decorative, pickable: pickable, collisionSound: "HitToiletBowl", actionSound: "ActToiletBowl");

        /// <summary>
        /// Creates new instance of a microwave.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateMicrowave(Name name, Plane area, bool decorative, bool pickable)
                    => new DumpObject(name: name, area: area, type: "mikrovlnka", decorative: decorative, pickable: pickable, collisionSound: "HitMicrowave", actionSound: "ActMicrowave");

        /// <summary>
        /// Creates new instance of a electric grill.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateElectricGrill(Name name, Plane area, bool decorative, bool pickable)
                    => new DumpObject(name: name, area: area, type: "elektrický gril", decorative: decorative, pickable: pickable, collisionSound: "HitelectricGrill");

        /// <summary>
        /// Creates new instance of a kitchen sink.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateVacuumCleaner(Name name, Plane area, bool decorative, bool pickable)
                    => new DumpObject(name: name, area: area, type: "vysavač", decorative: decorative, pickable: pickable, cutscene: "cs38");

        /// <summary>
        /// Creates new instance of a kitchen sink.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateLittleDoor(Name name, Plane area, bool decorative, bool pickable)
                    => new DumpObject(name: name, area: area, type: "dvířka", decorative: decorative, pickable: pickable, collisionSound: "HitLittleDoor");

        /// <summary>
        /// Creates new instance of a trash can.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateTrashCan(Name name, Plane area, bool decorative, bool pickable)
                    => new DumpObject(name: name, area: area, type: "koš", decorative: decorative, pickable: pickable, collisionSound: "HitTrashCan");

        /// <summary>
        /// Creates new instance of a kitchen sink.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateKitchenSink(Name name, Plane area, bool decorative, bool pickable)
                    => new DumpObject(name: name, area: area, type: "dřez", decorative: decorative, pickable: pickable, collisionSound: "HitKitchenSink", actionSound: "ActKitchenSink", stopWhenPlayerMoves: true);

        /// <summary>
        /// Creates new instance of a kitchen counter.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateKitchenCounter(Name name, Plane area, bool decorative, bool pickable)
                    => new DumpObject(name: name, area: area, type: "linka", decorative: decorative, pickable: pickable, collisionSound: "HitKitchenCounter", actionSound: "ActKitchenCounter", stopWhenPlayerMoves: true);

        /// <summary>
        /// Creates new instance of a table.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateTable(Name name, Plane area, bool decorative, bool pickable)
                    => new DumpObject(name: name, area: area, type: "stůl", decorative: decorative, pickable: pickable, collisionSound: "HitTable");

        /// <summary>
        /// Creates new instance of the bench (lavička w1) object in the Walsch's pool (bazén w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreatePoolsideBench(Name name, Plane area, bool decorative, bool pickable)
                    => new DumpObject(name, area, "lavička u bazénu", decorative, pickable: pickable, null, null, null, "cs1", true);

        /// <summary>
        /// Creates new instance of the bin (popelnice w1) object in the Walsch's pool (bazén w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreatePoolsideBin(Name name, Plane area, bool decorative, bool pickable)
        => new PoolsideBin(name, area, decorative, pickable: pickable);

        /// <summary>
        /// Creates new instance of the plank (prkno w1) object in the Walsch's pool (bazén w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreatePoolsidePlank(Name name, Plane area, bool decorative, bool pickable)
                    => new DumpObject(name, area, "prkno u bazénu", decorative, pickable: pickable, null, null, null, "cs4");

        /// <summary>
        /// Creates new instance of the pool steps (schůdky w1) object in the Walsch's pool (bazén
        /// w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreatePoolStairs(Name name, Plane area, bool decorative, bool pickable)
        => new DumpObject(name, area, "schůdky u bazénu", decorative, pickable: pickable, null, "snd3", quickActionsAllowed: true);

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
        /// Destroys the object or NPC.
        /// </summary>
        protected override void Destroy()
        {
            Locality?.Unregister(this);
            World.Remove(this);
            Disappear();
        }

        /// <summary>
        /// Moves the object to the specified coordinates.
        /// </summary>
        /// <param name="targetArea">Target coordinates</param>
        protected virtual void Move(Plane targetArea)
        {
            if (targetArea == null)
                throw new ArgumentNullException(nameof(targetArea));

            Disappear();
            Area = targetArea;
            Locality sourceLocality = Locality;
            _locality = _area.GetLocality();
            Appear();

            if (sourceLocality != Locality)
            {
                if (this is DumpObject d)
                {
                    sourceLocality.Unregister(d);
                    Locality.Register(d);
                }
                else if (this is Entity e)
                {
                    sourceLocality.Unregister(e);
                    Locality.Register(e);
                }
            }
        }

        /// <summary>
        /// Creates new instance of the cuckoo clock (kukačkové hodiny w1) object in the Walsch's
        /// dining room (jídelna w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateCuckooClock(Name name, Plane area, bool decorative, bool pickable)
        => new DumpObject(name, area, "kukačkové hodiny", decorative, pickable: pickable, null, null, "CuckooClockLoop",null,false, volume: .5f);

        /// <summary>
        /// Creates new instance of the dish washer (myčka) object.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static DumpObject CreateDishwasher(Name name, Plane area, bool decorative, bool pickable)
        => new DumpObject(name: name, area: area, type: "myčka", decorative: decorative, pickable: pickable, collisionSound: "HitDishWasher", actionSound: "ActDishwasher");

        /// <summary>
        /// Creates new instance of the fan (větrák p1) object in the Christine's bed room (ložnice
        /// p1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateFan(Name name, Plane area, bool decorative, bool pickable)
        => new DumpObject(name, area, "větrák", decorative, pickable: pickable, null, null, "FanLoop", volume: 1);

        /// <summary>
        /// Creates new instance of the fire place (krb) object.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateFireplace(Name name, Plane area, bool decorative, bool pickable)
        => new DumpObject(name, area, "krb", decorative, pickable: pickable, null, null, "FirePlaceLoop");

        /// <summary>
        /// Creates new instance of the freezer (mrazák v1) object in the Mariotti!s office
        /// (kancelář v1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateFreezer(Name name, Plane area, bool decorative, bool pickable)
        => new DumpObject(name, area, "mrazák", decorative, pickable: pickable, null, null, "FreezerLoop");

        /// <summary>
        /// Creates new instance of the fridge (lednice) object.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateFridge(Name name, Plane area, bool decorative, bool pickable)
        => new DumpObject(name, area, "lednice", decorative, pickable: pickable, null, "snd10", "FridgeLoop", volume: .5f, stopWhenPlayerMoves: true);

        /// <summary>
        /// Creates new instance of the chair (židle) object.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateChair(Name name, Plane area, bool decorative, bool pickable)
        => new DumpObject(name: name, area: area, type: "židle", decorative, pickable: pickable, actionSound: "ActChair", collisionSound: "HitChair");

        /// <summary>
        /// Creates new instance of the Christine's door bell object in the Belvedere street (ulice
        /// p1) locality.
        /// </summary>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateChristinesBell(Name name, Plane area, bool decorative, bool pickable)
            => new ChristinesBell(name, area, decorative, pickable: pickable);

        /// <summary>
        /// Creates new instance of the mirror (zrcadlo p1) object in the Christine's bed room
        /// (ložnice p1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateChristinesMirror(Name name, Plane area, bool decorative, bool pickable)
                    => new DumpObject(name, area, "zrcadlo u Kristýny", decorative, pickable: pickable, null, null, null, "cs14", true);

        /// <summary>
        /// Creates new instance of the icecream machine (automat v1) object in the hall of Vanilla
        /// crunch company (hala v1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateIcecreamMachine(Name name, Plane area, bool decorative, bool pickable)
                    => new IcecreamMachine(name, area, decorative, pickable: pickable);

        /// <summary>
        /// Creates new instance of the key hanger (věšák v1) object in the garage of Vanilla crunch
        /// company (garáž v1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateKeyHanger(Name name, Plane area, bool decorative, bool pickable) 
            => new KeyHanger(name, area, decorative, pickable: pickable);

        /// <summary>
        /// Creates new instance of the Killer's car (zabijákovo auto v1) object in the garage of
        /// Vanilla crunch company (garáž v1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static KillersCar CreateKillersCar(Name name, Plane area, bool decorative, bool pickable) => new KillersCar(name, area, decorative, pickable: pickable);

        /// <summary>
        /// Creates new instance of the Mariotti's chair (křeslo v1) object in the kancelář v1 locality.
        /// </summary>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateMariottisChair(Name name, Plane area, bool decorative, bool pickable)
            => new DumpObject(name, area, "křeslo u Mariottiho", decorative, pickable: pickable, null, null, null, "cs12", true);

        /// <summary>
        /// Creates new instance of the pinball table (pinball h1) object in the pub (výčep h1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreatePinball(Name name, Plane area, bool decorative, bool pickable)
            => new DumpObject(name, area, "pinball", decorative, pickable: pickable, null, "snd15", stopWhenPlayerMoves: true);

        /// <summary>
        /// Creates new instance of the table (stůl) object in the pub (výčep h1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static PubBench CreatePubBench(Name name, Plane area, bool decorative, bool pickable)
                        => new PubBench(name, area, decorative, pickable: pickable);

        /// <summary>
        /// Creates new instance of the safe (trezor s1) object in the Sweeney's bed room (pokoj s1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateSafe(Name name, Plane area, bool decorative, bool pickable)
                        => new DumpObject(name, area, "trezor", decorative, pickable: pickable, null, null, null, "cs18", true);

        /// <summary>
        /// Creates new instance of the Sweeney's door bell object in the Easterby street (ulice s1) locality.
        /// </summary>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateSweeneysBell(Name name, Plane area, bool decorative, bool pickable)
            => new SweeneysBell(name, area, decorative, pickable: pickable);

        /// <summary>
        /// Creates new instance of the computer (počítač s1) object in the Sweeney's bed room
        /// (pokoj s1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateSweeneysComputer(Name name, Plane area, bool decorative, bool pickable)
                => new DumpObject(name, area, "počítač u sweeneyho", decorative, pickable: pickable, null, null, "ComputerLoop", "cs16", true);

        /// <summary>
        /// Creates new instance of the cell phone (mobil s1) object in the Sweeney's bed room
        /// (pokoj s1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateSweeneysPhone(Name name, Plane area, bool decorative, bool pickable)
                => new DumpObject(name, area, "mobil u Sweeneyho", decorative, pickable: pickable, null, null, null, "cs15");

        /// <summary>
        /// Creates new instance of the table (stůl) object in the Sweeney's bed room (pokoj s1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateSweeneysTable(Name name, Plane area, bool decorative, bool pickable)
                                => new DumpObject(name, area, "stůl u sweeneyho", decorative, pickable: pickable, null, null, null, "cs17", true);

        /// <summary>
        /// Creates new instance of the car (auto) object in the garage of Vanilla crunch company
        /// (garáž v1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateVanillaCrunchCar(Name name, Plane area, bool decorative, bool pickable)
=> new VanillaCrunchCar(name, area, decorative, pickable: pickable);

        /// <summary>
        /// Creates new instance of the wall clock (hodiny) object.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static DumpObject CreateWallClock(Name name, Plane area, bool decorative, bool pickable)       
        => new DumpObject(name, area, "hodiny", decorative, pickable: pickable, null, null, "WallClockLoop", null, false, false);
    }
}