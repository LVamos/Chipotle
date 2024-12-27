using System;
using Game.Terrain;

namespace Game.Messaging.Events.Movement
{
	/// <summary>
	/// Indicates that an NPC moved from one locality to another one.
	/// </summary>
	/// <remarks>Sent from a descendant of the <see cref="Entities.Characters.Components.CharacterComponent"/> class.</remarks>
	[Serializable]
	public class LocalityChanged : Message
	{
		/// <summary>
		/// The locality the NPC left
		/// </summary>
		public readonly Locality Source;

		/// <summary>
		/// The locality the NPC entered
		/// </summary>
		public readonly Locality Target;

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="sender">Object that sends the message</param>
		/// <param name="source">The locality the NPC left</param>
		/// <param name="target">The locality the NPC entered</param>
		public LocalityChanged(object sender, Locality source, Locality target) : base(sender)
		{
			Source = source;
			Target = target;
		}
	}
}