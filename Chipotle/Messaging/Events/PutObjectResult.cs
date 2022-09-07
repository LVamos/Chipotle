using Game.Entities;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Game.Messaging.Events
{
	public class PutObjectResult: GameMessage
	{
		/// <summary>
		/// Result of placing an object to the ground
		/// </summary>
		public readonly bool Success;

		/// <summary>
		/// Soruce fo the message.
		/// </summary>
		public new readonly DumpObject Sender;

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="sender">Source of the message</param>
		/// <param name="result">Result of the operation</param>
		/// <exception cref="ArgumentNullException"></exception>
		public PutObjectResult(DumpObject sender, bool success = false): base(sender)
		{
			Sender = sender ?? throw new ArgumentNullException(nameof(sender));
			Success = success;
		}
	}
}
