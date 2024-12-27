using OpenTK;

namespace Luky
{
    /// <summary>
    /// Helper methods for the sound output
    /// </summary>
    public static class SoundExtensionMethods
    {
        /// <summary>
        /// Converts a <see cref="Vector2"/> to <see cref="Vector3"/> ignoring the Y axis.
        /// </summary>
        /// <param name="v">The vector to be converted</param>
        /// <returns>The <see cref="Vector3"/> struct</returns>
        public static Vector3 AsOpenALVector(this Vector2 v)
=> new Vector3(v.X, 0, v.Y);
    }
}