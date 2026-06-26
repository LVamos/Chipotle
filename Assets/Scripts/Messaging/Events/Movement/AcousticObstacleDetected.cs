using UnityEngine;

namespace Game.Messaging.Events.Movement
{
    public class AcousticObstacleDetected : Message
    {
        public readonly Vector2 Direction;

        public AcousticObstacleDetected(object sender, Vector2 direction) : base(sender)
        {
            Direction = direction;
        }
    }
}
