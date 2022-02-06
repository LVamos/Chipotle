using System;
using System.Collections.Generic;

using Game.Entities;
using Game.Messaging.Commands;
using Game.Messaging.Events;

using Luky;

using OpenTK;

namespace Game.Terrain
{
    /// <summary>
    /// Represents a sliding door.
    /// </summary>
    [Serializable]
    public class SlidingDoor : Door
    {

        /// <summary>
        /// Processes the EntityMoved message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnEntityMoved(EntityMoved message)
        {
            base.OnEntityMoved(message);

            Entity entity = message.Sender as Entity;
            bool opposite = IsInFrontOrBehind(entity.Area.Center);
            bool near = _area.GetDistanceFrom(entity.Area.Center)<= _minDistance;

            if (opposite && near && State == PassageState.Closed)
                Open(_area.Center, entity);
            else if (!near && State == PassageState.Open)
                Close(_area.Center, entity);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Inner name of the door</param>
        /// <param name="area">Coordinates of the area the door occupies</param>
        /// <param name="localities">The localities connected by the door</param>
        public SlidingDoor(Name name, Plane area, IEnumerable<Locality> localities) : base(name, PassageState.Closed, area, localities)
=> _openingSound = _closingSound = "SlidingDoor";

        /// <summary>
        /// Specifies the minimum distance between the entity and the door at which the door opens.
        /// </summary>
        protected int _minDistance = 4;

        /// <summary>
        /// Initializes the door and starts its message loop.
        /// </summary>
        public override void Start()
        {
            base.Start();

            // Register and unregister messages message
            _messageHandlers.Remove(typeof(UseObject)); // Can open only on its own.

            RegisterMessages(new Dictionary<Type, Action<Messaging.GameMessage>>()
            {
                [typeof(EntityMoved)] = (message) => OnEntityMoved((EntityMoved)message)
            });
        }
    }
}