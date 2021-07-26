using System;

namespace Luky
{
    /// <summary>
    /// 
    /// </summary>
    internal static class Names
    {
        public static readonly Name Master = "Master";
        public static readonly Name Music = "Music";
        public static readonly Name NonSpatialized = "NonSpatialized";
        public static readonly Name Spatialized = "Spatialized";
        public static readonly Name Window = "Window";
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class SoundExtensionMethods
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static OpenTK.Vector3 AsOpenTKV3(this Vector3 v)
        => new OpenTK.Vector3(v.X, v.Y, v.Z);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static OpenTK.Vector3 AsOpenTKV3(this Vector2 v)
        => new OpenTK.Vector3(v.X, v.Y, 0);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public static Vector2 AsV2(this OpenTK.Vector2 v)
        => new Vector2(v.X, v.Y);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        internal static Vector3 AsV3(this OpenTK.Vector3 v)
        => new Vector3(v.X, v.Y, v.Z);
    }

    /// <summary>
    /// 
    /// </summary>
    internal static class SoundHM
    {
        /// <summary>
        /// this method will pan a mono sound into interleaved stereo, outputting into the same buffer it was given
        /// </summary>
        /// <param name="sBuffer"></param>
        /// <param name="panning"></param>
        public static void Pan(ShortBuffer sBuffer, float panning)
        {
            if (panning < -1 || panning > 1)
            {
                throw new ArgumentException("panning must be between -1 and 1");
            }

            int inLength = sBuffer.Filled;
            short[] buffer = sBuffer.Data;
            int outLength = inLength * 2;
            if (buffer.Length < outLength)
            {
                throw new Exception("Pan requires a buffer that is at least twice the length of the input data");
            }

            float leftMultiplier = (1 - panning) / 2;
            float rightMultiplier = (1 + panning) / 2;

            // we fill the buffer in reverse, so we can use the same buffer for input and output
            for (int monoIndex = inLength - 1; monoIndex >= 0; monoIndex--)
            { // we read the mono bytes in reverse order
              // we output the interleaved stereo bytes in reverse order
                int stereoIndex = monoIndex * 2;
                short monoVal = buffer[monoIndex];
                buffer[stereoIndex] = (short)Math.Round(monoVal * leftMultiplier);
                buffer[stereoIndex + 1] = (short)Math.Round(monoVal * rightMultiplier);
            } // for

            sBuffer.Filled *= 2;
        }
    } // cls

    // cls

    // cls
}