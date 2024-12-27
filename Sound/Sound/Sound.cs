using System.Collections.Generic;

namespace Luky
{
    /// <summary>
    /// Represents a played sound.
    /// </summary>
    public sealed class Sound
    {
        /// <summary>
        /// Decoder used with this sound.
        /// </summary>
        public Decoder Decoder;

        /// <summary>
        /// Sound groups
        /// </summary>
        public List<Name> GroupNames = new List<Name>()
            {
                new Name("master"), // For controlling a single master volume on all sounds
new Name("window"), // Adjusted when the window gains and loses focus
};

        /// <summary>
        /// An unique identifier
        /// </summary>
        public int ID;

        /// <summary>
        /// Volume for this sound
        /// </summary>
        public float IndividualVolume = 1f;

        /// <summary>
        /// Panning value in range from 0 to 1
        /// </summary>
        public float? Panning;

        /// <summary>
        /// Full path to the played sound file
        /// </summary>
        public FilePath Path;

        public Playback Playback = Playback.OpenAL;

        /// <summary>
        /// Sample rat eof the sound data
        /// </summary>
        public int SampleRate;
    }
}