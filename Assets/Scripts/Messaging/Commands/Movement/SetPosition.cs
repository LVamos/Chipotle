using System;

namespace Game.Messaging.Commands.Movement
{
	/// <summary>
	/// Tells an NPC to immediately relocate to the specified coordinates.
	/// </summary>
	/// <remarks>
	/// Applies to the <see cref="Entities.Characters.Character"/> class. Can be sent only from inside the
	/// NPC from a descendant of the <see cref="Entities.Characters.Components.CharacterComponent"/> class.
	/// </remarks>
	[Serializable]
	public class SetPosition : Message
	{
		/// <summary>
		/// specifies if some walk sounds should be played.
		/// </summary>
		public readonly bool Silently;

		/// <summary>
		/// The location to which the NPC moves
		/// </summary>
		public readonly Terrain.Rectangle Target;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">source of the message</param>
		/// <param name="target">The location to which the NPC moves</param>
		/// <param name="silently">Specifies if some walk sounds should be played.</param>
		public SetPosition(object sender, Terrain.Rectangle target, bool silently = false) : base(sender)
		{
			Target = target;
			Silently = silently;
		}
	}
}