using Game.Messaging.Commands;
using Game.Terrain;


namespace Game.Entities
{
    public class MariottiAIComponent : AIComponent
    {
        public override void Start()
        {
            base.Start();
            Owner.ReceiveMessage(new SetPosition(this, new Plane("2018, 1131"), true));
        }
    }
}
