using Game.Messaging;
using Game.Messaging.Events;
using Game.Terrain;

using ProtoBuf;

namespace Game.Entities
{
    /// <summary>
    /// Controls the behavior of an NPC.
    /// </summary>
    [ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
    [ProtoInclude(100, typeof(BartenderAIComponent))]
    [ProtoInclude(101, typeof(CarsonAIComponent))]
    [ProtoInclude(102, typeof(ChristineAIComponent))]
    [ProtoInclude(103, typeof(MariottiAIComponent))]
    [ProtoInclude(104, typeof(SweeneyAIComponent))]
    [ProtoInclude(105, typeof(TuttleAIComponent))]
    public class AIComponent : CharacterComponent
    {
        /// <summary>
        /// Area occupied by the NPC
        /// </summary>
        protected Rectangle _area;

        /// <summary>
        /// Instance of a path finder
        /// </summary>
        [ProtoIgnore]
        protected PathFinder _finder = new PathFinder();

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected override void HandleMessage(GameMessage message)
        {
            switch (message)
            {
                case PositionChanged pc: OnPositionChanged(pc); break;
                default: base.HandleMessage(message); break;
            }
        }

        /// <summary>
        /// Processes the PositionChanged message.
        /// </summary>
        /// <param name="message">The message to be processed</param>
        protected void OnPositionChanged(PositionChanged message)
            => _area = message.TargetPosition;
    }
}