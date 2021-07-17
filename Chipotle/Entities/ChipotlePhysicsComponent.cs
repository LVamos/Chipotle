using DavyKager;

using Game.Entities;
using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Game.Terrain;

using Luky;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Game
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
                    Owner.ReceiveMessage(new TurnEntityResult(this, _orientation));
            }
        }

        private int _rotationStep;
        private int _plannedRotations;


        public override void Start()
        {
            // set initial position.
            SetPosition(new Plane(new Vector2(1025, 1030)));
            _orientation = new Orientation2D(0, 1);
            Locality locality = _area.GetLocality();
            locality.Register(Owner);
            locality.ReceiveMessage(new LocalityEntered(Owner, Owner));

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
                msg += " před tebou";
            else if (o.Area.UpperRightCorner.Y < _area.Center.Y)
                msg += " za tebou";
            else if (o.Area.LowerRightCorner.Y <= _area.Center.Y && o.Area.UpperRightCorner.Y >= _area.Center.Y)
            {
                if (o.Area.UpperRightCorner.X < _area.Center.X)
                    msg += " vlevo";
                else
                    msg += " vpravo";
            }

            Say(msg);
        }

        private void OnSayLocality(SayLocality m)
=> SayDelegate(World.Map[Area.UpperLeftCorner].Locality.Name.Friendly);

        private void OnSayTerrain(SayTerrain message)
        {
            SayDelegate(World.Map[Area.UpperLeftCorner].Terrain.GetDescription());
        }

        private void OnUseObject(UseObject message)
        {
            Tolk.Speak("OnUseObject neimplementováno.");
        }

        private void OnTurnEntity(TurnEntity message)
        {
            _rotationStep = message.Degrees >= 0 ? 1 : -1;
            _plannedRotations = Math.Abs(message.Degrees);
        }

        private void OnMakeStep(MakeStep message)
        {
            // Get target coordinates
            var finalOrientation = _orientation;

            if (message.Direction != TurnType.None)
                finalOrientation.Rotate(message.Direction);

            var target = Area;
            target.Move(finalOrientation, 1);

            // Is the terrain occupable?
            Assert(target.IsInMapBoundaries(), "Columbo off the map!"); // Verify map boundaries.
            Tile targetTile = World.Map[target.UpperLeftCorner] ?? throw new InvalidOperationException($"{nameof(OnMakeStep)}: empty tile."); // Null test

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

            // The road is clear! Move!
            SetPosition(target);
            Owner.ReceiveMessage(new EntityMoved(this, targetTile));

            Locality sourceLocality = _area.GetLocality();
            Locality targetLocality = World.Map[target.Center].Locality;
            EntityMoved moved = new EntityMoved(Owner, targetTile);

            if (targetLocality != sourceLocality)
            {
                sourceLocality.ReceiveMessage(moved);
                sourceLocality.ReceiveMessage(new LocalityLeft(this, Owner));
                targetLocality.ReceiveMessage(new LocalityEntered(Owner, Owner));
            }

            targetLocality.ReceiveMessage(moved);
        }





    }


}
