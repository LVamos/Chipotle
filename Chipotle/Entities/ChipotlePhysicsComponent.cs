using System;
using System.Collections.Generic;
using System.Linq;

using DavyKager;

using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using OpenTK;

namespace Game.Entities
{
    /// <summary>
    /// Controls movement of the Detective Chipotle NPC.
    /// </summary>
    [Serializable]
    public class ChipotlePhysicsComponent : PhysicsComponent
    {
        /// <summary>
        /// Specifies if the NPC can walk.
        /// </summary>
        protected bool _blockWalk;

        /// <summary>
        /// Time interval for backward walk
        /// </summary>
        private const int _backwardWalkSpeed = 20;

        /// <summary>
        /// Time interval for forward walk
        /// </summary>
        private const int _forwardWalkSpeed = 15;

        /// <summary>
        /// Determines maximum time between particullar steps.
        /// </summary>
        private const int _maxTimeBetweenSteps = _sideWalkSpeed;

        /// <summary>
        /// Time interval for walk to the side
        /// </summary>
        private const int _sideWalkSpeed = 24;

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
        /// Indicates if the player released keys after a step to the side was performed.
        /// </summary>
        private bool _sideStepInProgress;

        /// <summary>
        /// Indicates if the NPC is sitting at a table in the pub (výčep h1) locality.
        /// </summary>
        private bool _sittingAtPubTable;

        /// <summary>
        /// Indicates if the NPC is sitting on a chair.
        /// </summary>
        private bool _sittingOnChair;

        /// <summary>
        /// stores information about walk.
        /// </summary>
        [NonSerialized]
        private StartWalk _startWalkMessage;

        /// <summary>
        /// Indicates if the NPC stepped into the puddle in the pool (bazén w1) locality.
        /// </summary>
        private bool _steppedIntoPuddle;

        /// <summary>
        /// Counts time from the last step.
        /// </summary>
        private int _timeFromLastStep;

        /// <summary>
        /// Indicates if the Chipotle NPC is currently walking.
        /// </summary>
        private bool _walking;

        /// <summary>
        /// Sets time interval between steps.
        /// </summary>
        private int _walkSpeed;

        /// <summary>
        /// Measures time from last step.
        /// </summary>
        private int _walkTimer;

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
        private (Vector2 position, Tile tile) CurrentTile
            => (_area.Center, World.Map[_area.Center]);

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
                    [typeof(SayExits)] = (message) => OnSayExits((SayExits)message),
                    [typeof(StopWalk)] = (message) => OnStopWalk((StopWalk)message),
                    [typeof(SayTerrain)] = (message) => OnSayTerrain((SayTerrain)message),
                    [typeof(SayVisitedLocality)] = (message) => OnSayVisitedLocality((SayVisitedLocality)message),
                    [typeof(ChipotlesCarMoved)] = (message) => OnChipotlesCarMoved((ChipotlesCarMoved)message),
                    [typeof(CutsceneBegan)] = (m) => OnCutsceneBegan((CutsceneBegan)m),
                    [typeof(CutsceneEnded)] = (message) => OnCutsceneEnded((CutsceneEnded)message),
                    [typeof(SaySurroundingObjects)] = (m) => OnSaySurroundingObjects((SaySurroundingObjects)m),
                    [typeof(SayLocality)] = (m) => OnSayLocality((SayLocality)m),
                    [typeof(StartWalk)] = (m) => OnStartWalk((StartWalk)m),
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
            PerformWalk();
        }

        /// <summary>
        /// Processes the CutsceneBegan message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnCutsceneBegan(CutsceneBegan message)
            => CatchSitting(message);

        protected void SaveGame()
        {
            int temp = _currentRegion;
            bool temp2 = _inVisitedRegion;
            _currentRegion = -1;
            _inVisitedRegion = true;
            World.SaveGame();
            _currentRegion = temp;
            _inVisitedRegion = temp2;
        }

        /// <summary>
        /// Processes the CutsceneEnded message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnCutsceneEnded(CutsceneEnded message)
        {
            base.OnCutsceneEnded(message);
            SaveGame();
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
        /// Processes the SayExits message.
        /// </summary>
        /// <param name="message">The message</param>
        protected void OnSayExits(SayExits message)
        {
            Vector2 me = _area.Center;

            IEnumerable<Passage> exits = _locality.Passages.OrderBy(e => e.Area.GetDistanceFrom(me));

            IEnumerable<(string description, double compassDegrees)> exitInfo = exits
                .Select(e => (e.AnotherLocality(_locality).To, GetAngle(e.Area.GetClosestPointTo(me))));

            SayExits newMessage = exitInfo.IsNullOrEmpty() ? new SayExits(this, true) : new SayExits(this, exitInfo);
            Owner.ReceiveMessage(newMessage);
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
        private (Vector2 position, Tile tile) GetNextTile(Orientation2D direction, int step = 1)
        {
            Plane target = new Plane(_area);
            target.Move(direction, step);
            return (target.Center, World.Map[target.Center]);
        }

        /// <summary>
        /// Returns a tile at a given distance from the NPC in the direction of the NPC's current orientation.
        /// </summary>
        /// <param name="step">The distance between the NPC and the required tile</param>
        /// <returns>A reference to an tile that lays in the specified distance and direction</returns>
        /// <see cref="PhysicsComponent.Orientation"/>
        private (Vector2 position, Tile tile) GetNextTile(int step = 1)
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
        /// Stores indexes of regions visited by the NPC.
        /// </summary>
        /// <remarks>Each locality is divided into regions of same size specified by the _motionTrackRadius field. Index of a region define distance from the top left region of the current locality (MotionTrackWidth *row +column). Last row and last column can be smaller.</remarks>
        protected readonly Dictionary<Locality, HashSet<int>> _motionTrack = new Dictionary<Locality, HashSet<int>>();

        /// <summary>
        /// Computes amount of horizontal motion track regions for the current locality.
        /// </summary>
        protected int MotionTrackWidth => (int)(_locality.Area.Width/ _motionTrackRadius + (_locality.Area.Width % _motionTrackRadius > 0 ? 1 : 0));

        /// <summary>
        /// Computes amount of vertical motion track regions for the current locality.
        /// </summary>
        protected int MotionTrackHeight => (int)(_locality.Area.Height/ _motionTrackRadius + (_locality.Area.Height% _motionTrackRadius > 0 ? 1 : 0));

        protected int GetRegionIndex(Vector2 point)
        {
            Vector2 relative = Plane.GetRelativeCoordinates(point);

            int rX = (int)(relative.X / _motionTrackRadius + (relative.X % _motionTrackRadius > 0 ? 1 : 0));
            int rY = (int)(relative.Y / _motionTrackRadius + (relative.Y % _motionTrackRadius > 0 ? 1 : 0));
            return rX + rY;
        }

        /// <summary>
        /// Indicates if the NPC stands in a previously visited region.
        /// </summary>
        protected bool _inVisitedRegion;

        /// <summary>
        /// Specifies radius of surroundings around the NPC in steps used in motion tracking.
        /// </summary>
        protected const int _motionTrackRadius = 10;

        /// <summary>
        /// Stores index of current motion track region.
        /// </summary>
        protected int _currentRegion;

        /// <summary>
        /// Marks current nearest surroundings of the NPC as visited. If it's already been visited then the _inVisitedRegion is set to true.
        /// </summary>
        /// <param name="point">Current position of the NPC</param>
        protected void RecordRegion(Vector2 point)
        {
            if (!_motionTrack.ContainsKey(_locality))
                _motionTrack[_locality] = new HashSet<int>();

            int regionIndex = GetRegionIndex(point);

            if (_currentRegion != regionIndex)
            {
                _currentRegion = regionIndex;
                _inVisitedRegion = _motionTrack[_locality].Contains(regionIndex);

                if (!_inVisitedRegion)
                    _motionTrack[_locality].Add(regionIndex);
            }
        }

        /// <summary>
        /// Performs one step in direction specified in the _startWalkMessage field.
        /// </summary>
        private void MakeStep()
        {
            if (!_sideStepInProgress && !_walking)
                return;

            StartWalk message = _startWalkMessage
                ?? throw new ArgumentNullException(nameof(_startWalkMessage));

            // Get target coordinates
            Orientation2D finalOrientation = _orientation;

            if (message.Direction != TurnType.None)
                finalOrientation.Rotate(message.Direction);

            // Is the terrain occupable?
            (Vector2 position, Tile tile) targetTile = GetNextTile(finalOrientation);
            if (targetTile.tile == null)
                return;

            if (!targetTile.tile.Permeable)
            {
                _walking = false;
                _startWalkMessage = null;
                _sideStepInProgress = false;
                Owner.ReceiveMessage(new TerrainCollided(this, targetTile.position, targetTile.tile));
                return;
            }

            // Isn't an entity or object over there?
            GameObject o = World.GetObject(targetTile.position);
            if (o != null && o != Owner)
            {
                _walking = false;
                Owner.ReceiveMessage(new ObjectsCollided(this, o, targetTile.position, targetTile.tile));
                return;
            }

            // Isn't a closed door over there?
            Passage passage = World.GetPassage(targetTile.position);
            if (passage != null && passage is Door door && door.Closed)
            {
                _walking = false;
                Owner.ReceiveMessage(new DoorHit(this));
                return;
            }

            // The road is clear! Move!
            Locality sourceLocality = _area.GetLocality();
            Locality targetLocality = World.GetLocality(targetTile.position);
            SetPosition(targetTile.position);
            RecordRegion(targetTile.position);
            _timeFromLastStep = 0;

            if (sourceLocality != null && sourceLocality != targetLocality)
                SaveGame();

            // Inform Tuttle
            if (!IsTuttleNearBy())
                Tuttle.ReceiveMessage(new EntityMoved(Owner, targetTile.position));

            WatchPuddle(targetTile.position); // Check if player walked in a puddle
            WatchPhone();
        }

        /// <summary>
        /// Processes the ChipotlesCarMoved message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnChipotlesCarMoved(ChipotlesCarMoved message)
=> _carMovement = message;

        /// <summary>
        /// Processes the SayLocality message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnSayLocality(SayLocality message)
=> Tolk.Speak(_area.GetLocality().Name.Friendly);

        /// <summary>
        /// Enumerates all objects from current locality in the specified radius around the NPC.
        /// </summary>
        /// <param name="radius">Max radius around the NPC</param>
        /// <returns>Enumeration of dump objects</returns>
        protected IEnumerable<(DumpObject o, double compassDegrees)> GetNavigableObjects(int radius)
        {
            Vector2 me = _area.Center;
            IEnumerable<DumpObject> nearestObjects = _locality.GetSurroundingObjects(me, radius);

            return
                from o in nearestObjects
                select ((o, GetAngle(o.Area.GetClosestPointTo(me))));
        }

        /// <summary>
        /// Processes the SayNearestObject message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnSaySurroundingObjects(SaySurroundingObjects message)
            => Owner.ReceiveMessage(new SaySurroundingObjectsResult(this, GetNavigableObjects(20)));

        /// <summary>
        /// Processes the SayTerrain message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnSayTerrain(SayTerrain message)
            => Tolk.Speak(World.Map[_area.UpperLeftCorner].Terrain.GetDescription());

        /// <summary>
        /// Processes the SayVisitedLocality message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnSayVisitedLocality(SayVisitedLocality message)
            => Owner.ReceiveMessage(new SayVisitedLocalityResult(this, _inVisitedRegion));

        /// <summary>
        /// Processes the MakeStep message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnStartWalk(StartWalk message)
        {
            if (_blockWalk || StandUp())
                return;

            _blockWalk = true;
            _startWalkMessage = message;

            // Allow only single steps when walking to the side.
            if (message.Direction == TurnType.SharplyLeft || message.Direction == TurnType.SharplyRight)
            {
                if (!_sideStepInProgress && _timeFromLastStep >= _sideWalkSpeed)
                {
                    _sideStepInProgress = true;
                    MakeStep();
                }

                return;
            }

            // Set walk speed
            int walkSpeed;
            switch (message.Direction)
            {
                case TurnType.SharplyLeft: case TurnType.SharplyRight: walkSpeed = _sideWalkSpeed; break;
                case TurnType.Around: walkSpeed = _backwardWalkSpeed; break;
                default: walkSpeed = _forwardWalkSpeed; break;
            }

            if (_timeFromLastStep < walkSpeed)
                return;

            _walkSpeed = walkSpeed;
            _walking = true;
            _walkTimer = 0;
            _walking = true;
            MakeStep();
        }

        /// <summary>
        /// Processes the StopWalk message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnStopWalk(StopWalk message)
        {
            _blockWalk = false;
            _walking = false;
            _sideStepInProgress = false;
            _startWalkMessage = null;
        }

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
            foreach ((Vector2 position, Tile tile) t in World.Map.GetNeighbours8(CurrentTile.position))
            {
                Passage passage = World.GetPassage(t.position);

                if (passage != null && passage is Door door)
                    door.ReceiveMessage(new UseObject(Owner, t.position, t.tile));
            }

            // Detect object in front of Chipotle.
            (Vector2 position, Tile tile) tileInFront = GetNextTile();

            if (tileInFront.tile != null)
            {
                GameObject obj = World.GetObject(tileInFront.position);
                if (obj != null)
                {
                    UseObject useObject = new UseObject(Owner, tileInFront.position, tileInFront.tile);
                    obj.ReceiveMessage(useObject);
                }
            }
        }

        /// <summary>
        /// Controlls walk of the Chipotle NPC.
        /// </summary>
        private void PerformWalk()
        {
            if (_walking && _walkTimer >= _walkSpeed)
            {
                _walkTimer = 0;
                MakeStep();
            }
            _walkTimer++;

            if (_timeFromLastStep < _maxTimeBetweenSteps)
                _timeFromLastStep++;
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