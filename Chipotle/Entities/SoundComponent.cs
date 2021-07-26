
using Luky;

namespace Game.Entities
{
    public abstract class SoundComponent : EntityComponent
    {


        protected readonly SoundThread _sound = World.Sound;

    }
}
