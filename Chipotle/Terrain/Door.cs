using System;
using System.Collections.Generic;
using System.Linq;

using Game.Entities;
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
        protected string _closingSound = "snd24";

        /// <summary>
        /// Sound played when an NPC bumps to the door.
        /// </summary>
        protected string _hitSound = "KitchenDoorCrash";

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
        [typeof(DoorHit)] = (message) => OnDoorHit((DoorHit)message),
        [typeof(UseObject)] = (m) => OnUseObject((UseObject)m)
    });
        }

        /// <summary>
        ///  Handles the DoorHit message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected virtual void OnDoorHit(DoorHit message)
        {
            World.Sound.Play(stream: World.Sound.GetRandomSoundStream(_hitSound), role: null, looping: false, PositionType.Absolute, message.Point.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);

        }

        /// <summary>
        /// Closes the door if possible
        /// </summary>
        protected void Close(Vector2 position, object sender)
        {
            if (Area.GetTiles().All(t => World.IsWalkable(t.position)))
            {
                State = PassageState.Closed;

                DoorManipulated message = new DoorManipulated(this);
                foreach (Locality l in Localities)
                    l.ReceiveMessage(message);

                Play(_closingSound, position, sender);
            }
        }

        /// <summary>
        /// Plays the specified sound.
        /// </summary>
        /// <param name="sound">Name of the sound to be played</param>
        /// <param name="position"></param>
        /// <param name="obstacle">Describes type of obstacle between the entity and the player if any.</param>
        protected void Play(string sound, Vector2 position, object sender)
        {

            // Set attenuation parameters
            Entity entity = sender as Entity;
            ObstacleType obstacle = sender!= World.Player ? World.DetectAcousticObstacles(entity.Area) : ObstacleType.None;

            if (obstacle == ObstacleType.Far)
            {
                Locality l = World.Player.Locality;
                if (Localities[0].IsAccessible(l) || Localities[1].IsAccessible(l)) // Can be heart from the adjecting locality
                    obstacle = ObstacleType.Wall;
                    else return; // Too far and inaudible

            }

                // Set attenuation parameters
                bool attenuate = obstacle != ObstacleType.None && obstacle != ObstacleType.IndirectPath; ;
                (float gain, float gainHF) lowpass = default;
                float volume = _defaultVolume;

                switch (obstacle)
                {
                    case ObstacleType.Wall: lowpass = World.Sound.OverWallLowpass; volume = World.Sound.GetOverWallVolume(_defaultVolume); break;
                    case ObstacleType.Door: lowpass = World.Sound.OverDoorLowpass; volume = World.Sound.GetOverDoorVolume(_defaultVolume); break;
                    case ObstacleType.Object: lowpass = World.Sound.OverObjectLowpass; volume = World.Sound.GetOverObjectVolume(_defaultVolume); break;
                }

                // Play the sound
                int id = World.Sound.Play(stream: World.Sound.GetRandomSoundStream(sound), role: null, looping: false, PositionType.Absolute, position.AsOpenALVector(), true, volume);

                if (attenuate)
                    World.Sound.ApplyLowpass(id, lowpass);
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
                Open(message.Position, message.Sender);
            else
                Close(message.Position, message.Sender);
        }

        /// <summary>
        /// Opens the door if possible.
        /// </summary>
        /// <param name="position">
        /// The coordinates of the place on the door that an NPC is pushing on
        /// </param>
        protected virtual void Open(Vector2 position, object sender)     
        {
            State = PassageState.Open;

            DoorManipulated message = new DoorManipulated(this);
                Localities[0].ReceiveMessage(message);
                Localities[1].ReceiveMessage(message);

            Play(_openingSound, position, sender);
        }
    }
}