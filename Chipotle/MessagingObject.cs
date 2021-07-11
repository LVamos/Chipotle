
using Luky;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Game
{


	public delegate void MessageHandler(Message message);

    public abstract class MessagingObject:DebugSO
    {
        public override int GetHashCode()
            => unchecked(4565 *(566 +_messagingEnabled.GetHashCode()) *(7193 +(_messages==null ? 0 : _messages.GetHashCode())));

		public virtual void Update()
        {
            if (_messagingEnabled && !_messages.IsNullOrEmpty())
                HandleMessage(DequeueMessage());
        }


        public virtual void Start()
        {
            _messages = new Queue<Message>();
            _messagingEnabled = true;
        }

        protected bool _messagingEnabled;


        protected void RegisterMessageHandlers(Dictionary<Type, Action<Message>> handlers)
        => _messageHandlers = _messageHandlers.Concat(handlers).GroupBy(d => d.Key).ToDictionary(d => d.Key, d => d.First().Value);

        /// <summary>
        /// Gets a message from another game object and handles it in appropriate way.
        /// </summary>
        /// <param name="message"></param>
        public virtual void ReceiveMessage(Message message)
        {
            if(_messagingEnabled)
            EnqueueMessage(message);
        }

        protected Dictionary<Type, Action<Message>> _messageHandlers;

        /// <summary>
        /// Handles concrete message
        /// </summary>
        /// <param name="message"></param>
        protected virtual void HandleMessage(Message message)
        {
                if (_messageHandlers.TryGetValue(message.GetType(), out var handler))
                handler(message);
        }



        /// <summary>
        /// Sends a message to another game object.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="message"></param>
        protected virtual void SendMessage(MessagingObject target, Message message)
            => target.ReceiveMessage(message);

        /// <summary>
        /// Adds incoming message to queue.
        /// </summary>
        /// <param name="message"></param>
        protected virtual void EnqueueMessage            (Message message)
            => _messages.Enqueue(message);

        /// <summary>
        /// Takes first message from message queue.
        /// </summary>
        /// <returns>First message from queue</returns>
        protected virtual Message DequeueMessage()
=> _messages.Count > 0 ? _messages.Dequeue() : null;

        /// <summary>
        /// 
        /// </summary>
        protected Queue<Message> _messages;

        /// <summary>
        /// Constructor
        /// </summary>
        protected MessagingObject()
        {
            _messageHandlers = new Dictionary<Type, Action<Message>>();
        }
    }
}
