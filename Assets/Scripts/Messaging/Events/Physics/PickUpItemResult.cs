using Game.Entities.Items;

namespace Game.Messaging.Events.Physics
{
	/// <summary>
	/// informs an entity or entity component if the specified object was picked up or not.
	/// </summary>
	public class PickUpItemResult : Message
	{
		/// <summary>
		/// Defines possible results of picking an object off the ground.
		/// </summary>
		public enum ResultType
		{
			/// <summary>
			/// The object was picked up.
			/// </summary>
			Success,

			/// <summary>
			/// Unable to pcik the object up.
			/// </summary>
			Unpickable,

			/// <summary>
			/// Inventory of the NPC that wanted to pick up an object is full.
			/// </summary>
			FullInventory,

			/// <summary>
			/// No object within reach.
			/// </summary>
			NothingFound,
		}

		/// <summary>
		/// Specifies result of the attempt to pick up an object off the ground.
		/// </summary>
		public readonly ResultType Result;

		public readonly bool Silently;

		/// <summary>
		/// Sourceof the message
		/// </summary>
		public new readonly MessagingObject Sender;

		/// <summary>
		/// The manipulated object
		/// </summary>
		public readonly Item Object;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="object">The manipulated object</param>
		/// <param name="success">Specifies if the object was picked up off the ground or not.</param>
		public PickUpItemResult(MessagingObject sender, Item @object = null, ResultType result = ResultType.NothingFound, bool silently = false) : base(sender)
		{
			Sender = sender;
			Object = @object;
			Result = result;
			Silently = silently;
		}
	}
}