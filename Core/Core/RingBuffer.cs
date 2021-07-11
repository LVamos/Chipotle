using System;
using System.Collections.Generic;

namespace Luky
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class RingBuffer<T>
    {
        private int _capacity;
        private int _end = 0;
        private List<T> _list;
        private int _start = 0; // points to the first item in the list.

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="capacity"></param>
        public RingBuffer(int capacity)
        {
            _capacity = capacity;
            _list = new List<T>(capacity);
            for (int i = 0; i < capacity; i++)
                _list.Add(default(T));
        }

        public int Count { get => Length;  }

        /// <summary>
        /// points to one passed the last item in the list, which could be the same as mStart if no items are in the list, or if the list is entirely full.
        /// </summary>
        public int Length { get; private set; }

        public T this[int index]
        { get => _list[MapIndex(index)];  }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Add(T item)
        {
            _list[_end] = item;
            _end++;
            // Bob Nystrom shows this cooler way of doing this in the Event queue chapter of Game Programming Patterns:
            // mEnd = mEnd % mCapacity; // that handles wrapping back down to 0.
            if (_end == _capacity)
                _end = 0;
            if (Length == _capacity)
            { // mEnd caught up with mStart, so we lost an item from the start and need to move the start forward.
                _start++;
                if (_start == _capacity)
                    _start = 0;
            }
            else
                Length++;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public bool Any()
        => Length > 0; 

        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        => _start = _end = Length = 0; 

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public T Last()
        {
            if (Length == 0)
                throw new Exception("Length was 0");
            int index = _end - 1;
            if (index == -1)
                index = _capacity - 1;
            return _list[index];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public T Pop()
        {
            if (Length == 0)
                throw new Exception("Cannot pop from a ring buffer that has no items");
            _end--;
            if (_end < 0)
                _end = _capacity - 1;
            Length--;
            return _list[_end];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        private int MapIndex(int index)
        {
            if (index < 0 || index >= Length)
                throw new Exception(String.Format("index was outside the bounds of the ring buffer. index is {0}, length is {1}", index, Length));
            index += _start;
            if (index >= _capacity)
                index -= _capacity;
            return index;
        }
    } 
}