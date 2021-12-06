using System;

namespace Game.Messaging.Commands
{
    [Serializable]
    public class StopWalk : GameMessage
    {
        public StopWalk(object sender) : base(sender) { }
    }
}