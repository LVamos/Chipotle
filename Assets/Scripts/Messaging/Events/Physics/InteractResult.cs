using Game.Entities;
using Game.Entities.Characters;

namespace Game.Messaging.Events.Physics
{
	/// <summary>
	/// Represents the result of an interaction.
	/// </summary>
	public class InteractResult : Message
	{
		public enum ResultType
		{
			/// <summary>
			/// Successfully used the item or character.
			/// </summary>
			Success,
			/// <summary>
			/// No items or characters in range
			/// </summary>
			NoObjects,
			/// <summary>
			/// No usable items or characters in range
			/// </summary>
			NoUsableObjects,
			/// <summary>
			/// Usable items or charycters far away
			/// </summary>
			Far
		};

		/// <summary>
		/// Result of the operation
		/// </summary>
		public readonly ResultType Result;

		/// <summary>
		/// The character that interacted with the object.
		/// </summary>
		public readonly Character Character;

		/// <summary>
		/// The object that was used.
		/// </summary>
		public readonly Entity UsedObject;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Sender of the event</param>
		/// <param name="result">Result of the action</param>
		/// <param name="character">The character that interacted with the item or character</param>param>
		/// <param name="usedObject">The used item or character</param>
		public InteractResult(object sender, ResultType result, Character character = null, Entity usedObject = null) : base(sender)
		{
			Result = result;
			Character = character;
			UsedObject = usedObject;
		}
	}
}