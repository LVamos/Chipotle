using Game.Terrain;


namespace Game.Messaging.Commands
{
    public class MoveChipotlesCar : GameMessage
    {
        public readonly Locality Destination;

        public MoveChipotlesCar(object sender, Locality destination) : base(sender) => Destination = destination;
    }
}
