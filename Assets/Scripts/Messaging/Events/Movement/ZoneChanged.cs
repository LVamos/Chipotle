using System;
using Game.Terrain;

namespace Game.Messaging.Events.Movement
{
	/// <summary>
	/// Indicates that an NPC moved from one zone to another one.
	/// </summary>
	/// <remarks>Sent from a descendant of the <see cref="Entities.Characters.Components.CharacterComponent"/> class.</remarks>
	[Serializable]
	public class ZoneChanged : Message
	{
		/// <summary>
		/// The zone the NPC left
		/// </summary>
		public readonly Zone Source;

		/// <summary>
		/// The zone the NPC entered
		/// </summary>
		public readonly Zone Target;

		/// <summary>
		/// constructor
		/// </summary>
		/// <param name="sender">Object that sends the message</param>
		/// <param name="source">The zone the NPC left</param>
		/// <param name="target">The zone the NPC entered</param>
		public ZoneChanged(object sender, Zone source, Zone target) : base(sender)
		{
			Source = source;
			Target = target;
		}
	}
}