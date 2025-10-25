using Game.Terrain;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Messaging
{
	public abstract class GameComponent<TOwner> : MessagingObject
		where TOwner : MapElement
	{
		public virtual TOwner Owner { get; }

		/// <summary>
		/// Indexed name of the owner fo this component.
		/// </summary>
		protected string _ownerName;
	}
}
