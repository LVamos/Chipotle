using System;
using System.Collections.Generic;
using System.Linq;

using DavyKager;

using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using OpenTK;

namespace Game.Entities
{
    /// <summary>
    /// Controls movement of the Detective Chipotle NPC.
    /// </summary>
    public class ChipotlePhysicsComponent : PhysicsComponent
    {
        /// <summary>
        /// Stores references to all the localities the NPC has visited.
        /// </summary>
        private readonly HashSet<Locality> _visitedLocalities = new HashSet<Locality>();

        /// <summary>
        /// A delayed message of ChipotlesCarMoved type
        /// </summary>
        private ChipotlesCarMoved _carMovement;

        /// <summary>
        /// Indicates the ongoing countdown of the phone call cutscene that should be played after
        /// the Detective Chipotle and Tuttle NPCs leave the Christine's bed room (ložnice p1) locality.
        /// </summary>
        private bool _phoneCountdown;

        /// <summary>
        /// The time that has elapsed since the phone call cutscene countdown started.
        /// </summary>
        private int _phoneDeltaTime;

        /// <summary>
        /// Specifies how long the phone call cutscene countdown should last.
        /// </summary>
        private int _phoneInterval;

        /// <summary>
        /// Indicates if the NPC is sitting at a table in the pub (výčep h1) locality.
        /// </summary>
        private bool _sittingAtPubTable;

        /// <summary>
        /// Indicates if the NPC is sitting on a chair.
        /// </summary>
        private bool _sittingOnChair;

        /// <summary>
        /// Indicates if the NPC stepped into the puddle in the pool (bazén w1) locality.
        /// </summary>
        private bool _steppedIntoPuddle;

        /// <summary>
        /// Returns reference to the asphalt road (asfaltka c1) locality.
        /// </summary>
        private Locality AsphaltRoad
            => World.GetLocality("asfaltka c1");

        /// <summary>
        /// Returns a reference to the Chipotle's car object.
        /// </summary>
        private ChipotlesCar Car
            => World.GetObject("detektivovo auto") as ChipotlesCar;

        /// <summary>
        /// Returns reference to the tile the NPC currently stands on.
        /// </summary>
        private Tile CuurrentTile
            => World.Map[_area.Center];

        /// <summary>
        /// Returns reference to the Tuttle NPC.
        /// </summary>
        private Entity Tuttle
            => World.GetEntity("tuttle");

        /// <summary>
        /// Initializes the component and starts its message loop.
        /// </summary>
        public override void Start()
        {
            // set initial position.
            SetPosition(1028, 1034, true);
            _orientation = new Orientation2D(0, 1);

            base.Start();

            RegisterMessages(
                new Dictionary<Type, Action<GameMessage>>()
                {
                    // Test messages
                    [typeof(SayTerrain)] = (message) => OnSayTerrain((SayTerrain)message),

                    // Other messages
                    [typeof(ChipotlesCarMoved)] = (message) => OnChipotlesCarMoved((ChipotlesCarMoved)message),
                    [typeof(CutsceneBegan)] = (m) => OnCutsceneBegan((CutsceneBegan)m),
                    [typeof(CutsceneEnded)] = (message) => OnCutsceneEnded((CutsceneEnded)message),
                    [typeof(SayNearestObject)] = (m) => OnSayNearestObject((SayNearestObject)m),
                    [typeof(SayLocality)] = (m) => OnSayLocality((SayLocality)m),
                    [typeof(MakeStep)] = (m) => OnMakeStep((MakeStep)m),
                    [typeof(TurnEntity)] = (message) => OnTurnEntity((TurnEntity)message),
                    [typeof(UseObject)] = (message) => OnUseObject((UseObject)message)
                }
                );

            // Play intro cutscene
            World.PlayCutscene(Owner, "cs6");
        }

        /// <summary>
        /// Processes incoming messages.
        /// </summary>
        public override void Update()
        {
            base.Update();
            CountPhone();
        }

        /// <summary>
        /// Processes the CutsceneBegan message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnCutsceneBegan(CutsceneBegan message)
            => CatchSitting(message);

        /// <summary>
        /// Processes the CutsceneEnded message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnCutsceneEnded(CutsceneEnded message)
        {
            base.OnCutsceneEnded(message);
            WatchCar();

            switch (message.CutsceneName)
            {
                case "cs7": case "cs10": PlayFinalScene(); break;
                case "cs11": WatchIcecreamMachine(); JumpToMariottisOffice(); break;
                case "cs12": JumpToVanillaCrunchGarage(); break;
                case "cs14": JumpToBelvedereStreet2(); break;
                case "cs15":
                case "cs16":
                case "cs17":
                case "cs18": WatchSweeneysRoom(); break;
                case "cs21": JumpToChristinesHall(); break;
                case "cs23": JumpToSweeneysHall(); break;
                case "cs35": QuitGame(); break;
            }
        }

        /// <summary>
        /// Checks if the NPC sits and sets appropriate fields.
        /// </summary>
        /// <param name="message"></param>
        private void CatchSitting(CutsceneBegan message)
        {
            switch (message.CutsceneName)
            {
                case "cs24": case "cs25": _sittingAtPubTable = true; break;
                case "snd12": _sittingOnChair = true; break;
            }
        }

        /// <summary>
        /// Measures time for the phone call countdown.
        /// </summary>
        private void CountPhone()
        {
            if (_phoneCountdown)
                _phoneDeltaTime += World.DeltaTime;
        }

        /// <summary>
        /// Returns a tile at the specified distance and direction.
        /// </summary>
        /// <param name="direction">The direction of the tile to be found</param>
        /// <param name="step">The distance between the NPC and the tile to be found</param>
        /// <returns>A reference to an tile that lays in the specified distance and direction</returns>
        private Tile GetNextTile(Orientation2D direction, int step = 1)
        {
            Plane target = new Plane(_area);
            target.Move(direction, step);
            return World.Map[target.Center];
        }

        /// <summary>
        /// Returns a tile at a given distance from the NPC in the direction of the NPC's current orientation.
        /// </summary>
        /// <param name="step">The distance between the NPC and the required tile</param>
        /// <returns>A reference to an tile that lays in the specified distance and direction</returns>
        /// <completionlist cref="PhysicsComponent.Orientation"/>
        private Tile GetNextTile(int step = 1)
            => GetNextTile(Orientation, step);

        /// <summary>
        /// Checks if Tuttle and Chipotle NPCs are in the same locality.
        /// </summary>
        /// <returns>True if Tuttle and Chipotle NPCs are in the same locality</returns>
        private bool IsTuttleNearBy()
=> _area.GetLocality().IsItHere(World.GetEntity("tuttle"));

        /// <summary>
        /// Chipotle and Tuttle NPCs relocate from the Walsch's drive way (příjezdoivá cesta w1)
        /// locality right outside the Christine's door.Christine's front door.
        /// </summary>
        private void JumpToBelvedereStreet2()
        {
            _phoneCountdown = true;
            Random r = new Random();
            _phoneInterval = r.Next(30000, 120000);
            _phoneDeltaTime = 0;
            SetPosition(1805, 1121, true);
        }

        /// <summary>
        /// The Detective Chipotle and Tuttle NPCs relocate from the Belvedere street (ulice p1)
        /// locality to the Christine's hall (hala p1) locality.
        /// </summary>
        private void JumpToChristinesHall()
            => SetPosition(1797, 1125, true);

        /// <summary>
        /// Relocates the NPC from the hall in Vanilla crunch company (hala v1) into the Mariotti's
        /// office (kancelář v1) locality.
        /// </summary>
        private void JumpToMariottisOffice()
            => SetPosition(2018, 1123, true);

        /// <summary>
        /// Chipotle and Tuttle NPCs relocate from the Easterby street (ulice p1) locality to the
        /// Sweeney's hall (hala s1) locality.
        /// </summary>
        private void JumpToSweeneysHall()
            => SetPosition(1405, 965, true);

        /// <summary>
        /// Relocates the NPC from the Mariotti's office (kancelář v1) into the garage of the
        /// vanilla crunch company (garáž v1) locality.
        /// </summary>
        private void JumpToVanillaCrunchGarage()
            => SetPosition(2006, 1166, true);

        /// <summary>
        /// Processes the ChipotlesCarMoved message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnChipotlesCarMoved(ChipotlesCarMoved message)
=> _carMovement = message;

        /// <summary>
        /// Processes the MakeStep message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnMakeStep(MakeStep message)
        {
            if (StandUp())
                return;

            // Get target coordinates
            Orientation2D finalOrientation = _orientation;

            if (message.Direction != TurnType.None)
                finalOrientation.Rotate(message.Direction);

            // Is the terrain occupable?
            Tile targetTile = GetNextTile(finalOrientation);
            if (targetTile == null)
                return;

            if (!targetTile.Permeable)
            {
                Owner.ReceiveMessage(new TerrainCollided(this, targetTile));
                return;
            }

            // Isn't an entity or object over there?
            if (targetTile.IsOccupied && targetTile.Object != Owner)
            {
                Owner.ReceiveMessage(new ObjectsCollided(this, targetTile));
                return;
            }

            // Isn't a closed door over there?
            if (targetTile.Passage != null && targetTile.Passage is Door)
            {
                Door door = targetTile.Passage as Door;
                if (door.Closed)
                {
                    Owner.ReceiveMessage(new DoorHit(this));
                    return;
                }
            }

            // The road is clear! Move!
            SetPosition(targetTile.Position);
            RecordLocality(targetTile.Locality);

            // Inform Tuttle
            if (!IsTuttleNearBy())
                Tuttle.ReceiveMessage(new EntityMoved(Owner, targetTile));

            WatchPuddle(targetTile.Position); // Check if player walked in a puddle
            WatchPhone();
        }

        /// <summary>
        /// Processes the SayLocality message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnSayLocality(SayLocality message)
=> Tolk.Speak(_area.GetLocality().Name.Friendly);

        /// <summary>
        /// Processes the SayNearestObject message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnSayNearestObject(SayNearestObject message)
        {
            Vector2 me = _area.Center;

            GameObject o =
                World.GetNearestObjects(me)
                .Where(obj => obj.Locality == _area.GetLocality())
                .FirstOrDefault();

            if (o == null)
            {
                Tolk.Speak("Nic tu není");
                return;
            }

            Vector2 point = o.Area.GetClosestPointTo(me);
            double x = point.X - me.X;
            double y = point.Y - me.Y;
            double z = Math.Round(Math.Sqrt(Math.Pow(x, 2) + Math.Pow(y, 2)));
            double r = Math.Atan2(y, x);
            Angle angle = new Angle(r) + Angle.FromCartesianDegrees(_orientation.Angle.CompassDegrees);
            double degrees = Math.Round(angle.CompassDegrees);

            string msg = o.Name.Friendly;
            if (degrees >= 315 || degrees < 45)
                msg += " před tebou";
            else if (degrees >= 45 && degrees < 135)
                msg += " napravo";
            else if (degrees >= 135 && degrees < 225)
                msg += " za tebou";
            else msg += " nalevo";

            Tolk.Speak(msg);
        }

        /// <summary>
        /// Processes the SayTerrain message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnSayTerrain(SayTerrain message)
            => Tolk.Speak(World.Map[_area.UpperLeftCorner].Terrain.GetDescription());

        /// <summary>
        /// Processes the TurnEntity message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnTurnEntity(TurnEntity message)
        {
            _orientation.Rotate(message.Degrees);
            Owner.ReceiveMessage(new TurnEntityResult(this, _orientation));
        }

        /// <summary>
        /// Processes the UseObject message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnUseObject(UseObject message)
        {
            // Detect door and use it if possible.
            IEnumerable<Tile> adjectingTiles = CuurrentTile.GetNeighbours8();
            Tile tileWithDoor = adjectingTiles.Where(t => t.Passage != null && t.Passage is Door).FirstOrDefault();
            if (tileWithDoor != null)
                tileWithDoor.Passage.ReceiveMessage(new UseObject(Owner, tileWithDoor));

            // Detect object in front of Chipotle.
            Tile tileInFront = GetNextTile();

            if (tileInFront.Object != null)
                tileInFront.Object.ReceiveMessage(new UseObject(Owner, tileInFront));
        }

        /// <summary>
        /// Plays the game finals.
        /// </summary>
        private void PlayFinalScene() => World.PlayCutscene(Owner, "cs35");

        /// <summary>
        /// Terminates the game and runs the main menu.
        /// </summary>
        private void QuitGame()
            => World.QuitGame();

        /// <summary>
        /// Records the specified locality as visited.
        /// </summary>
        /// <param name="locality">The locality to be recorded</param>
        private void RecordLocality(Locality locality)
        {
            if (!_visitedLocalities.Contains(locality))
                _visitedLocalities.Add(locality);
        }

        /// <summary>
        /// He puts the NPC on its feet if it is sitting and plays the appropriate sound.
        /// </summary>
        /// <returns>True on success</returns>
        private bool StandUp()
        {
            if (_sittingAtPubTable)
            {
                _sittingAtPubTable = false;
                World.PlayCutscene(Owner, IsTuttleNearBy() ? "cs27" : "cs26");
                return true;
            }

            if (_sittingOnChair)
            {
                _sittingOnChair = false;
                World.PlayCutscene(Owner, "snd13");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Relocates the NPC near the car of the Detective Chipotle NPC if the car has moved recently.
        /// </summary>
        private void WatchCar()
        {
            if (_carMovement == null)
                return;

            Vector2? target = _carMovement.Target.FindRandomWalkableTile(1);
            Assert(target.HasValue, "No walkable tile found.");
            SetPosition((Vector2)target, true);
            _carMovement = null;
        }

        /// <summary>
        /// Plays an appropriate cutscene if the icecream machine (automat v1) has been used recently.
        /// </summary>
        private void WatchIcecreamMachine()
        {
            if (World.GetObject("automat v1").Used)
                World.PlayCutscene(this, "cs13");
        }

        /// <summary>
        /// Makes the Easterby street (ulice s1) accessible if the Detective Chipotle has answered a
        /// phone call recently. and plays an appropriate cutscene
        /// </summary>
        private void WatchPhone()
        {
            if (_phoneCountdown && _phoneDeltaTime >= _phoneInterval)
            {
                _phoneCountdown = false;
                World.GetObject("detektivovo auto").ReceiveMessage(new UnblockLocality(Owner, World.GetLocality("ulice s1")));
                World.PlayCutscene(Owner, "cs22");
            }
        }

        /// <summary>
        /// Plays an appropriate cutscene if the Detective Chipotle NPC stepped into the puddle next
        /// to the Walsch's pool (bazén w1) object.
        /// </summary>
        /// <param name="point">
        /// Coordintes of a tile the Detective Chipotle NPC is gonna step on to.
        /// </param>
        private void WatchPuddle(Vector2 point)
        {
            if (_steppedIntoPuddle)
                return;

            Plane puddle = new Plane("937, 1081, 941, 1065");
            if (puddle.LaysOnPlane(point))
            {
                _steppedIntoPuddle = true;
                World.PlayCutscene(Owner, "cs2");
            }
        }

        /// <summary>
        /// Relocates the Chipotle's car (detektivovo auto) object to the afphalt road (asfaltka c1)
        /// locality and plays an appropriate cutscene if crutial objects in the sweeney's room
        /// (pokoj s1) has been used.
        /// </summary>
        /// <remarks>The Detective Chiptole and Tuttle NPCs should move with the car afterwards.</remarks>
        private void WatchSweeneysRoom()
        {
            if (
                 World.GetObject("trezor s1").Used
                && (World.GetObject("stůl s1").Used || World.GetObject("stůl s5").Used)
                && World.GetObject("počítač s1").Used
                && World.GetObject("mobil s1").Used
                )
            {
                Car.ReceiveMessage(new MoveChipotlesCar(Owner, AsphaltRoad));
                World.PlayCutscene(Owner, "cs19");
            }
        }
    }
}