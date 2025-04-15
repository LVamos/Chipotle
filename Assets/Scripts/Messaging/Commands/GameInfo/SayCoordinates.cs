using System;

namespace Game.Messaging.Commands.GameInfo
{
	/// <summary>
	/// Reports current position of an NPC in absolute or relative coordinates.
	/// </summary>
	[Serializable]
	public class SayCoordinates : Message
	{
		/// <summary>
		/// Specifies if the game should report relative or absolute coordinates. Relative coordinates are computed as an offset from upper left corner of the current zone.
		/// </summary>
		public readonly bool Relative;

		public SayCoordinates(object sender, bool relative = true) : base(sender)
			=> Relative = relative;
	}
}