using System;
using Game.Entities;

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
		public readonly Entity Object;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source fo the message</param>
		/// <exception cref="ArgumentNullException">Thrown when <paramref name="sender"/> is null</exception>
		public Interact(object sender, Entity obj = null) : base(sender)
		{
			Object = obj;
		}
	}
}