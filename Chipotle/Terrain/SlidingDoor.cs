using Game.Entities;
using Game.Messaging;
using Game.Messaging.Events;

using Luky;

using OpenTK;

using ProtoBuf;

using System.Collections.Generic;

namespace Game.Terrain
{
    /// <summary>
    /// Represents a sliding door.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public class SlidingDoor : Door
    {

        /// <summary>
        /// Processes the EntityMoved message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected override void OnEntityMoved(CharacterMoved message)
        {
            base.OnEntityMoved(message);

            Character entity = message.Sender as Character;
            bool opposite = IsInFrontOrBehind(entity.Area.Center);
            bool near = _area.GetDistanceFrom(entity.Area.Center) <= _minDistance;

            // Find point from which the door sound should be heart.
            Vector2? tmp = _area.FindOppositePoint(entity.Area);
            Vector2 point = tmp.HasValue ? (Vector2)tmp : _area.GetClosestPoint(entity.Area.Center);

            if (opposite && near && State == PassageState.Closed)
                Open(entity, point);
            else if (!near && State == PassageState.Open)
                Close(entity, point);
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="name">Inner name of the door</param>
        /// <param name="area">Coordinates of the area the door occupies</param>
        /// <param name="localities">The localities connected by the door</param>
        public SlidingDoor(Name name, Rectangle area, IEnumerable<string> localities) : base(name, PassageState.Closed, area, localities)
        {
            _openable = false;
            _openingSound = _closingSound = "SlidingDoor";
        }

        /// <summary>
        /// Specifies the minimum distance between the entity and the door at which the door opens.
        /// </summary>
        protected int _minDistance = 4;

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(GameMessage message)
        {
            switch (message)
            {
                case CharacterMoved em: OnEntityMoved(em); break;
                default: base.HandleMessage(message); break;
            }
        }

        /// <summary>
        /// Initializes the door and starts its message loop.
        /// </summary>
        public override void Start()
        {
            base.Start();
        }
    }
}