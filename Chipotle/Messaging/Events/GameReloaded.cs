namespace Game.Messaging.Events
{
    /// <summary>
    /// Indicates that the game state was loaded from file.
    /// </summary>
    public class GameReloaded : GameMessage
    {
        public GameReloaded() : base(null) { }
    }
}