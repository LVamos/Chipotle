using System;
using Game.Entities;
using Game.Entities.Items;


namespace Game.Messaging.Commands.Physics
{
	/// <summary>
	/// Instructs a character to apply an item to another item or character.
	/// </summary>
	/// <remarks>If Target is null, the NPC has to investigate if there's another usable usable item or character.</remarks>
	public class ApplyItemToTarget : Message
	{
		/// <summary>
		///  Specifies the item to use
		/// </summary>
		public readonly Item ItemToUse;

		/// <summary>
		/// The target item or character.
		/// </summary>
		public readonly Entity Target;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Sende of the message</param>
		/// <param name="itemToUse">The item to be used</param>
		/// <param name="target">The target item or character</param>
		public ApplyItemToTarget(object sender, Item itemToUse, Entity target = null) : base(sender)
		{
			ItemToUse = itemToUse ?? throw new ArgumentNullException(nameof(itemToUse));
			Target = target;
		}
	}
}