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
    public class FadingRecord
    {
        /// <summary>
        /// Method of fading for the specified sound
        /// </summary>
        public readonly FadingType Type;

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
        /// <param name="type">Method of fading for the specified sound</param>
        /// <param name="volumeDelta">specifies how steep should the volume change be.</param>
        /// <param name="targetVolume">Specifies final volume of the specified sound.</param>
        public FadingRecord(FadingType type, float volumeDelta, float targetVolume)
        {
            Type = type;
            TargetVolume = targetVolume;
            VolumeDelta = volumeDelta;
        }
    }
}
