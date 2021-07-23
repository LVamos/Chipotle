using Game.Messaging;
using Game.Messaging.Commands;
using Game.Messaging.Events;
using Luky;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Terrain
{
    public class Door: Passage
    {
        public bool Closed { get; protected set; }


        public Door(Name name, bool closed, Plane area, IEnumerable<Locality> localities) : base(name, true, closed, area, localities, false)
        {
            Closed = closed;
            RegisterMessages(
                new Dictionary<Type, Action<GameMessage>>() 
                { 
                    [typeof(UseObject)] = (m) => OnUseObject((UseObject)m) 
                });
        }

        private void OnUseObject(UseObject m)
        {
            if (Closed)
                Open();
            else Close();
        }


        /// <summary>
        /// Closes the door if possible.
        /// </summary>
        protected void Open()
        {
            Closed = false;
            World.Sound.Play(stream: World.Sound.GetRandomSoundStream("snd23"), role: null, looping: false, PositionType.Absolute, Area.Center.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);

        }



        /// <summary>
        /// Closes the door if possible
        /// </summary>
        protected void Close()
        {
if(Area.GetTiles().All(t=> t.Walkable))
            {
                Closed = true;
                World.Sound.Play(stream: World.Sound.GetRandomSoundStream("snd24"), role: null, looping: false, PositionType.Absolute, Area.Center.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);
            }
        }


    }
}
