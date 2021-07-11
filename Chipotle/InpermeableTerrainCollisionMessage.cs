using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Game.Terrain;
namespace Game
{
	class InpermeableTerrainCollisionMessage: Message
	{
		public readonly Tile Tile;

		/// <summary>
		/// Constructs new instance of InpermeableTerrainCollision.
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="collidingTile">The inpermeable tile</param>
		public InpermeableTerrainCollisionMessage(object sender, Tile tile) : base(sender) => Tile = tile;
	}
}
