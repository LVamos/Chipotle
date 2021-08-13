using Game.Terrain;


namespace Game.Messaging.Commands
{
    public class UnblockLocality : GameMessage
    {
        public readonly Locality Locality;

        public UnblockLocality(object sender, Locality locality) : base(sender) => Locality = locality;
    }
}
