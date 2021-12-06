namespace Game.Terrain
{
    /// <summary>
    /// Defines all possible rotation moves.
    /// </summary>
    public enum TurnType : int
    {
        /// <summary>
        /// Default
        /// </summary>
        None = 0,

        /// <summary>
        /// Slightly left
        /// </summary>
        SlightlyLeft = -45,

        /// <summary>
        /// Slightly right
        /// </summary>
        SlightlyRight = 45,

        /// <summary>
        /// Sharply left
        /// </summary>
        SharplyLeft = -90,

        /// <summary>
        /// Sharply right
        /// </summary>
        SharplyRight = 90,

        /// <summary>
        /// All around
        /// </summary>
        Around = 180
    };
}