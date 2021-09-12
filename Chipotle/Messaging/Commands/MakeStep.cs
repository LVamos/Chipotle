
using Game.Terrain;

namespace Game.Messaging.Commands
{
    public class MakeStep : TurnEntity
    {
        /// <summary>
        /// Constructs new instance of the message.
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="direction">Specifies if the moved entity should go forth, left or right. TurnType.None means forth.</param>
        public MakeStep(object sender, TurnType direction) : base(sender, direction) { }
    }
}
