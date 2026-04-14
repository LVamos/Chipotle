using Game.Messaging.Commands.Movement;
using Game.Terrain;

namespace Game.Messaging.Events.Movement
{
	/// <summary>
	/// Indicates that an character has completed a rotation.
	/// </summary>
	/// <remarks>Sent from a descendant of the <see cref="Entities.Characters.Components.CharacterComponent"/> class.</remarks>
	public class CharacterRotated : ChangeOrientation
	{
		/// <summary>
		/// Original orientation
		/// </summary>
		public readonly Orientation2D Source;

		/// <summary>
		/// New orientation
		/// </summary>
		public Orientation2D Target;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="source">Original orientation of the NPC</param>
		/// <param name="target">New orientation of the NPC</param>
		public CharacterRotated(
			object sender,
			Orientation2D source,
			Orientation2D target,
			TurnType direction
			)
			: base(sender, direction)
		{
			Source = source;
			Target = target;
		}
	}
}