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
        private readonly Stopwatch _realTime  = new Stopwatch();
        private bool _disposed;
        private BlockingCollection<Message> _messages = new BlockingCollection<Message>();

        /// <summary>
        /// 
        /// </summary>
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

        // these methods provide hooks for sub classes to measure performance or wrap the commands and queries in other logic if they want.
        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        protected virtual void OnCommand(Action command)
        => command(); 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        protected virtual object[] OnQuery(Func<object[]> query)
        => query(); 

        /// <summary>
        /// 
        /// </summary>
        protected virtual void OnTick()
        => throw new Exception("The base class did not override OnTick"); 

        /// <summary>
        /// 
        /// </summary>
        protected void ProcessMessages()
        { 
            // this overload never calls OnTick
            _realTime .Start();

            while (!_disposed)
            {
                var message = _messages.Take();

                if (message.Type == MessageType.Command)
                    OnCommand(message.Command);
                else
                {
                    // must be a query
                    message.Results = OnQuery(message.Query);

                    if (!message.ResetEvent.Set())
                        throw new Exception("The ManualResetEvent.Set method returned false.");
                }//ls
            } // whl
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="millisecondsPerTick"></param>
        protected void ProcessMessages(int millisecondsPerTick)
        {
            _realTime .Start();

            while (!_disposed)
            {
                int millisecondsAlreadyProcessed = _ticks * millisecondsPerTick;
                int millisecondsAlreadyElapsedThisTick = (int)Math.Floor(_realTime .Elapsed.TotalMilliseconds) - millisecondsAlreadyProcessed;
                int sleepTime = millisecondsPerTick - millisecondsAlreadyElapsedThisTick;

                // We ensure sleepTime is at least 0, so we can always run mMessages.TryTake, ensuring we check for messages every loop instead of running multiple ticks without checking messages.
                if (sleepTime < 0)
                    sleepTime = 0;
                Message message;

                if (_messages.TryTake(out message, sleepTime))
                {
                    if (message.Type == MessageType.Command)
                        OnCommand(message.Command);
                    else
                    {
                        // must be a query
                        message.Results = OnQuery(message.Query);

                        if (!message.ResetEvent.Set())
                            throw new Exception("The ManualResetEvent.Set method returned false.");
                    }//ls
                }//if
                else
                {
                    OnTick();
                    _ticks++;
                }//ls
            } // whl
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        protected void RunCommand(Action command)
        {
            var m = new Message();
            m.Type = MessageType.Command;
            m.Command = command;
            _messages.Add(m);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryFunc"></param>
        /// <returns></returns>
        protected object[] RunQuery(Func<object[]> queryFunc)
        {
            var m = new Message();
            m.Type = MessageType.Query;
            m.Query = queryFunc;
            m.ResetEvent = new ManualResetEvent(false);
            _messages.Add(m);
            m.ResetEvent.WaitOne();
            m.ResetEvent.Dispose();
            return m.Results;
        }

        /// <summary>
        /// 
        /// </summary>
        private sealed class Message
        {
            public Action Command;
            public Func<object[]> Query;
            public ManualResetEvent ResetEvent;
            public object[] Results;
            public MessageType Type;
        } // cls
    } // cls
}