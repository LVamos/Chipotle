using Game.Terrain;
using DavyKager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Luky;

using System.Windows.Forms;
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
            if(_plannedRotations>0)
            {
                _orientation.Rotate(_rotationStep);
            _plannedRotations--;

                if(_plannedRotations==0)
            Owner.ReceiveMessage(new TurnoverDoneMessage(this,_orientation));
            }
        }

        private int _rotationStep;
        private int _plannedRotations;


        public override void Start()
        {
            // set initial position.
            SetPosition(new Plane(new Vector2(1027, 1005)));
            _orientation = new Orientation2D(0, 1);

            base.Start();

            RegisterMessageHandlers(
                new Dictionary<Type, Action<Message>>()
                {
                    // Test messages
                    [typeof(TerrainInfo)] =(message)=> OnTerrainInfo((TerrainInfo)message),


                    [typeof(LocalityAnnouncement)] = (m) => OnLocalityAnnouncement((LocalityAnnouncement)m),
                    [typeof(Movement)] =(m)=> OnMovement((Movement)m),
                    [typeof(Turnover)] =(message)=> OnTurnover((Turnover)message),
                    [typeof(InteractionStartMessage)] = OnInteractionStart
                }
                );

        }

        private void OnLocalityAnnouncement(LocalityAnnouncement m)
=>  SayDelegate(World.Map[Area.UpperLeftCorner].Locality.Name.Friendly);

        private void OnTerrainInfo(TerrainInfo message)
        {
            SayDelegate(World.Map[Area.UpperLeftCorner].Terrain.GetDescription());
        }

        private void OnInteractionStart(Message message)
        {
            Tolk.Speak("OnUseObject neimplementováno.");
        }

        private void OnTurnover(Turnover message)
        {
             _rotationStep =message.Degrees>=0 ? 1 : -1;
            _plannedRotations = Math.Abs(message.Degrees);
        }

        private void OnMovement(Movement message)
        {
            // Get target coordinates
            var finalOrientation = _orientation;

            if (message.Direction!= TurnType.None)
                finalOrientation.Rotate((TurnType)message.Direction);

            var target = Area;
            target.Move(finalOrientation, .3f);

            // Is the terrain occupable?
            Assert(target.IsInMapBoundaries(), "Columbo off the map!"); // Verify map boundaries.
            Tile targetTile = World.Map[target.UpperLeftCorner] ?? throw new InvalidOperationException($"{nameof(OnMovement)}: empty tile."); // Null test

            if (!targetTile.Permeable)
            {
                Owner.ReceiveMessage(new InpermeableTerrainCollisionMessage(this, targetTile));
                return;
            }

            // Isn't an entity or object over there?
if(targetTile.IsOccupied&& targetTile.Object!=Owner)
            {
                Owner.ReceiveMessage(new CollisionMessage(this,targetTile));
                return;
            }

            // The road is clear! Move!
                SetPosition(target);
            Owner.ReceiveMessage(new MovementDoneMessage(this,targetTile));
        }





    }
}
