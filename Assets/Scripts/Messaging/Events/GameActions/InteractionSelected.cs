using Game.Terrain;

using System;

namespace Game.Messaging.Events.GameActions
{
	public class InteractionSelected : Message
	{
		/// <summary>
		/// An item or character to be used.
		/// </summary>
		public readonly MapElement Object;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="sender"/> is null</exception>
		public InteractionSelected(object sender, MapElement obj = null) : base(sender)
		{
			Object = obj;
		}
	}
}
