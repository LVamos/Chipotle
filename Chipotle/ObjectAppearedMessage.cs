using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Entities;

using Game.Terrain;

namespace Game
{
	class ObjectAppearedMessage: Message
	{
		public readonly GameObject  NewObject;

		/// <summary>
		/// Constructs new instance of the message.
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="newObject">Newly appeared object</param>
		public ObjectAppearedMessage(MessagingObject sender, GameObject newObject) : base(sender) => NewObject = newObject;
	}
}
