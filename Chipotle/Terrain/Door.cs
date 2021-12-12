using System;
using System.Collections.Generic;
using System.Linq;

using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;

using Luky;

using OpenTK;

namespace Game.Terrain
{
    /// <summary>
    /// Represents a door between two localities.
    /// </summary>
    [Serializable]
    public class Door : Passage
    {
        /// <summary>
        /// Describes if a door is opened or closed.
        /// </summary>
        public enum DoorState
        {
            Open,
            Closed
        }

        /// <summary>
        /// Indicates if the door is open or closed.
        /// </summary>
        public DoorState State { get; protected set; }

        /// <summary>
        /// specifies if the door can be opened by an NPC.
        /// </summary>
        protected readonly bool _openable;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">Inner name for the door</param>
        /// <param name="closed">Specifies whether the door should be implicitly closed or open</param>
        /// <param name="area">Location of the door</param>
        /// <param name="localities">Two localities connected by the door</param>
        /// <param name="openable">Specifies whether the door can be opened by an NPC.</param>
        public Door(Name name, DoorState state, Plane area, IEnumerable<Locality> localities, bool openable = true) : base(name, area, localities)
        {
            State = state;
            _openable = openable;
        }

        /// <summary>
        /// Initializes the door and starts its message loop.
        /// </summary>
        public override void Start()
        {
            base.Start();

            RegisterMessages(
    new Dictionary<Type, Action<GameMessage>>()
    {
        [typeof(UseObject)] = (m) => OnUseObject((UseObject)m)
    });
        }

        /// <summary>
        /// Closes the door if possible
        /// </summary>
        protected void Close(Vector2 coords)
        {
            if (Area.GetTiles().All(t => World.IsWalkable(t.position)))
            {
                State = DoorState.Closed;

                DoorManipulated message = new DoorManipulated(this);
                foreach (Locality l in Localities)
                    l.ReceiveMessage(message);

                World.Sound.Play(stream: World.Sound.GetRandomSoundStream("snd24"), role: null, looping: false, PositionType.Absolute, Area.Center.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);
            }
        }

        /// <summary>
        /// Processes the UseObject message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected virtual void OnUseObject(UseObject message)
        {
            if (!_openable)
                return;

            if (State == DoorState.Closed)
                Open(message.Position);
            else
                Close(message.Position);
        }

        /// <summary>
        /// Opens the door if possible.
        /// </summary>
        /// <param name="coords">
        /// The coordinates of the place on the door that an NPC is pushing on
        /// </param>
        protected virtual void Open(Vector2 coords)     
        {
            State = DoorState.Open;

            DoorManipulated message = new DoorManipulated(this);
            foreach (Locality l in Localities)
                l.ReceiveMessage(message);

            World.Sound.Play(stream: World.Sound.GetRandomSoundStream("snd23"), role: null, looping: false, PositionType.Absolute, Area.Center.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);
        }
    }
}