using Game.Terrain;



namespace Game.Messaging.Commands
{
    public class Reveal : GameMessage
    {
        public readonly Plane Location;

        public Reveal(object sender, Plane location) : base(sender)
            => Location = location;
    }
}
