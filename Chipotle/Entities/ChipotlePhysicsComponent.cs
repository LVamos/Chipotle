using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using System;
using System.Collections.Generic;
using System.Linq;

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


        private void OnTurnEntity(TurnEntity message)
        {
            _rotationStep = message.Degrees >= 0 ? 1 : -1;
            _plannedRotations = Math.Abs(message.Degrees);
        }

        protected override void OnCutsceneBegan(CutsceneBegan message) => CatchSitting(message);

        protected override void OnCutsceneEnded(CutsceneEnded message)
        {
            base.OnCutsceneEnded(message);

            switch(message.CutsceneName)
            {
                case "cs8": JumpToPub(); break; // Chipotle moves to pub and sits at table. Tuttle will do the same.
                case "cs7": case "cs10": PlayFinalScene(); break;
                case "cs35": QuitGame(); break;
            }
        }

        private void QuitGame()
        {
            // todo Implement ChiipotlePhysiicsComponent.QuitGame
            Environment.Exit(0);
        }
        private void JumpToPub()
            => SetPosition(1550, 1014, true); // Jumps just before pub.

        private void PlayFinalScene() => World.PlayCutscene(Owner, "cs35");

        private void CatchSitting(CutsceneBegan message)
        {
            switch (message.CutsceneName)
            {
                case "cs24": case "cs25": _sittingAtPubTable = true; break;
                case "snd12": _sittingOnChair = true; break;
            }
        }

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

            Plane target = new Plane(_area);
            target.Move(finalOrientation, 1);

            // Is the terrain occupable?
            Tile targetTile = World.Map[target.Center] ?? throw new InvalidOperationException($"{nameof(OnMakeStep)}: empty tile."); // Null test

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
            SetPosition(target);
            RecordLocality(targetTile.Locality);

            // Check if player walked in a puddle
            WatchPuddle(targetTile.Position);
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
