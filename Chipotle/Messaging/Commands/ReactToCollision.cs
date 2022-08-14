using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Game.Entities;

namespace Game.Messaging.Commands
{
	/// <summary>
	/// A message that instruct an entity component to react on collision.
	/// </summary>
	public class ReactToCollision: GameMessage
	{
		/// <summary>
		/// The entity that bumped to this entity.
		/// </summary>
		public readonly Entity entity;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="entity">The entity that bumped to this entity</param>
		public ReactToCollision(object sender, Entity entity): base(sender) { }
	}
}
