using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Game.Terrain;

namespace Game
{
	class TurnoverDoneMessage: Message
	{
		Orientation2D NewOrientation;

		/// <summary>
		/// Constructs new instance of TurnoverDone.
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="newOrientation">Final orientation vector</param>
		public TurnoverDoneMessage(object sender, Orientation2D newOrientation) : base(sender) => NewOrientation = newOrientation;
	}
}
