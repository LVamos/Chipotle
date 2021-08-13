using Game.Messaging;
using Game.Messaging.Commands;

using Luky;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Game.Terrain
{
    public class Door : Passage
    {
        public bool Closed { get; protected set; }

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
        /// constructor
        /// </summary>
        /// <param name="name">Inner name for the door</param>
        /// <param name="closed">Specifies whether the door should be implicitly closed or open</param>
        /// <param name="area">Location of the door</param>
        /// <param name="localities">Two localities, between which the door is.</param>
        /// <param name="openable">Specifies whether the door should be openable</param>
        public Door(Name name, bool closed, Plane area, IEnumerable<Locality> localities, bool openable=true) : base(name, area, localities)
        {
            Closed = closed;
            _openable = openable;
        }

        protected  readonly  bool _openable;

        protected virtual void OnUseObject(UseObject m)
        {
            if (!_openable)
                return;

            if (Closed)
            {
                Open(m.Tile.Position);
            }
            else
            {
                Close(m.Tile.Position);
            }
        }


        /// <summary>
        /// Closes the door if possible.
        /// </summary>
        protected virtual void Open(Vector2 coords)
        {
            Closed = false;
            World.Sound.Play(stream: World.Sound.GetRandomSoundStream("snd23"), role: null, looping: false, PositionType.Absolute, coords.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);

        }



        /// <summary>
        /// Closes the door if possible
        /// </summary>
        protected void Close(Vector2 coords)
        {
            if (Area.GetTiles().All(t => t.Walkable))
            {
                Closed = true;
                World.Sound.Play(stream: World.Sound.GetRandomSoundStream("snd24"), role: null, looping: false, PositionType.Absolute, coords.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);
            }
        }


    }
}
