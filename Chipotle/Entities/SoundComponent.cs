using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Luky;

namespace Game.Entities
{
  public abstract  class SoundComponent: EntityComponent
    {


        protected  readonly SoundThread _sound = World.Sound;

    }
}
