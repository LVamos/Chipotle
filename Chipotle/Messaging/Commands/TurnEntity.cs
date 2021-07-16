using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Terrain;
namespace Game.Messaging.Commands
{
	class TurnEntity : GameMessage
	{
		public readonly TurnType Direction;
		public readonly int Degrees;

		/// <summary>
		/// Constructs new instance of Turn message.
		/// </summary>
		/// <param name="sender">Soruce object</param>
		/// <param name="direction">Turnover rate enum</param>
		public TurnEntity (object sender, TurnType direction) : base(sender)
		{
			Direction = direction;
			Degrees = (int)direction;
		}
	}
}
