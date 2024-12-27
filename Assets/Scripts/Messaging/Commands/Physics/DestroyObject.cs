using Game.Entities;

using System;

namespace Game.Messaging.Commands.Physics
{
	/// <summary>
	/// Destroys an NPC or object.
	/// </summary>
	/// <remarks>Applies to all descendants of the <see cref="Entity"/> class.</remarks>
	[Serializable]
	public class DestroyObject : Message
	{
		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		public DestroyObject(object sender) : base(sender)
		{
		}
	}
}