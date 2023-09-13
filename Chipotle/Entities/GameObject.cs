using System;
using System.Collections.Generic;
using System.Linq;
using Game.Terrain;
using Luky;
using ProtoBuf;

namespace Game.Entities
{
    /// <summary>
    /// A base class for all simple objects and NPCS
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    [ProtoInclude(100, typeof(Item))]
    [ProtoInclude(101, typeof(Character))]
    public class GameObject : MapElement
    {
	    /// <summary>
        /// Type of the object; it allows grouping objects with tha same behavior.
        /// </summary>
        public readonly string Type;

	    /// <summary>
        /// Stores ID of the current description of the object.
        /// </summary>
        protected int _descriptionID;

	    /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="type">Type of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        public GameObject(Name name, string type, Rectangle area) : base(name, area)
	    {
		    Type = type.PrepareForIndexing();
	    }

	    /// <summary>
        /// Returns current description of the object.
        /// </summary>
        public string Description => World.GetObjectDescription(this, _descriptionID);

	    /// <summary>
        /// Checks if the specified object and this object are at least partially in the same locality.
        /// </summary>
        /// <param name="o">The character or object to be checked</param>
        /// <returns>True if the specified object or character and this object or character are at least partially in the same locality.</returns>
        public bool SameLocality(GameObject o)
        {
            IEnumerable<Locality> mine = _area.GetLocalities();
            IEnumerable<Locality> its = o.Area.GetLocalities();

            return mine.Any(l => its.Contains(l));
        }


	    /// <summary>
        /// Creates new instance of the pool (bazének w1) object in the Walsch's bathroom (koupelna
        /// w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static Item CreateBathroomPool(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "bazének", decorative, pickable, null, "snd7", stopWhenPlayerMoves: true);
	    }

	    public static Item CreateBathroomSinkSweeney(Name name, Rectangle area, bool decorative, bool pickable)
		{
			return new Item(name, area, "umyvadlo koupelna s", decorative, pickable, null, "snd8",
				stopWhenPlayerMoves: true);
		}

	    public static Item CreateBathroomSinkPub(Name name, Rectangle area, bool decorative, bool pickable)
		{
			return new Item(name, area, "umyvadlo záchod h", decorative, pickable, null, "snd8",
				stopWhenPlayerMoves: true);
		}


	    /// <summary>
		/// Creates new instance of the sink (umyvadlo w1) object in the Walsch's bathroom (koupelna
		/// w1) locality.
		/// </summary>
		/// <param name="name">Inner and public name of the object</param>
		/// <param name="area">Coordinates of the area that the object occupies</param>
		/// <returns>New instance of the object</returns>
		public static Item CreateBathroomSinkWalsch(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "umyvadlo koupelna w", decorative, pickable, null, "snd8",
			    stopWhenPlayerMoves: true);
	    }

	    public static Item CreateBathroomSinkPierce(Name name, Rectangle area, bool decorative, bool pickable)
		{
			return new Item(name, area, "umyvadlo koupelna p", decorative, pickable, null, "snd8",
				stopWhenPlayerMoves: true);
		}


	    /// <summary>
		/// Creates new instance of the bench (lavička c1) object in the Carson's garden (zahrada
		/// c1) locality.
		/// </summary>
		/// <param name="name">Inner and public name of the object</param>
		/// <param name="area">Coordinates of the area that the object occupies</param>
		/// <returns>New instance of the object</returns>
		public static Item CreateCarsonsBench(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new CarsonsBench(name, area, decorative, pickable);
	    }

	    /// <summary>
        /// Creates new instance of the coffee maker (kávovar) object.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static Item CreateCoffeemaker(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "kávovar", decorative, pickable, null, null, null, "ActCoffeeMaker");
	    }

	    /// <summary>
        /// Creates new instance of the corpse (tělo w1) object in the poolside (bazén w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static Item CreateCorpse(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Corpse(name, area, decorative, pickable);
	    }

	    /// <summary>
        /// Creates new instance of the electrical box (rozvodna w1) object in the basement (sklep
        /// w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static Item CreateElectricalBox(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "rozvodna", decorative, pickable, null, null, "ElectricalBoxLoop", volume: .5f);
	    }

	    /// <summary>
        /// Creates new instance of the fish tank (akvárko w1) object in the basement (sklep w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static Item CreateFishTank(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "akvárko", decorative, pickable, null, null, "FishTankLoop");
	    }

	    /// <summary>
        /// Creates new instance of the garden grill (gril c1) object in the Sweeney's garden
        /// (zahrada c1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static Item CreateGardenGril(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new CarsonsGrill(name, area, decorative, pickable);
	    }

	    /// <summary>
        /// Creates new instance of the garden hose (hadice c1) object in the Sweeney's
        /// garden(zahrda c1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static Item CreateGardenHose(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "zahradní hadice", decorative, pickable, null, "snd1",
			    stopWhenPlayerMoves: true);
	    }

	    /// <summary>
        /// Creates new instance of a high way (dálnice w1) object.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static Item CreateHighway(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "dálnice", decorative, pickable, null, null, "HighwayLoop");
	    }

	    /// <summary>
        /// Creates new instance of the Detective Chipotle's car (detektivovo auto) object in the
        /// Walsch's drive way (příjezdová cesta w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static ChipotlesCar CreateChipotlesCar(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new ChipotlesCar(name, area, decorative, pickable);
	    }

	    /// <summary>
        /// Creates new instance of an object according to the specified type.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <param name="type">Type of the object</param>
        /// <param name="decorative">Specifies if the object works as a decorator.</param>
        /// <param name="pickable">Determines if the object can be picked up by a character</param>
        /// <returns>New instance of the object</returns>
        public static Item CreateObject(Name name, Rectangle area, string type, bool decorative = false, bool pickable = false)
        {
            if (string.IsNullOrEmpty(name.Indexed))
                throw new ArgumentException(nameof(name));

            switch (type)
            {
                case "sporák p": return CreateOvenPierce(name, area, decorative, pickable);
                case "vana p": return CreateTubPierce(name, area, decorative, pickable);
                case "toaleťák": return CreateToiletPaper(name, area, decorative, pickable);
                case "police v garáži p": return CreateGarageShelfPierce(name, area, decorative, pickable);
                case "záchod w": return CreateToiletBowlWalsch(name, area, decorative, pickable);
                case "záchod s": return CreateToiletBowlSweeney(name, area, decorative, pickable);
                case "mísa záchodová h": return CreateToiletBowlPub(name, area, decorative, pickable);
                case "mikrovlnka s": return CreateMicrowaveSweeney(name, area, decorative, pickable);
                case "gril elektrický": return CreateElectricGrill(name, area, decorative, pickable);
                case "vysavač robotický": return CreateVacuumCleaner(name, area, decorative, pickable);
                case "dvířka u bar pultu": return CreateBarCounterDoorPub(name, area, decorative, pickable);
                case "koš čistý": return CreateCleanTrashCan(name, area, decorative, pickable);
                case "koš odpadkový na ulici": return CreateStreetTrashCan(name, area, decorative, pickable);
                case "koš drátěný na záchodech h": return CreateStreetTrashCan(name, area, decorative, pickable);
                case "dřez čistý": return CreateCleanKitchenSink(name, area, decorative, pickable);
                case "dřez s": return CreateKitchenSinkSweeney(name, area, decorative, pickable);
                case "dřez c": return CreateKitchenSinkCarson(name, area, decorative, pickable);
                case "linka w": return CreateKitchenCounterWalsch(name, area, decorative, pickable);
                case "linka s": return CreateKitchenCounterSweeney(name, area, decorative, pickable);
                case "linka p": return CreateKitchenCounterPierce(name, area, decorative, pickable);

                case "stůl jídelní p": return CreateDiningTable(name, area, decorative, pickable);
                case "stolek obývák p": return CreateCoffeeTablePierce(name, area, decorative, pickable);
                case "stůl u mariottiho": return CreateOfficeTableMariotti(name, area, decorative, pickable);
				case "stůl pokoj s": return CreateComputerTableSweeney(name, area, decorative, pickable);
				case "stůl kuchyňský s": return CreateDiningTableSweeney(name, area, decorative, pickable);
				case "stůl zahradní c": return CreateGardenTableSweeney(name, area, decorative, pickable);
                case "stůl hospodský": return CreateDiningTable(name, area, decorative, pickable);
				case "dálnice": return CreateHighway(name, area, decorative, pickable);
                case "rozvodna": return CreateElectricalBox(name, area, decorative, pickable);
                case "větrák": return CreateFan(name, area, decorative, pickable);
                case "krb w": return CreateFireplaceWalsch(name, area, decorative, pickable);
                case "mrazák v": return CreateFreezerMariotti(name, area, decorative, pickable);
                case "akvárko": return CreateFishTank(name, area, decorative, pickable);
                case "hodiny kočičí p": return CreateCatWallClockPierce(name, area, decorative, pickable);
                case "kukačkové hodiny": return CreateCuckooClock(name, area, decorative, pickable);
                case "křeslo u mariottiho": return CreateMariottisChair(name, area, decorative, pickable);
                case "sweeneyho zvonek": return CreateSweeneysBell(name, area, decorative, pickable);
                case "christinin zvonek": return CreateChristinesBell(name, area, decorative, pickable);
                case "zrcadlo u kristýny": return CreateChristinesMirror(name, area, decorative, pickable);
                case "vražedné auto": return CreateKillersCar(name, area, decorative, pickable);
				case "auto s delfínem": return CreateVanillaCrunchDolphinCar(name, area, decorative, pickable);
				case "auto s kočkou": return CreateVanillaCrunchCatCar(name, area, decorative, pickable);
				case "auto s krtkem": return CreateVanillaCrunchMoleCar(name, area, decorative, pickable);
				case "auto s motýlem": return CreateVanillaCrunchButterflyCar(name, area, decorative, pickable);
				case "auto s mrožem": return CreateVanillaCrunchWalrusCar(name, area, decorative, pickable);
				case "auto s rackem": return CreateVanillaCrunchSeagullCar(name, area, decorative, pickable);
				case "auto s tučňákem": return CreateVanillaCrunchPenguinCar(name, area, decorative, pickable);
				case "auto se psem": return CreateVanillaCrunchDogCar(name, area, decorative, pickable);
				case "auto se sněhulákem": return CreateVanillaCrunchSnowmanCar(name, area, decorative, pickable);
				case "věšák na klíče": return CreateKeyHanger(name, area, decorative, pickable);
                case "hospodská lavice": return CreatePubBench(name, area, decorative, pickable);
                case "lavice u carsona": return CreateCarsonsBench(name, area, decorative, pickable);
                case "trezor": return CreateSafe(name, area, decorative, pickable);
                case "počítač u sweeneyho": return CreateSweeneysComputer(name, area, decorative, pickable);
                case "mobil u sweeneyho": return CreateSweeneysPhone(name, area, decorative, pickable);
                case "pinball": return CreatePinball(name, area, decorative, pickable);
                case "automat na zmrzlinu": return CreateIcecreamMachine(name, area, decorative, pickable);

				case "židle kuchyň p": return CreateKitchenChairPierce(name, area, decorative, pickable);
                case "židle s": return CreateKitchenChairSweeney(name, area, decorative, pickable);
				case "židle kuchyňská s": return CreateKitchenChairSweeney(name, area, decorative, pickable);
                case "židle v jídelně w": return CreateDiningRoomChairWalsch(name, area, decorative, pickable);
                case "židle kuchyň w": return CreateKitchenChairWalsch(name, area, decorative, pickable);
                case "židle salon w": return CreateSalonChairWalsch(name, area, decorative, pickable);

                case "myčka": return CreateDishwasher(name, area, decorative, pickable);
				case "lednice p": return CreateFridgePierce(name, area, decorative, pickable);
                case "lednice s": return CreateFridgeSweeney(name, area, decorative, pickable);
				case "lednice w": return CreateFridgeWalsch(name, area, decorative, pickable);

                case "kávovar": return CreateCoffeemaker(name, area, decorative, pickable);

				case "umyvadlo koupelna w": return CreateBathroomSinkWalsch(name, area, decorative, pickable);
                case "umyvadlo koupelna p": return CreateBathroomSinkPierce(name, area, decorative, pickable);
                case "umyvadlo koupelna s": return CreateBathroomSinkSweeney(name, area, decorative, pickable);
                case "umyvadlo záchod h": return CreateBathroomSinkPub(name, area, decorative, pickable);

                case "bazének": return CreateBathroomPool(name, area, decorative, pickable);
                case "detektivovo auto": return CreateChipotlesCar(name, area, decorative, pickable);
                case "mrtvola": return CreateCorpse(name, area, decorative, pickable);
                case "prkno u bazénu": return CreatePoolsidePlank(name, area, decorative, pickable);
                case "popelnice u bazénu": return CreatePoolsideBin(name, area, decorative, pickable);
                case "lavička u bazénu": return CreatePoolsideBench(name, area, decorative, pickable);
                case "schůdky u bazénu": return CreatePoolStairs(name, area, decorative, pickable);
                case "gril u carsona": return CreateGardenGril(name, area, decorative, pickable);
                case "zahradní hadice": return CreateGardenHose(name, area, decorative, pickable);

                default: return new Item(name, area, string.Empty, decorative, pickable);
            }
        }

	    /// <summary>
        /// Creates new instance of a oven turned on.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static Item CreateOvenPierce(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "sporák p", decorative, pickable, loopSound: "LoopOven");
	    }

	    /// <summary>
        /// Creates new instance of a tub.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static Item CreateTubPierce(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "vana p", decorative, pickable, "HitTub");
	    }

	    /// <summary>
        /// Creates new instance of toilet paper.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static Item CreateToiletPaper(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "toaleťák", decorative, pickable, "HitToiletPaper");
	    }

	    /// <summary>
        /// Creates new instance of a shelf.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static Item CreateGarageShelfPierce(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "police v garáži p", decorative, pickable, "HitShelf");
	    }

	    /// <summary>
        /// Creates new instance of a toilet bowl.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static Item CreateToiletBowlWalsch(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "záchod w", decorative, pickable, "HitToiletBowl", "ActToiletBowl");
	    }

	    /// <summary>
	    /// Creates new instance of a toilet bowl.
	    /// </summary>
	    /// <param name="name">Inner and public name of the object</param>
	    /// <param name="area">Coordinates of the area that the object occupies</param>
	    /// <returns>New instance of the object</returns>
	    public static Item CreateToiletBowlSweeney(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "záchod s", decorative, pickable, "HitToiletBowl", "ActToiletBowl");
	    }

	    /// <summary>
	    /// Creates new instance of a toilet bowl.
	    /// </summary>
	    /// <param name="name">Inner and public name of the object</param>
	    /// <param name="area">Coordinates of the area that the object occupies</param>
	    /// <returns>New instance of the object</returns>
	    public static Item CreateToiletBowlPub(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "mísa záchodová h", decorative, pickable, "HitToiletBowl", "ActToiletBowl");
	    }


	    /// <summary>
		/// Creates new instance of a microwave.
		/// </summary>
		/// <param name="name">Inner and public name of the object</param>
		/// <param name="area">Coordinates of the area that the object occupies</param>
		/// <returns>New instance of the object</returns>
		public static Item CreateMicrowaveSweeney(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "mikrovlnka s", decorative, pickable, "HitMicrowave", "ActMicrowave");
	    }

	    /// <summary>
        /// Creates new instance of a electric grill.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static Item CreateElectricGrill(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "gril elektrický", decorative, pickable, "HitelectricGrill");
	    }

	    /// <summary>
        /// Creates new instance of a kitchen sink.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static Item CreateVacuumCleaner(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "vysavač robotický", decorative, pickable, cutscene: "cs39");
	    }

	    /// <summary>
        /// Creates new instance of a kitchen sink.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static Item CreateBarCounterDoorPub(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "dvířka u bar pultu", decorative, pickable, "HitLittleDoor");
	    }

	    /// <summary>
        /// Creates new instance of a trash can.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static Item CreateCleanTrashCan(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "koš čistý", decorative, pickable, "HitTrashCan");
	    }

	    /// <summary>
	    /// Creates new instance of a trash can.
	    /// </summary>
	    /// <param name="name">Inner and public name of the object</param>
	    /// <param name="area">Coordinates of the area that the object occupies</param>
	    /// <returns>New instance of the object</returns>
	    public static Item CreateStreetTrashCan(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "koš odpadkový na ulici", decorative, pickable, "HitTrashCan");
	    }

	    /// <summary>
	    /// Creates new instance of a trash can.
	    /// </summary>
	    /// <param name="name">Inner and public name of the object</param>
	    /// <param name="area">Coordinates of the area that the object occupies</param>
	    /// <returns>New instance of the object</returns>
	    public static Item CreateWirePubTrashCan(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "koš drátěný na záchodech h", decorative, pickable, "HitTrashCan");
	    }

	    /// <summary>
	    /// Creates new instance of a kitchen sink.
	    /// </summary>
	    /// <param name="name">Inner and public name of the object</param>
	    /// <param name="area">Coordinates of the area that the object occupies</param>
	    /// <returns>New instance of the object</returns>
	    public static Item CreateKitchenSinkCarson(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "dřez c", decorative, pickable, "HitKitchenSink", "ActKitchenSink",
			    stopWhenPlayerMoves: true);
	    }


	    /// <summary>
		/// Creates new instance of a kitchen sink.
		/// </summary>
		/// <param name="name">Inner and public name of the object</param>
		/// <param name="area">Coordinates of the area that the object occupies</param>
		/// <returns>New instance of the object</returns>
		public static Item CreateKitchenSinkSweeney(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "dřez s", decorative, pickable, "HitKitchenSink", "ActKitchenSink",
			    stopWhenPlayerMoves: true);
	    }


	    /// <summary>
		/// Creates new instance of a kitchen sink.
		/// </summary>
		/// <param name="name">Inner and public name of the object</param>
		/// <param name="area">Coordinates of the area that the object occupies</param>
		/// <returns>New instance of the object</returns>
		public static Item CreateCleanKitchenSink(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "dřez čistý", decorative, pickable, "HitKitchenSink", "ActKitchenSink",
			    stopWhenPlayerMoves: true);
	    }


	    /// <summary>
	    /// Creates new instance of a kitchen counter.
	    /// </summary>
	    /// <param name="name">Inner and public name of the object</param>
	    /// <param name="area">Coordinates of the area that the object occupies</param>
	    /// <returns>New instance of the object</returns>
	    public static Item CreateKitchenCounterPierce(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "linka p", decorative, pickable, "HitKitchenCounter", "ActKitchenCounter",
			    stopWhenPlayerMoves: true);
	    }


	    /// <summary>
		/// Creates new instance of a kitchen counter.
		/// </summary>
		/// <param name="name">Inner and public name of the object</param>
		/// <param name="area">Coordinates of the area that the object occupies</param>
		/// <returns>New instance of the object</returns>
		public static Item CreateKitchenCounterSweeney(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "linka s", decorative, pickable, "HitKitchenCounter", "ActKitchenCounter",
			    stopWhenPlayerMoves: true);
	    }


	    /// <summary>
		/// Creates new instance of a kitchen counter.
		/// </summary>
		/// <param name="name">Inner and public name of the object</param>
		/// <param name="area">Coordinates of the area that the object occupies</param>
		/// <returns>New instance of the object</returns>
		public static Item CreateKitchenCounterWalsch(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "linka w", decorative, pickable, "HitKitchenCounter", "ActKitchenCounter",
			    stopWhenPlayerMoves: true);
	    }

	    /// <summary>
		/// Creates new instance of a table.
		/// </summary>
		/// <param name="name">Inner and public name of the object</param>
		/// <param name="area">Coordinates of the area that the object occupies</param>
		/// <returns>New instance of the object</returns>
		public static Item CreateOfficeTableMariotti(Name name, Rectangle area, bool decorative, bool pickable)
		{
			return new Item(name, area, "stůl u mariottiho", decorative, pickable, "HitTable");
		}

	    /// <summary>
		/// Creates new instance of a table.
		/// </summary>
		/// <param name="name">Inner and public name of the object</param>
		/// <param name="area">Coordinates of the area that the object occupies</param>
		/// <returns>New instance of the object</returns>
		public static Item CreateCoffeeTablePierce(Name name, Rectangle area, bool decorative, bool pickable)
		{
			return new Item(name, area, "stolek obývák p", decorative, pickable, "HitTable");
		}


	    /// <summary>
		/// Creates new instance of a table.
		/// </summary>
		/// <param name="name">Inner and public name of the object</param>
		/// <param name="area">Coordinates of the area that the object occupies</param>
		/// <returns>New instance of the object</returns>
		public static Item CreateDiningTable(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "stůl jídelní p", decorative, pickable, "HitTable");
	    }


	    public static Item CreateTablePub(Name name, Rectangle area, bool decorative, bool pickable)
		{
			return new Item(name, area, "stůl hospodský", decorative, pickable, "HitTable");
		}

	    public static Item CreateGardenTableSweeney(Name name, Rectangle area, bool decorative, bool pickable)
		{
			return new Item(name, area, "stůl zahradní c", decorative, pickable, "HitTable");
		}

	    /// <summary>
		/// Creates new instance of the bench (lavička w1) object in the Walsch's pool (bazén w1) locality.
		/// </summary>
		/// <param name="name">Inner and public name of the object</param>
		/// <param name="area">Coordinates of the area that the object occupies</param>
		/// <returns>New instance of the object</returns>
		public static Item CreatePoolsideBench(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "lavička u bazénu", decorative, pickable, null, null, null, "cs1", true);
	    }

	    /// <summary>
        /// Creates new instance of the bin (popelnice w1) object in the Walsch's pool (bazén w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static Item CreatePoolsideBin(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new PoolsideBin(name, area, decorative, pickable);
	    }

	    /// <summary>
        /// Creates new instance of the plank (prkno w1) object in the Walsch's pool (bazén w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static Item CreatePoolsidePlank(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "prkno u bazénu", decorative, pickable, null, null, null, "cs4");
	    }

	    /// <summary>
        /// Creates new instance of the pool steps (schůdky w1) object in the Walsch's pool (bazén
        /// w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static Item CreatePoolStairs(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "schůdky u bazénu", decorative, pickable, "snd3");
	    }

	    /// <summary>
        /// Returns the hash code for this object.
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
	    {
		    return unchecked(3000 * (2000 + Area.GetHashCode()) * (3000 + Type.GetHashCode()) * 4000);
	    }

	    /// <summary>
        /// Returns public name of the object.
        /// </summary>
        /// <returns>public name of the object</returns>
        public override string ToString()
	    {
		    return Name.Friendly;
	    }

	    /// <summary>
        /// Destroys the object or NPC.
        /// </summary>
        protected override void Destroy()
	    {
		    World.Remove(this);
	    }

	    /// <summary>
        /// Moves the object to the specified coordinates.
        /// </summary>
        /// <param name="targetArea">Target coordinates</param>
        protected virtual void Move(Rectangle targetArea)
	    {
		    Area = targetArea ?? throw new ArgumentNullException(nameof(targetArea));
	    }

	    /// <summary>
        /// Creates new instance of the cuckoo clock (kukačkové hodiny w1) object in the Walsch's
        /// dining room (jídelna w1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static Item CreateCuckooClock(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "kukačkové hodiny", decorative, pickable, null, null, "CuckooClockLoop",
			    volume: .5f);
	    }

	    /// <summary>
        /// Creates new instance of the dish washer (myčka) object.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        public static Item CreateDishwasher(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "myčka", decorative, pickable, "HitDishWasher", "ActDishwasher");
	    }

	    /// <summary>
        /// Creates new instance of the fan (větrák p1) object in the Christine's bed room (ložnice
        /// p1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static Item CreateFan(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "větrák", decorative, pickable, null, null, "FanLoop", volume: 1);
	    }

	    /// <summary>
        /// Creates new instance of the fire place (krb) object.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static Item CreateFireplaceWalsch(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "krb w", decorative, pickable, null, null, "FirePlaceLoop");
	    }

	    /// <summary>
        /// Creates new instance of the freezer (mrazák v1) object in the Mariotti!s office
        /// (kancelář v1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static Item CreateFreezerMariotti(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "mrazák v", decorative, pickable, null, null, "FreezerLoop");
	    }

	    /// <summary>
        /// Creates new instance of the fridge (lednice) object.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static Item CreateFridgePierce(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "lednice p", decorative, pickable, null, "snd10", "FridgeLoop", volume: .5f,
			    stopWhenPlayerMoves: true);
	    }

	    private static Item CreateFridgeSweeney(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "lednice s", decorative, pickable, null, "snd10", "FridgeLoop", volume: .5f,
			    stopWhenPlayerMoves: true);
	    }

	    private static Item CreateFridgeWalsch(Name name, Rectangle area, bool decorative, bool pickable)
		{
			return new Item(name, area, "lednice w", decorative, pickable, null, "snd10", "FridgeLoop", volume: .5f,
				stopWhenPlayerMoves: true);
		}


	    private static Item CreateKitchenChairWalsch(Name name, Rectangle area, bool decorative, bool pickable)
		{
			return new Item(name, area, "židle kuchyň w", decorative, pickable, actionSound: "ActChair",
				collisionSound: "HitChair");
		}


	    private static Item CreateSalonChairWalsch(Name name, Rectangle area, bool decorative, bool pickable)
		{
			return new Item(name, area, "židle salon w", decorative, pickable, actionSound: "ActChair",
				collisionSound: "HitChair");
		}


	    /// <summary>
		/// Creates new instance of the chair (židle) object.
		/// </summary>
		/// <param name="name">Inner and public name of the object</param>
		/// <param name="area">Coordinates of the area that the object occupies</param>
		/// <returns>New instance of the object</returns>
		private static Item CreateKitchenChairPierce(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "židle kuchyň p", decorative, pickable, actionSound: "ActChair",
			    collisionSound: "HitChair");
	    }

	    private static Item CreateDiningRoomChairWalsch(Name name, Rectangle area, bool decorative, bool pickable)
		{
			return new Item(name, area, "židle v jídelně w", decorative, pickable, actionSound: "ActChair",
				collisionSound: "HitChair");
		}


	    private static Item CreateChairSweeney(Name name, Rectangle area, bool decorative, bool pickable)
		{
			return new Item(name, area, "židle s", decorative, pickable, actionSound: "ActChair",
				collisionSound: "HitChair");
		}

	    private static Item CreateKitchenChairSweeney(Name name, Rectangle area, bool decorative, bool pickable)
		{
			return new Item(name, area, "židle kuchyňská s", decorative, pickable, actionSound: "ActChair",
				collisionSound: "HitChair");
		}


	    /// <summary>
		/// Creates new instance of the Christine's door bell object in the Belvedere street (ulice
		/// p1) locality.
		/// </summary>
		/// <returns>New instance of the object</returns>
		private static Item CreateChristinesBell(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new ChristinesBell(name, area, decorative, pickable);
	    }

	    /// <summary>
        /// Creates new instance of the mirror (zrcadlo p1) object in the Christine's bed room
        /// (ložnice p1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static Item CreateChristinesMirror(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "zrcadlo u Kristýny", decorative, pickable, null, null, null, "cs14", true);
	    }

	    /// <summary>
        /// Creates new instance of the icecream machine (automat v1) object in the hall of Vanilla
        /// crunch company (hala v1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static Item CreateIcecreamMachine(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new IcecreamMachine(name, area, decorative, pickable);
	    }

	    /// <summary>
        /// Creates new instance of the key hanger (věšák v1) object in the garage of Vanilla crunch
        /// company (garáž v1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static Item CreateKeyHanger(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new KeyHanger(name, area, decorative, pickable);
	    }

	    /// <summary>
        /// Creates new instance of the Killer's car (zabijákovo auto v1) object in the garage of
        /// Vanilla crunch company (garáž v1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static KillersCar CreateKillersCar(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new KillersCar(name, area, decorative, pickable);
	    }

	    /// <summary>
        /// Creates new instance of the Mariotti's chair (křeslo v1) object in the kancelář v1 locality.
        /// </summary>
        /// <returns>New instance of the object</returns>
        private static Item CreateMariottisChair(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "křeslo u Mariottiho", decorative, pickable, null, null, null, "cs12", true);
	    }

	    /// <summary>
        /// Creates new instance of the pinball table (pinball h1) object in the pub (výčep h1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static Item CreatePinball(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "pinball", decorative, pickable, null, "snd15", stopWhenPlayerMoves: true);
	    }

	    /// <summary>
        /// Creates new instance of the table (stůl) object in the pub (výčep h1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static PubBench CreatePubBench(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new PubBench(name, area, decorative, pickable);
	    }

	    /// <summary>
        /// Creates new instance of the safe (trezor s1) object in the Sweeney's bed room (pokoj s1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static Item CreateSafe(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "trezor", decorative, pickable, null, null, null, "cs18", true);
	    }

	    /// <summary>
        /// Creates new instance of the Sweeney's door bell object in the Easterby street (ulice s1) locality.
        /// </summary>
        /// <returns>New instance of the object</returns>
        private static Item CreateSweeneysBell(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new SweeneysBell(name, area, decorative, pickable);
	    }

	    /// <summary>
        /// Creates new instance of the computer (počítač s1) object in the Sweeney's bed room
        /// (pokoj s1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static Item CreateSweeneysComputer(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "počítač u sweeneyho", decorative, pickable, null, null, "ComputerLoop", "cs16",
			    true);
	    }

	    /// <summary>
        /// Creates new instance of the cell phone (mobil s1) object in the Sweeney's bed room
        /// (pokoj s1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static Item CreateSweeneysPhone(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "mobil u Sweeneyho", decorative, pickable, null, null, null, "cs15");
	    }

	    /// <summary>
        /// Creates new instance of the table (stůl) object in the Sweeney's bed room (pokoj s1) locality.
        /// </summary>
        /// <param name="name">Inner and public name of the object</param>
        /// <param name="area">Coordinates of the area that the object occupies</param>
        /// <returns>New instance of the object</returns>
        private static Item CreateComputerTableSweeney(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "stůl u sweeneyho", decorative, pickable, "HitTable", null, null, "cs17", true);
		}

	    private static Item CreateDiningTableSweeney(Name name, Rectangle area, bool decorative, bool pickable)
		{
			return new Item(name, area, "stůl kuchyňský s", decorative, pickable, "HitTable", null, null, "cs17", true);
		}

	    /// <summary>
		/// Creates new instance of the car (auto) object in the garage of Vanilla crunch company
		/// (garáž v1) locality.
		/// </summary>
		/// <param name="name">Inner and public name of the object</param>
		/// <param name="area">Coordinates of the area that the object occupies</param>
		/// <returns>New instance of the object</returns>
		private static Item CreateVanillaCrunchDolphinCar(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new VanillaCrunchCar(name, area, decorative, pickable, "auto s delfínem");
	    }

	    private static Item CreateVanillaCrunchCatCar(Name name, Rectangle area, bool decorative, bool pickable)
		{
			return new VanillaCrunchCar(name, area, decorative, pickable, "auto s kočkou");
		}

	    private static Item CreateVanillaCrunchMoleCar(Name name, Rectangle area, bool decorative, bool pickable)
		{
			return new VanillaCrunchCar(name, area, decorative, pickable, "auto s krtkem");
		}

	    private static Item CreateVanillaCrunchButterflyCar(Name name, Rectangle area, bool decorative, bool pickable)
		{
			return new VanillaCrunchCar(name, area, decorative, pickable, "auto s motýlem");
		}

	    private static Item CreateVanillaCrunchWalrusCar(Name name, Rectangle area, bool decorative, bool pickable)
		{
			return new VanillaCrunchCar(name, area, decorative, pickable, "auto s mrožem");
		}

	    private static Item CreateVanillaCrunchSeagullCar(Name name, Rectangle area, bool decorative, bool pickable)
		{
			return new VanillaCrunchCar(name, area, decorative, pickable, "auto s rackem");
		}

	    private static Item CreateVanillaCrunchPenguinCar(Name name, Rectangle area, bool decorative, bool pickable)
		{
			return new VanillaCrunchCar(name, area, decorative, pickable, "auto s tučňákem");
		}

	    private static Item CreateVanillaCrunchDogCar(Name name, Rectangle area, bool decorative, bool pickable)
		{
			return new VanillaCrunchCar(name, area, decorative, pickable, "auto se psem");
		}

	    private static Item CreateVanillaCrunchSnowmanCar(Name name, Rectangle area, bool decorative, bool pickable)
		{
			return new VanillaCrunchCar(name, area, decorative, pickable, "auto se sněhulákem");
		}


	    /// <summary>
		/// Creates new instance of the wall clock (hodiny) object.
		/// </summary>
		/// <param name="name">Inner and public name of the object</param>
		/// <param name="area">Coordinates of the area that the object occupies</param>
		/// <returns>New instance of the object</returns>
		private static Item CreateCatWallClockPierce(Name name, Rectangle area, bool decorative, bool pickable)
	    {
		    return new Item(name, area, "hodiny kočičí p", decorative, pickable, null, null, "WallClockLoop", null, false,
			    false);
	    }
    }
}