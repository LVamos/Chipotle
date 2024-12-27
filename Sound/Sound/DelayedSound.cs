using System;

namespace Luky
{
    /// <summary>
    /// A sound that was delayed because it was not primed yet
    /// </summary>
    public sealed class DelayedSound
    {
        /// <summary>
        /// Initializes a sound when it's primed.
        /// </summary>
        public Action Callback;

        /// <summary>
        /// Source of the sound data
        /// </summary>
        public ReadStream ReadStream;

        /// <summary>
        /// Unique identifier of the sound
        /// </summary>
        public int SoundID;
    }
}
