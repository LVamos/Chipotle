using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;

namespace Luky
{
    /// <summary>
    /// Base class for threads
    /// </summary>
    public abstract class BaseThread : DebugSO
    {
        protected int _ticks;
        private readonly BlockingCollection<Message> _messages = new BlockingCollection<Message>();
        private readonly Stopwatch _realTime = new Stopwatch();
        private bool _disposed;

        private enum MessageType
        {
            Command,
            Query,
        }

        /// <summary>
        /// Disposes the thread
        /// </summary>
        public virtual void Dispose()
        => RunCommand(() => _disposed = true);

        /// <summary>
        /// Code performed in thread's loop
        /// </summary>
        protected virtual void OnTick()
        => throw new Exception("The base class did not override OnTick");

        /// <summary>
        /// Loop of the thread
        /// </summary>
        protected void ProcessMessages()
        {
            // this overload never calls OnTick
            _realTime.Start();

            while (!_disposed)
            {
                Message message = _messages.Take();

                if (message.Type == MessageType.Command)
                    message.Command();
                else
                {
                    message.Results = message.Query();

                    if (!message.ResetEvent.Set())
                        throw new Exception("The ManualResetEvent.Set method returned false.");
                }
            }
        }

        /// <summary>
        /// Loop of the thread
        /// </summary>
        /// <param name="millisecondsPerTick">Interval of the loop</param>
        protected void ProcessMessages(int millisecondsPerTick)
        {
            _realTime.Start();

            while (!_disposed)
            {
                int millisecondsAlreadyProcessed = _ticks * millisecondsPerTick;
                int millisecondsAlreadyElapsedThisTick = (int)Math.Floor(_realTime.Elapsed.TotalMilliseconds) - millisecondsAlreadyProcessed;
                int sleepTime = millisecondsPerTick - millisecondsAlreadyElapsedThisTick;

                //I ensure sleepTime is at least 0, so I can always run _messages.TryTake,
                // ensuring I check for messages every loop instead of running multiple ticks
                // without checking messages.
                //if (sleepTime < 0)
                sleepTime = 0;

                if (_messages.TryTake(out Message message, sleepTime))
                {
                    if (message.Type == MessageType.Command)
                        message.Command();
                    else
                    {
                        message.Results = message.Query();

                        if (!message.ResetEvent.Set())
                            throw new Exception("The ManualResetEvent.Set method returned false.");
                    }
                }
                else
                {
                    OnTick();
                    _ticks++;
                }
            }
        }

        /// <summary>
        /// Schedules a command
        /// </summary>
        /// <param name="command">The acttion to be performed</param>
        protected void RunCommand(Action command)
        {
            Message m = new Message
            {
                Type = MessageType.Command,
                Command = command
            };
            _messages.Add(m);
        }

        /// <summary>
        /// Schedules a query
        /// </summary>
        /// <param name="queryFunc">The function to perform</param>
        /// <returns>Array of return values</returns>
        protected object[] RunQuery(Func<object[]> queryFunc)
        {
            Message m = new Message
            {
                Type = MessageType.Query,
                Query = queryFunc,
                ResetEvent = new ManualResetEvent(false)
            };
            _messages.Add(m);
            m.ResetEvent.WaitOne();
            m.ResetEvent.Dispose();
            return m.Results;
        }

        /// <summary>
        /// Encapsulates an action
        /// </summary>
        private sealed class Message
        {
            public Action Command;
            public Func<object[]> Query;
            public ManualResetEvent ResetEvent;
            public object[] Results;
            public MessageType Type;
        }
    }
}