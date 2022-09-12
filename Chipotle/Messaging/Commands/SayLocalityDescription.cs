using System;
using System.Collections.Generic;
using System.Deployment.Application;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Messaging.Commands
{
	/// <summary>
	/// Instructs a sound component of a character to read description of the current locality.
	/// </summary>
	public class SayLocalityDescription : GameMessage
	{
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Souruce of the message</param>
		public SayLocalityDescription(object sender) : base(sender)
		{
		}
	}
}
