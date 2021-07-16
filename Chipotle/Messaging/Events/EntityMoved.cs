using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Game.Terrain;

namespace Game.Messaging.Events
{
	class EntityMoved : GameMessage
	{
		public readonly Tile Target;


		/// <summary>
		/// Constructs new instance of the message.
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="target">Final location</param>
		public EntityMoved (object sender, Tile target) : base(sender) => Target=target;

	}
}
