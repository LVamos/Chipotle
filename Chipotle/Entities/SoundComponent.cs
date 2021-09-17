using Luky;

namespace Game.Entities
{
    /// <summary>
    /// Controls the sound output of an NPC.
    /// </summary>
    public class SoundComponent : EntityComponent
    {
        /// <summary>
        /// Reference to the sound player
        /// </summary>
        protected readonly SoundThread _sound = World.Sound;
    }
}