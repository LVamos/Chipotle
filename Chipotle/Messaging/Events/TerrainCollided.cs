using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Game.Terrain;
namespace Game.Messaging.Events
{
	class TerrainCollided : GameMessage
	{
		public readonly Tile Tile;

		/// <summary>
		/// Constructs new instance of InpermeableTerrainCollision.
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="collidingTile">The inpermeable tile</param>
		public TerrainCollided (object sender, Tile tile) : base(sender) => Tile = tile;
	}
}
