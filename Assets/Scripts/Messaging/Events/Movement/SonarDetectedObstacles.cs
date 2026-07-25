namespace Game.Messaging.Events.Movement
{
    public class SonarDetectedObstacles : Message
    {
        public bool ObstaclesOnLeft { get; set; }
        public bool ObstaclesOnRight { get; set; }

        public SonarDetectedObstacles(object sender, bool obstaclesOnLeft, bool obstaclesOnRight) : base(sender)
        {
            ObstaclesOnLeft = obstaclesOnLeft;
            ObstaclesOnRight = obstaclesOnRight;
        }
    }
}
