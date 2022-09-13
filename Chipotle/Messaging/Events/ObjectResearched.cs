using Game.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Messaging.Events
{
	/// <summary>
	/// Announces an object or character that it was researched by a character.
	/// </summary>
	public class ObjectResearched: GameMessage
	{
		/// <summary>
		/// The character that researched the object.
		/// </summary>
		public new readonly Character Sender;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">The character that researched the object or another character</param>
		/// <exception cref="ArgumentNullException">Thrown if some of the parameters are null</exception>
		public ObjectResearched(Character sender) : base(sender) => Sender = sender ?? throw new ArgumentNullException(nameof(sender));
	}
}
