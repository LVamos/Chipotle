using System;
using System.Collections.Generic;
using System.Linq;

using Game.Messaging.Events;

using Luky;

namespace Game.Messaging
{
    public delegate void MessageHandler(GameMessage message);

    public abstract class MessagingObject : DebugSO
    {
        protected Dictionary<Type, Action<GameMessage>> _messageHandlers;

        /// <summary>
        /// </summary>
        protected Queue<GameMessage> _messages;

        protected bool _messagingEnabled;

        /// <summary>
        /// Constructor
        /// </summary>
        protected MessagingObject() => _messageHandlers = new Dictionary<Type, Action<GameMessage>>();

        public override int GetHashCode()
                                            => unchecked(4565 * (566 + _messagingEnabled.GetHashCode()) * (7193 + (_messages == null ? 0 : _messages.GetHashCode())));

        /// <summary>
        /// Gets a message from another game object and handles it in appropriate way.
        /// </summary>
        /// <param name="message"></param>
        public virtual void ReceiveMessage(GameMessage message)
        {
            if (_messagingEnabled)
                if (message.Sender != this || (message.Sender == this && (message is CutsceneBegan || message is CutsceneEnded)))
                    EnqueueMessage(message);
        }

        /// <summary>
        /// Initializes the messaging object and starts its message loop.
        /// </summary>
        public virtual void Start()
        {
            _messages = new Queue<GameMessage>();
            _messagingEnabled = true;
        }

        public virtual void Update()
        {
            if (_messagingEnabled && !_messages.IsNullOrEmpty())
                HandleMessage(DequeueMessage());
        }

        /// <summary>
        /// Takes first message from message queue.
        /// </summary>
        /// <returns>First message from queue</returns>
        protected virtual GameMessage DequeueMessage()
=> _messages.Count > 0 ? _messages.Dequeue() : null;

        /// <summary>
        /// Adds incoming message to queue.
        /// </summary>
        /// <param name="message"></param>
        protected virtual void EnqueueMessage(GameMessage message)
            => _messages.Enqueue(message);

        /// <summary>
        /// Handles concrete message
        /// </summary>
        /// <param name="message"></param>
        protected virtual void HandleMessage(GameMessage message)
        {
            if (_messageHandlers.TryGetValue(message.GetType(), out Action<GameMessage> handler))
                handler(message);
        }

        protected void RegisterMessages(Dictionary<Type, Action<GameMessage>> handlers)
                                => _messageHandlers = _messageHandlers.Concat(handlers).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);

        /// <summary>
        /// Sends a message to another game object.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="message"></param>
        protected virtual void SendMessage(MessagingObject target, GameMessage message)
            => target.ReceiveMessage(message);
    }
}