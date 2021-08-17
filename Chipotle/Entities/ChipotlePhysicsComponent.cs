using System;
using System.Collections.Generic;
using System.Linq;

using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

namespace Game.Entities
{
    public class ChipotlePhysicsComponent : PhysicsComponent
    {
        private bool _sittingAtPubTable;

        private HashSet<Locality> _visitedLocalities = new HashSet<Locality>();

        public override void Update()
        {
            base.Update();
            UpdateRotation();
            CountPhone();
        }

        private void CountPhone()
        {
            if (_phoneCountdown)
                _phoneDeltaTime += World.DeltaTime;
        }

        private void UpdateRotation()
        {
            if (_plannedRotations > 0)
            {
                _orientation.Rotate(_rotationStep);
                _plannedRotations--;

                if (_plannedRotations == 0)
                {
                    Owner.ReceiveMessage(new TurnEntityResult(this, _orientation));
                }
            }
        }

        private int _rotationStep;
        private int _plannedRotations;
        private bool _steppedIntoPuddle;
        private bool _sittingOnChair;
        private bool _phoneCountdown;
        private int _phoneDeltaTime;
        private int _phoneInterval;

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

        private void OnChipotlesCarMoved(ChipotlesCarMoved message)
=> _carMovement = message;

        private void OnSayNearestObject(SayNearestObject m)
        {
            GameObject o = World.GetNearestObjects(_area.UpperLeftCorner).Where(obj => obj.Locality == _area.GetLocality()).FirstOrDefault();
            if (o == null)
            {
                Say("Nic tu není");
                return;
            }
            string msg = o.Name.Friendly;

            if (o.Area.LowerRightCorner.Y > _area.Center.Y)
            {
                msg += " před tebou";
            }
            else if (o.Area.UpperRightCorner.Y < _area.Center.Y)
            {
                msg += " za tebou";
            }
            else if (o.Area.LowerRightCorner.Y <= _area.Center.Y && o.Area.UpperRightCorner.Y >= _area.Center.Y)
            {
                if (o.Area.UpperRightCorner.X < _area.Center.X)
                {
                    msg += " vlevo";
                }
                else
                {
                    msg += " vpravo";
                }
            }

            Say(msg);
        }

        private void OnSayLocality(SayLocality m)
=> SayDelegate(_area.GetLocality().Name.Friendly);

        private void OnSayTerrain(SayTerrain message)
            => SayDelegate(World.Map[_area.UpperLeftCorner].Terrain.GetDescription());

        private void OnUseObject(UseObject message)
        {
            IEnumerable<Tile> tiles = World.Map[_area.Center].GetNeighbours8();
            Tile tile = tiles.FirstOrDefault(t => t.Passage is Door || t.Object != null);

            if (tile == null)
                return;

            UseObject newMessage = new UseObject(this, tile);
            if (tile.Passage != null)
            {
                tile.Passage.ReceiveMessage(newMessage);
                return;
            }
            tile.Object.ReceiveMessage(newMessage);
        }

        private void WatchSweeneysRoom()
        {
            if (
                 World.GetObject("trezor s1").Used
                && (World.GetObject("stůl s1").Used || World.GetObject("stůl s5").Used)
                && World.GetObject("počítač s1").Used
                && World.GetObject("mobil s1").Used
                )
            {
                World.PlayCutscene(Owner, "cs19");
                Car.ReceiveMessage(new MoveChipotlesCar(Owner, AsphaltRoad));
            }
        }
        private Locality AsphaltRoad => World.GetLocality("asfaltka c1");
        private ChipotlesCar Car => World.GetObject("detektivovo auto") as ChipotlesCar;


        private void OnTurnEntity(TurnEntity message)
        {
            _rotationStep = message.Degrees >= 0 ? 1 : -1;
            _plannedRotations = Math.Abs(message.Degrees);
        }

        protected override void OnCutsceneBegan(CutsceneBegan message) => CatchSitting(message);

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

        private ChipotlesCarMoved _carMovement;

        private void WatchCar()
        {
            if (_carMovement == null)
                return;

            Vector2? target = _carMovement.TargetLocation.FindRandomWalkableTile(1);
            Assert(target.HasValue, "No walkable tile found.");
            SetPosition((Vector2)target, true);
            _carMovement = null;
        }


        private void JumpToMariottisOffice()
            => SetPosition(2018, 1123, true);

        private void WatchIcecreamMachine()
        {
            if (World.GetObject("automat v1").Used)
                World.PlayCutscene(this, "cs13");
        }

        private void JumpToVanillaCrunchGarage()
            => SetPosition(2006, 1166, true);

        private void JumpToSweeneysHall()
            => SetPosition(1405, 965, true);

        private void JumpToChristinesHall()
            => SetPosition(1797, 1125, true);


        /// <summary>
        /// Chipotle and Tuttle get out to Belvedere street right in front of Christine's front door.
        /// </summary>
        private void JumpToBelvedereStreet2()
        {
            _phoneCountdown = true;
            Random r = new Random();
            _phoneInterval = r.Next(30000, 120000);
            _phoneDeltaTime = 0;
            SetPosition(1805, 1121, true);
        }

        private void QuitGame() =>
            Environment.Exit(0);


        private void PlayFinalScene() => World.PlayCutscene(Owner, "cs35");

        private void CatchSitting(CutsceneBegan message)
        {
            switch (message.CutsceneName)
            {
                case "cs24": case "cs25": _sittingAtPubTable = true; break;
                case "snd12": _sittingOnChair = true; break;
            }
        }

        private Tile GetNextTile(Orientation2D orientation, int step = 1)
        {
            Plane target = new Plane(_area);
            target.Move(orientation, step);
            return World.Map[target.Center];
        }

        private Tile GetNextTile(int step = 1)
            => GetNextTile(Orientation, step);

        private void OnMakeStep(MakeStep message)
        {
            if (StandUp())
                return;

            // Get target coordinates
            Orientation2D finalOrientation = _orientation;

            if (message.Direction != TurnType.None)
            {
                finalOrientation.Rotate(message.Direction);
            }

            // Is the terrain occupable?
            Tile targetTile = GetNextTile(finalOrientation) ?? throw new InvalidOperationException($"{nameof(OnMakeStep)}: empty tile."); // Null test

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

            WatchPuddle(targetTile.Position); // Check if player walked in a puddle
            WatchPhone();
        }

        private void WatchPhone()
        {
            if (_phoneCountdown && _phoneDeltaTime >= _phoneInterval)
            {
                _phoneCountdown = false;
                World.GetObject("detektivovo auto").ReceiveMessage(new UnblockLocality(Owner, World.GetLocality("ulice s1")));
                World.PlayCutscene(Owner, "cs22");
            }
        }

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

        private bool IsTuttleNearBy()
=> _area.GetLocality().IsItHere(World.GetEntity("tuttle"));

        private void RecordLocality(Locality locality)
        {
            if (!_visitedLocalities.Contains(locality))
                _visitedLocalities.Add(locality);
        }

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
    }


}
