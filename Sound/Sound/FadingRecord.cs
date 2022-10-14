namespace Luky
{
    /// <summary>
    /// Describes how a sound's volume should be dynamically modified.
    /// </summary>
    public class FadingRecord
    {
        /// <summary>
        /// Counts time from initialization.
        /// </summary>
        public int Ticks;

        /// <summary>
        /// Method of fading for the specified sound
        /// </summary>
        public readonly FadingType Type;

        /// <summary>
        /// specifies how steep should the volume change be.
        /// </summary>
        public readonly float VolumeDelta;

        /// <summary>
        /// Specifies if the sound should be stopped after it was muted.
        /// </summary>
        public bool Stop { get; }

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
        /// <param name="stop">Specifies if the sound should be stopped after it was muted.</param>
        public FadingRecord(FadingType type, float volumeDelta, float targetVolume, bool stop = true)
        {
            Type = type;
            TargetVolume = targetVolume;
            VolumeDelta = volumeDelta;
            Stop = stop;
        }
    }
}
