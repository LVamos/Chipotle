using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Game.Terrain;

namespace Game
{
	class MovementDoneMessage: Message
	{
		public readonly Tile Target;


		/// <summary>
		/// Constructs new instance of the message.
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="target">Final location</param>
		public MovementDoneMessage(object sender, Tile target) : base(sender) => Target=target;

	}
}
