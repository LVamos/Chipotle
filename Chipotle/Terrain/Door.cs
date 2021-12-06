using System;
using System.Collections.Generic;
using System.Linq;

using Game.Messaging;
using Game.Messaging.Commands;

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
        /// constructor
        /// </summary>
        /// <param name="name">Inner name for the door</param>
        /// <param name="closed">Specifies whether the door should be implicitly closed or open</param>
        /// <param name="area">Location of the door</param>
        /// <param name="localities">Two localities connected by the door</param>
        /// <param name="openable">Specifies whether the door can be opened by an NPC.</param>
        public Door(Name name, bool closed, Plane area, IEnumerable<Locality> localities, bool openable = true) : base(name, area, localities)
        {
            Closed = closed;
            _openable = openable;
        }

        /// <summary>
        /// Specifies if the door is closed or open.
        /// </summary>
        public bool Closed { get; protected set; }

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
                Closed = true;
                World.Sound.Play(stream: World.Sound.GetRandomSoundStream("snd24"), role: null, looping: false, PositionType.Absolute, coords.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);
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

            if (Closed)
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
            Closed = false;
            World.Sound.Play(stream: World.Sound.GetRandomSoundStream("snd23"), role: null, looping: false, PositionType.Absolute, coords.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);
        }
    }
}