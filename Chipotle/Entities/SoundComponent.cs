using System;

using Luky;

namespace Game.Entities
{
    /// <summary>
    /// Controls the sound output of an NPC.
    /// </summary>
    [Serializable]
    public class SoundComponent : EntityComponent
    {
        /// <summary>
        /// Reference to the sound player
        /// </summary>
        protected SoundThread _sound => World.Sound;
    }
}