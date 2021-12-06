using System;
using System.Collections.Generic;
using System.Linq;

using Game.Messaging.Events;

using Luky;

namespace Game.Messaging
{
    /// <summary>
    /// Delegate for all message handlers
    /// </summary>
    /// <param name="message"></param>
    public delegate void MessageHandler(GameMessage message);

    /// <summary>
    /// Base class for all objects used in the game; receives, sends and processes messages.
    /// </summary>
    [Serializable]
    public abstract class MessagingObject : DebugSO
    {
        /// <summary>
        /// Maps messages to message handlers
        /// </summary>
        protected Dictionary<Type, Action<GameMessage>> _messageHandlers;

        /// <summary>
        /// Stores the messages before they are processed.
        /// </summary>
        protected Queue<GameMessage> _messages;

        /// <summary>
        /// Starts or stops the messaging.
        /// </summary>
        protected bool _messagingEnabled;

        /// <summary>
        /// Constructor
        /// </summary>
        protected MessagingObject() => _messageHandlers = new Dictionary<Type, Action<GameMessage>>();

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
        public virtual void ReceiveMessage(GameMessage message)
        {
            Assert(message != null, nameof(message) + " was null.");

            if (_messagingEnabled)
                if (message.Sender != this || (message.Sender == this && (message is CutsceneBegan || message is CutsceneEnded)))
                    EnqueueMessage(message);
        }

        /// <summary>
        /// Initializes the object and starts the messaging.
        /// </summary>
        public virtual void Start()
        {
            _messages = new Queue<GameMessage>();
            _messagingEnabled = true;
        }

        /// <summary>
        /// Processes incoming messages.
        /// </summary>
        public virtual void Update()
        {
            if (_messagingEnabled && !_messages.IsNullOrEmpty())
                HandleMessage(DequeueMessage());
        }

        /// <summary>
        /// Takes a message from the message queue.
        /// </summary>
        /// <returns>First message from the queue</returns>
        protected virtual GameMessage DequeueMessage()
=> _messages.Count > 0 ? _messages.Dequeue() : null;

        /// <summary>
        /// Adds a message to the queue.
        /// </summary>
        /// <param name="message">The message to be stored</param>
        protected virtual void EnqueueMessage(GameMessage message)
            => _messages.Enqueue(message);

        /// <summary>
        /// Runs a message handler for the specified message.
        /// </summary>
        /// <param name="message">The message to be handled</param>
        protected virtual void HandleMessage(GameMessage message)
        {
            if (_messageHandlers.TryGetValue(message.GetType(), out Action<GameMessage> handler))
                handler(message);
        }

        /// <summary>
        /// Registers messages and their handlers.
        /// </summary>
        /// <param name="handlers">Dictionary of the messages and their handlers to be registered</param>
        protected void RegisterMessages(Dictionary<Type, Action<GameMessage>> handlers)
                                => _messageHandlers = _messageHandlers.Concat(handlers).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);

        /// <summary>
        /// Sends a message to another game object.
        /// </summary>
        /// <param name="target">The receiving messaging object</param>
        /// <param name="message">The message to be distributed</param>
        protected virtual void SendMessage(MessagingObject target, GameMessage message)
            => target.ReceiveMessage(message);
    }
}