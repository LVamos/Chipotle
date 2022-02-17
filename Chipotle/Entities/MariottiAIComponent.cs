using ProtoBuf;
using System;

using Game.Messaging.Commands;
using Game.Terrain;

using ProtoBuf;

namespace Game.Entities
{
    /// <summary>
    /// Controls behavior of the Paolo Mariotti NPC.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    public class MariottiAIComponent : AIComponent
    {
        /// <summary>
        /// Initializes the component and starts its message loop.
        /// </summary>
        public override void Start()
        {
            base.Start();
            Owner.ReceiveMessage(new SetPosition(this, new Plane("2018, 1131"), true));
        }
    }
}