using Game.Entities.Characters.Components;
using Game.Messaging.Events.Sound;
using Game.Terrain;

using ProtoBuf;

using System;
using System.Collections.Generic;

using UnityEngine;

namespace Game.Messaging
{
	/// <summary>
	/// Base class for all objects used in the game; receives, sends and processes messages.
	/// </summary>
	[ProtoContract(SkipConstructor = true, ImplicitFields = ImplicitFields.AllFields)]
	[ProtoInclude(100, typeof(CharacterComponent))]
	[ProtoInclude(101, typeof(MapElement))]
	public abstract class MessagingObject : MonoBehaviour
	{
		/// <summary>
		/// Stores the messages before they are processed.
		/// </summary>
		[ProtoIgnore]
		protected Queue<Message> _messages = new();

		/// <summary>
		/// Starts or stops the messaging.
		/// </summary>
		protected bool _messagingEnabled;

		/// <summary>
		/// returns a hash code for the instance.
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode()
			=> unchecked(4565 * (566 + _messagingEnabled.GetHashCode()) * (7193 + (_messages == null ? 0 : _messages.GetHashCode())));

		/// <summary>
		/// Gets a message from another messaging object and stores it for processing.
		/// </summary>
		/// <param name="message">The message to be received</param>
		public virtual void TakeMessage(Message message)
		{
			if (message == null)
				throw new ArgumentNullException(nameof(message));

			// Initialize the message queue if needed.
			CheckQueue();

			if (_messagingEnabled)
				if (message.Sender != this || (message.Sender == this && (message is CutsceneBegan || message is CutsceneEnded)))
					EnqueueMessage(message);
		}

		/// <summary>
		/// INitializes the message queue if needed.
		/// </summary>
		protected void CheckQueue()
		{
			if (_messagingEnabled && _messages == null)
				_messages = new();
		}

		/// <summary>
		/// Enables messaging and performs initial actions.
		/// </summary>
		public virtual void Activate()
			=> EnableMessaging();

		/// <summary>
		/// Enables messaging and initializes the message queue.
		/// </summary>
		protected void EnableMessaging()
		{
			_messages = new();
			_messagingEnabled = true;
		}

		/// <summary>
		/// Processes incoming messages.
		/// </summary>
		public virtual void GameUpdate()
		{
			if (_messagingEnabled && !_messages.IsNullOrEmpty())
				HandleMessage(DequeueMessage());
		}

		/// <summary>
		/// Takes a message from the message queue.
		/// </summary>
		/// <returns>First message from the queue</returns>
		protected virtual Message DequeueMessage()
			=> _messages.Count > 0 ? _messages.Dequeue() : null;

		/// <summary>
		/// Adds a message to the queue.
		/// </summary>
		/// <param name="message">The message to be stored</param>
		protected virtual void EnqueueMessage(Message message)
			=> _messages.Enqueue(message);

		/// <summary>
		/// Runs a message handler for the specified message.
		/// </summary>
		/// <param name="message">The message to be handled</param>
		protected virtual void HandleMessage(Message message)
			=> CheckQueue();

		/// <summary>
		/// Sends a message to another game object.
		/// </summary>
		/// <param name="target">The receiving messaging object</param>
		/// <param name="message">The message to be distributed</param>
		protected virtual void SendMessage(MessagingObject target, Message message)
			=> target.TakeMessage(message);
	}
}