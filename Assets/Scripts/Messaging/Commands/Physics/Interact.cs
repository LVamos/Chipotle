using Game.Entities;
using Game.Terrain;

using System;

namespace Game.Messaging.Commands.Physics
{
	/// <summary>
	/// Instructs a physics component of a character to use an object.
	/// </summary>
	/// <remarks>When Object is null, the NPC finds usable objects.</remarks>
	public class Interact : Message
	{
		/// <summary>
		/// An item or character to be used.
		/// </summary>
		public readonly MapElement Object;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source fo the message</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="sender"/> is null</exception>
		public Interact(object sender, MapElement obj = null) : base(sender)
		{
			Object = obj;
		}
	}
}