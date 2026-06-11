using Game.Messaging;

using UnityEngine;

namespace Assets.Scripts.Messaging.Events.Movement
{
    public class ObstacleDetected : Message
    {
        public readonly Vector2 Direction;

        public ObstacleDetected(object sender, Vector2 direction) : base(sender)
        {
            Direction = direction;
        }
    }
}
