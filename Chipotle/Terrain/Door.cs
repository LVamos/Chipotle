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
        public Door(Name name, bool closed, Plane area, IEnumerable<Locality> localities) : base(name, true, closed, area, localities, false)
        {
        }


        /// <summary>
        /// Closes the door if possible.
        /// </summary>
        public void Open()
        {
            Closed = false;
            World.Sound.Play(stream: World.Sound.GetRandomSoundStream("snd23"), role: null, looping: false, PositionType.Absolute, Area.Center.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);

        }



        /// <summary>
        /// Closes the door if possible
        /// </summary>
        public void Close()
        {
if(Area.GetTiles().All(t=> t.Walkable))
            {
                Closed = true;
                World.Sound.Play(stream: World.Sound.GetRandomSoundStream("snd24"), role: null, looping: false, PositionType.Absolute, Area.Center.AsOpenALVector(), true, 1f, null, 1f, 0, Playback.OpenAL);
            }
        }


    }
}
