namespace Game.Terrain
{
    /// <summary>
    /// Defines all possible rotation moves.
    /// </summary>
    public enum TurnType : int
    {
        None = 0,
        SlightlyLeft = -45,
        SlightlyRight = 45,
        SharplyLeft = -90,
        SharplyRight = 90,
        Around = 180
    };

}
