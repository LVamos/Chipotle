
using Game.Terrain;

namespace Game.Messaging.Events
{
    internal class TurnEntityResult : GameMessage
    {
        private Orientation2D NewOrientation;

        /// <summary>
        /// Constructs new instance of TurnoverDone.
        /// </summary>
        /// <param name="sender">Source of the message</param>
        /// <param name="newOrientation">Final orientation vector</param>
        public TurnEntityResult(object sender, Orientation2D newOrientation) : base(sender) => NewOrientation = newOrientation;
    }
}
