
using Game.Entities;
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
            {
                return;
            }

            UseObject m = new UseObject(Owner, tile);
            if (tile.Object != null)
            {       
                tile.Object.ReceiveMessage(m);
            }
            else
            {
                tile.Passage.ReceiveMessage(m);
            }
        }

        private void OnTurnEntity(TurnEntity message)
        {
            _rotationStep = message.Degrees >= 0 ? 1 : -1;
            _plannedRotations = Math.Abs(message.Degrees);
        }

        private void OnMakeStep(MakeStep message)
        {
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

            // Check if player walked in a puddle
            WatchPuddle(targetTile.Position);
        }


        private void WatchPuddle(Vector2 point)
        {
            if (_steppedIntoPuddle)
                return;

            Plane puddle = new Plane("937, 1081, 941, 1065");
            if(puddle.LaysOnPlane(point))
            {
                _steppedIntoPuddle = true;
                World.PlayCutscene(Owner, "cs2");
            }
        }
    }


}
