using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Luky
{
    /// <summary>
    /// Describes how a sound's volume should be dynamically modified.
    /// </summary>
    public struct FadingRecord
    {
        /// <summary>
        /// Declares types of fading.
        /// </summary>
        public enum FadingType
        {
            /// <summary>
            /// Specifies that a sound should be fluently muted.
            /// </summary>
            In,

            /// <summary>
            /// specifies that a sound should be fluently unmuted.
            /// </summary>
            Out
        };

        /// <summary>
        /// Method of fading for the specified sound
        /// </summary>
        public readonly FadingType Type;

        /// <summary>
        /// Identifier of the sound to be modified.
        /// </summary>
        public readonly Sound Sound;

        /// <summary>
        /// specifies how steep should the volume change be.
        /// </summary>
        public readonly float VolumeDelta;

        /// <summary>
        /// Specifies final volume of the specified sound.
        /// </summary>
        public readonly float TargetVolume;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="sound">Identifier of the sound to be modified.</param>
        /// <param name="type">Method of fading for the specified sound</param>
        /// <param name="volumeDelta">specifies how steep should the volume change be.</param>
        /// <param name="targetVolume">Specifies final volume of the specified sound.</param>
        public FadingRecord(Sound sound, FadingType type, float volumeDelta, float targetVolume)
        {
            Sound = sound;
            Type = type;
            TargetVolume = targetVolume;
            VolumeDelta = volumeDelta;
        }
    }
}
