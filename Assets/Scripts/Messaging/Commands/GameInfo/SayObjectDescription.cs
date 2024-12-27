using Game.Entities;

using System;

namespace Game.Messaging.Commands.GameInfo
{
	/// <summary>
	/// Instructs a sound component of a character to read description of the specified object or character.
	/// </summary>
	/// <remarks>If Object is null, it means that there is no object in front of the character.
	[Serializable]
	public class SayObjectDescription : Message
	{
		/// <summary>
		/// The object whose description should be read.
		/// </summary>
		public readonly Entity Object;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="object">The object whose description should be read.</param>
		/// <remarks>If Object is null then the sound component should announce that there's no object to be described.</remarks>
		public SayObjectDescription(object sender, Entity @object) : base(sender) => Object = @object;
	}
}