using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Terrain;

namespace Game
{
	class CollisionMessage: Message
	{
		public readonly Tile Tile;

		/// <summary>
		/// Constructs new instance of the message.
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="tile">Tile with the colliding object</param>
		public CollisionMessage(object sender, Tile tile) : base(sender) => Tile=tile;
	}
}
