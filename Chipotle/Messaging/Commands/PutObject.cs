using OpenTK;
using Game.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Messaging.Commands
{
	/// <summary>
	/// Represents a command that causes that NPC puts an object on the ground.
	/// </summary>
	public class PutObject: GameMessage
	{
		/// <summary>
		/// Source of the message.
		/// </summary>
		public new readonly Entity Sender;

		/// <summary>
		/// The object that should be put on the ground.
		/// </summary>
		public readonly DumpObject Object;

		/// <summary>
		/// A point near which the object should be placed.
		/// </summary>
		public readonly Vector2 Position;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="object">The object that should be put on the ground</param>
		/// <param name="position">A point near which the object should be placed</param>
		/// <exception cref="ArgumentNullException">Throws an exception if one of the required parameters are null.</exception>
		public PutObject(Entity sender, DumpObject @object, Vector2 position): base(sender)
		{
			Sender = sender ?? throw new ArgumentNullException(nameof(sender));
			Object = @object ?? throw new ArgumentNullException(nameof(@object));
			Position = position;
		}
	}
}
