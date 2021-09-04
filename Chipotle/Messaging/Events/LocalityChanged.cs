
using Game.Terrain;

namespace Game.Messaging.Events
{
    public class LocalityChanged: GameMessage
    {
        public readonly Locality Source;
        public readonly Locality Target;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="sender">Object that sends the message</param>
        /// <param name="source">Locality that the entity leaves</param>
        /// <param name="target">Locality too which the entity comes</param>
        public LocalityChanged(object sender, Locality source, Locality target) : base(sender)
        {
            Source = source;
            Target = target;
        }

    }
}
