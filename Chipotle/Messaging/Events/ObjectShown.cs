using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Game.Entities;

using Game.Terrain;

namespace Game.Messaging.Events
{
	class ObjectShown : GameMessage
	{
		public readonly GameObject  NewObject;

		/// <summary>
		/// Constructs new instance of the message.
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="newObject">Newly appeared object</param>
		public ObjectShown (MessagingObject sender, GameObject newObject) : base(sender) => NewObject = newObject;
	}
}
