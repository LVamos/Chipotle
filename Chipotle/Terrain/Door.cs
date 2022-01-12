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
        /// specifies if the door can be opened by an NPC.
        /// </summary>
        protected readonly bool _openable;

        /// <summary>
        /// Sound of the door being opened
        /// </summary>
        protected string _openingSound = "snd23";

        /// <summary>
        /// Sound of the door being closed
        /// </summary>
        protected string _closingSound;

        /// <summary>
        /// Sound played when an NPC bumps to the door.
        /// </summary>
        protected string _hitSound;

        /// <summary>
        /// Sound of the door being opened
        /// </summary>
        protected string _lockedSound;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="name">Inner name for the door</param>
        /// <param name="closed">Specifies whether the door should be implicitly closed or open</param>
        /// <param name="area">Location of the door</param>
        /// <param name="localities">Two localities connected by the door</param>
        /// <param name="openable">Specifies whether the door can be opened by an NPC.</param>
        public Door(Name name, PassageState state, Plane area, IEnumerable<Locality> localities, bool openable = true) : base(name, area, localities)
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
                State = PassageState.Closed;

                DoorManipulated message = new DoorManipulated(this);
                foreach (Locality l in Localities)
                    l.ReceiveMessage(message);

                World.Sound.Play(stream: World.Sound.GetRandomSoundStream(_closingSound), role: null, looping: false, PositionType.Absolute, Area.Center.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);
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

            if (State == PassageState.Closed)
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
            State = PassageState.Open;

            DoorManipulated message = new DoorManipulated(this);
                Localities[0].ReceiveMessage(message);
                Localities[1].ReceiveMessage(message);

            World.Sound.Play(stream: World.Sound.GetRandomSoundStream(_openingSound), role: null, looping: false, PositionType.Absolute, Area.Center.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);
        }
    }
}