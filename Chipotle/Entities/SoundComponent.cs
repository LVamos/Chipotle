using Luky;

namespace Game.Entities
{
    public class SoundComponent : EntityComponent
    {
        protected readonly SoundThread _sound = World.Sound;
    }
}