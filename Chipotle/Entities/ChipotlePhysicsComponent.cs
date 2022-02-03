using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Forms;

using DavyKager;

using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;
using Game.UI;

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
        /// Returns a tile at a given distance from the NPC in the direction of the NPC's current orientation.
        /// </summary>
        /// <param name="step">The distance between the NPC and the required tile</param>
        /// <returns>A reference to an tile that lays in the specified distance and direction</returns>
        /// <see cref="PhysicsComponent.Orientation"/>
        protected override Vector2 GetNextTile()
        {
            Orientation2D finalOrientation = _orientation;
            if (_startWalkMessage.Direction != TurnType.None)
                finalOrientation.Rotate(_startWalkMessage.Direction);
            return GetNextTile(finalOrientation, 1).position;
        }

        /// <summary>
        /// Immediately changes position of the NPC.
        /// </summary>
        /// <param name="target">Coordinates of the target position</param>
        /// <param name="silently">Specifies if the NPC plays sounds of walk.</param>
        protected override void Move(Vector2 target, bool silently = false)
        {
                Locality sourceLocality = _area?.GetLocality();
            Locality targetLocality = World.GetLocality(target);
            base.Move(target, silently);

            RecordRegion(target);

            if (sourceLocality != null && sourceLocality != targetLocality)
                SaveGame();

            // Watch important events
            WatchPuddle(target); // Check if player walked in a puddle
            WatchPhone();
        }


        /// <summary>
        /// Constructor
        /// </summary>
        public ChipotlePhysicsComponent() : base()
        {
            // set initial position.&
            if (Program.TestMode && File.Exists("initpos.txt"))
              StartPosition = (Vector2?) new Plane(File.ReadAllText("initpos.txt")).Center;
            else
                StartPosition = (Vector2?)(new Vector2(1028, 1034));

        }

        /// <summary>
        /// Specifies if the NPC can walk.
        /// </summary>
        protected bool _blockWalk;

        /// <summary>
        /// Time interval for backward walk
        /// </summary>
        private const float _backwardSpeed = 2;

        /// <summary>
        /// Time interval for forward walk
        /// </summary>
        private const int _forwardWalkSpeed = 15;

        /// <summary>
        /// Time interval for walk to the side
        /// </summary>
        private const float _sideSpeed = 1.6f;

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
        /// stores information about walk.
        /// </summary>
        [NonSerialized]
        private StartWalk _startWalkMessage;

        /// <summary>
        /// Indicates if the NPC stepped into the puddle in the pool (bazén w1) locality.
        /// </summary>
        private bool _steppedIntoPuddle;

        /// <summary>
        /// Indicates if the Chipotle NPC is currently walking.
        /// </summary>
        private bool _walking;

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
        /// Returns reference to the Tuttle NPC.
        /// </summary>
        private Entity Tuttle
            => World.GetEntity("tuttle");

        /// <summary>
        /// Initializes the component and starts its message loop.
        /// </summary>
        public override void Start()
        {
            Move((Vector2)StartPosition, true);
                _orientation = new Orientation2D(0, 1);
            Owner.ReceiveMessage(new OrientationChanged(this, _orientation, _orientation, TurnType.None, true));
            base.Start();

            RegisterMessages(
                new Dictionary<Type, Action<GameMessage>>()
                {
                    [typeof(ExitNavigationStopped)] = (message) => OnExitNavigationStopped((ExitNavigationStopped)message),
                    [typeof(ListExits)] = (message) => OnListExits((ListExits)message),
                    [typeof(ObjectNavigationStopped)] = (message) => OnObjectNavigationStopped((ObjectNavigationStopped)message),
                    [typeof(StopObjectNavigation)] = (message) => OnStopObjectNavigation((StopObjectNavigation)message),
                    [typeof(ListObjects)] = (message) => OnListObjects((ListObjects)message),
                    [typeof(SayExits)] = (message) => OnSayExits((SayExits)message),
                    [typeof(StopWalk)] = (message) => OnStopWalk((StopWalk)message),
                    [typeof(SayTerrain)] = (message) => OnSayTerrain((SayTerrain)message),
                    [typeof(SayVisitedLocality)] = (message) => OnSayVisitedLocality((SayVisitedLocality)message),
                    [typeof(ChipotlesCarMoved)] = (message) => OnChipotlesCarMoved((ChipotlesCarMoved)message),
                    [typeof(CutsceneBegan)] = (m) => OnCutsceneBegan((CutsceneBegan)m),
                    [typeof(CutsceneEnded)] = (message) => OnCutsceneEnded((CutsceneEnded)message),
                    [typeof(SayObjects)] = (m) => OnSayObjects((SayObjects)m),
                    [typeof(SayLocality)] = (m) => OnSayLocality((SayLocality)m),
                    [typeof(StartWalk)] = (m) => OnStartWalk((StartWalk)m),
                    [typeof(ChangeOrientation)] = (message) => OnChangeOrientation((ChangeOrientation)message),
                    [typeof(UseObject)] = (message) => OnUseObject((UseObject)message)
                }
                );
        }

        /// <summary>
        /// Processes the ExitNavigationStopped message.
        /// </summary>
        /// <param name="message">source of the message</param>
        private void OnExitNavigationStopped(ExitNavigationStopped message)
        {
            if(message.Sender == _navigatedExit)
_navigatedExit = null;
        }

        /// <summary>
        /// Processes the ListExits message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnListExits(ListExits message)
        {
            StopNavigation(); // If there's any navigation in progress, it'll be stopped and this command will be cancelled.
            if (NavigationInProgress)
                return;

            (string[] descriptions, Passage[] exits) result = GetNavigableExits();

            if (result.descriptions.IsNullOrEmpty()) // No objects near by
            {
                Owner.ReceiveMessage(new SayExitsResult(this));
                return;
            }

            // Run the menu
            int option = WindowHandler.Menu(result.descriptions, "Východy", true);
            if (option == -1)
                return;

            Passage target = result.exits[option];
            if (_navigatedExit != null && _navigatedExit != target)
                StopNavigation();

            _navigatedExit = target;
            _navigatedExit.ReceiveMessage(new StartExitNavigation(Owner));
        }

        /// <summary>
        /// An exit to which the NPC is navigated.
        /// </summary>
        protected Passage _navigatedExit;
        /// <summary>
        /// Processes the ObjectNavigationStopped message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnObjectNavigationStopped(ObjectNavigationStopped message)
        {
            if (message.Sender == _navigatedObject)
                _navigatedObject = null;
        }

        /// <summary>
        /// Processes the StopObjectNavigation message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnStopObjectNavigation(StopObjectNavigation message) 
            => StopNavigation();

        /// <summary>
        /// Specifies max radius for navigable objects enumeration.
        /// </summary>
        protected const int _navigableObjectsRadius = 50;

        /// <summary>
        /// Processes the ListNavigableObjects message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnListObjects(ListObjects message)
        {
            StopNavigation(); // If there's any navigation in progress, it'll be stopped and this command will be cancelled.
            if (NavigationInProgress)
                return;

            (string[] descriptions, DumpObject[] objects) objects = GetNavigableObjects();

            if (objects.objects.IsNullOrEmpty())
            {
                Owner.ReceiveMessage(new SayObjectsResult(this, null));
                return;
            }

            int option = WindowHandler.Menu(objects.descriptions, "Okolní objekty");

            if (option == -1)
                return;

            DumpObject target = objects.objects[option];

            //if (_navigatedObject!=null && target != _navigatedObject)
            //    StopNavigation();

            _navigatedObject = target;
            _navigatedObject.ReceiveMessage(new StartObjectNavigation (Owner));
        }

        /// <summary>
        /// Stops ongoing object navigation.
        /// </summary>
        private void StopNavigation()
            {
            if (_navigatedObject != null)
                _navigatedObject.ReceiveMessage(new StopObjectNavigation(Owner));

            if (_navigatedExit!= null)
                _navigatedExit.ReceiveMessage(new StopExitNavigation(Owner));
        }

        /// <summary>
        /// Checks if there's any navigation in progress.
        /// </summary>
        protected bool NavigationInProgress 
            => _navigatedExit != null || _navigatedObject != null;

        /// <summary>
        /// Objectt to which tthe NPC is currently navigated.
        /// </summary>
        protected DumpObject _navigatedObject;

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

            if(Program.TestMode)
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
            // If there's any navigation in progress, it'll be stopped and this command will be cancelled.
            StopNavigation();
            if (NavigationInProgress)
                return;

            // If the player stands in a passage inform him.
            Passage occupiedPassage = World.GetPassage(_area.Center);
            if (occupiedPassage != null)
                Owner.ReceiveMessage(new SayExitsResult(this, occupiedPassage));
            else Owner.ReceiveMessage(new SayExitsResult(this, GetNavigableExits().descriptions));
        }

        /// <summary>
        /// Generates a text representation of the specified distance in Czech.
        /// </summary>
        /// <param name="distance">The distance in meters to be described</param>
        /// <returns>The text representation of the specified distance</returns>
        private string GetDistanceDescription(int distance)
        {
            string d = distance.ToString();

            if (distance == 1)
                return string.Empty;
            if (distance >= 2 && distance <= 4)
                return $" {d} metry ";
            return $" {d} metrů ";
        }

        /// <summary>
        /// Returns text descriptions of the specified exits including distance and position.
        /// </summary>
        /// <returns>A string array</returns>
        private (string[] descriptions, Passage[] exits) GetNavigableExits()
        {
Passage[] exits = _locality.GetNearestExits(_area.Center, _exitRadius).ToArray<Passage>();

            Vector2 me = _area.Center;
            string[] descriptions =
    (
    from e in exits
    let point = e.Area.GetClosestPoint(_area.Center)
    let distance = GetDistanceDescription((int)World.GetDistance(me, point))
    let type = e is Door ? "dveře " : "průchod "
    let to = e.AnotherLocality(_locality).To + " "
    let angle = Angle.GetDescription(GetAngle(point))
    select ($"{type} {to}: {distance} {angle}:")
    ).ToArray<string>();

            return (descriptions, exits);
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
            Move(1805, 1121, true);
        }

        /// <summary>
        /// The Detective Chipotle and Tuttle NPCs relocate from the Belvedere street (ulice p1)
        /// locality to the Christine's hall (hala p1) locality.
        /// </summary>
        private void JumpToChristinesHall()
            => Move(1797, 1125, true);

        /// <summary>
        /// Relocates the NPC from the hall in Vanilla crunch company (hala v1) into the Mariotti's
        /// office (kancelář v1) locality.
        /// </summary>
        private void JumpToMariottisOffice()
            => Move(2018, 1123, true);

        /// <summary>
        /// Chipotle and Tuttle NPCs relocate from the Easterby street (ulice p1) locality to the
        /// Sweeney's hall (hala s1) locality.
        /// </summary>
        private void JumpToSweeneysHall()
            => Move(1405, 965, true);

        /// <summary>
        /// Relocates the NPC from the Mariotti's office (kancelář v1) into the garage of the
        /// vanilla crunch company (garáž v1) locality.
        /// </summary>
        private void JumpToVanillaCrunchGarage()
            => Move(2006, 1166, true);

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
        /// Specifies distance in which exits from current locality are searched.
        /// </summary>
        protected const int _exitRadius = 80;

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
        protected override void MakeStep()
        {
            // Speed limitation
            if (_walkTimer < _speed)
                return;

            base.MakeStep();

            Vector2 target = GetNextTile(); // Get target coordinates

            // Move if the terrain is occupable.
            if (!SolveObstacle(target))
            Move(target);
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
=> Tolk.Speak(_area.GetLocality().Name.Friendly, true);

        /// <summary>
        /// Returns information about all navigable objects from current locality in the specified radius around the NPC.
        /// </summary>
        /// <returns>Tuple with an object list and text descriptions including distance and position of each object</returns>
        protected (string[] descriptions, DumpObject[] objects) GetNavigableObjects()
        {
            Vector2 me = _area.Center;
            DumpObject[] objects = _locality.GetSurroundingObjects(me, _navigableObjectsRadius).ToArray<DumpObject>();

            string[] descriptions =
                (
                from o in objects
                let point = o.Area.GetClosestPoint(me)
                let name = o.Name.Friendly
                let distance = GetDistanceDescription((int)World.GetDistance(me, point))
                let angle = Angle.GetDescription(GetAngle(point))
                select ($"{name}: {distance} {angle}: ")
                ).ToArray<string>();

            return (descriptions, objects);
        }

        /// <summary>
        /// Processes the SayNearestObject message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnSayObjects(SayObjects message)
        {
            // If there's any navigation in progress, it'll be stopped and this command will be cancelled.
            StopNavigation();
            if (!NavigationInProgress)
            Owner.ReceiveMessage(new SayObjectsResult(this, GetNavigableObjects().descriptions));
        }

        /// <summary>
        /// Processes the SayTerrain message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnSayTerrain(SayTerrain message)
            => Tolk.Speak(World.Map[_area.UpperLeftCorner].Terrain.GetDescription(), true);

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
            _startWalkMessage = null;
        }

        /// <summary>
        /// Processes the TurnEntity message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        private void OnChangeOrientation(ChangeOrientation message)
        {
            Orientation2D source = _orientation;
            _orientation.Rotate(message.Degrees);
            Owner.ReceiveMessage(new OrientationChanged(this, source, _orientation, message.Direction));
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
                {
                    door.ReceiveMessage(new UseObject(Owner, t.position, t.tile));
                    break;
                }
            }

            // Detect object in front of Chipotle.
            (Vector2 position, Tile tile) tileInFront = GetNextTile(1);

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
        protected override void PerformWalk()
        {
            base.PerformWalk();

            if (_walking && _walkTimer >= _speed)
                MakeStep();
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
            Move((Vector2)target, true);
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

        /// <summary>
        /// Computes length of the next step of the NPC.
        /// </summary>
        /// <param name="goal">Coordinates of the goal of an ongoing movement of the NPC</param>
        protected override int GetSpeed()
        {
            float coefficient = 1;
              if (_startWalkMessage.Direction == TurnType.SharplyLeft || _startWalkMessage.Direction == TurnType.SharplyRight)
                coefficient = _sideSpeed;
                else if(_startWalkMessage.Direction == TurnType.Around)
                coefficient = _backwardSpeed;

            return (int) (GetTerrainSpeed() * coefficient);
        }

        /// <summary>
        /// Solves a situation when the NPC encounters an obstacle.
        /// </summary>
        /// <param name="obstacle">Nearest point of the obstacle</param>
        protected override bool SolveObstacle(Vector2 obstacle)
        {
            Tile t = World.Map[obstacle];
            if (t == null) // Off the map
                return true;

            GameObject o = World.GetObject(obstacle);
            Passage p = World.GetPassage(obstacle);

            if (!t.Walkable)
                Owner.ReceiveMessage(new TerrainCollided(this, obstacle));
            else if (o != null && o != Owner)
                Owner.ReceiveMessage(new ObjectsCollided(this, o, obstacle));
            else if (p != null && p.State == PassageState.Closed)
            {
                p.ReceiveMessage(new DoorHit(Owner, p as Door, obstacle));
                Owner.ReceiveMessage(new DoorHit(this, p as Door, obstacle));
            }
            else return false;

            _walking = false;
            _startWalkMessage = null;
            return true;
        }
    }
}