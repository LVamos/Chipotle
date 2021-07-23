using Game.Terrain;

namespace Game.Messaging.Commands
{
	public class UseObject : GameMessage
	{
		public readonly Tile Tile;

		public UseObject (object sender, Tile tile=null) : base(sender) 
		{
			Tile = tile;
		}

	}
}
