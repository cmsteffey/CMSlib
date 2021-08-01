using System;
using System.Collections;
using System.Collections.Generic;

namespace CMSlib.CollectionTypes
{
    public class FifoBuffer<T> : IEnumerable<T>
    {
        private readonly T[] _buffer;
        private int _offset;
        private int _count;
        private object lockObj = new();
        public FifoBuffer(int capacity)
        {
            _buffer = new T[capacity];
        }

        public void Add(T item)
        {
            lock (lockObj)
            {
                if (_count == _buffer.Length)
                {
                    _buffer[_offset] = item;
                    _offset++;
                    _offset %= _buffer.Length;
                }
                else
                {
                    _buffer[_offset] = item;
                    _offset++;
                    _count++;
                }
            }
        }

        public T this[int index]
        {
            get
            {
                if (index >= _count || index < 0)
                    throw new IndexOutOfRangeException(
                        "Index must be greater or equal to 0 and less than the size of this collection");
                return _buffer[(index + _offset) % _buffer.Length];
            }
        }

        public int Count => _count;

        public int Capacity
        {
            get => _buffer.Length;
        }
        public void Clear()
        {
            lock (lockObj)
            {
                _offset = 0;
                _count = 0;
            }
        }
        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return new FifoBufferIterator<T>(this);
        }
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new FifoBufferIterator<T>(this);
        }

        private class FifoBufferIterator<TItemType> : IEnumerator<TItemType>
        {
            private TItemType[] _buffer;
            private int itemPointer;
            
            internal FifoBufferIterator(FifoBuffer<TItemType> buffer)
            {
                _buffer = new TItemType[buffer._count];
                bool wrap = buffer._offset + buffer._count >= buffer._buffer.Length;
                if (wrap)
                {
                    int firstChunkLen = buffer._buffer.Length - buffer._offset;
                    Array.Copy(buffer._buffer, buffer._offset, _buffer, 0, firstChunkLen);
                    Array.Copy(buffer._buffer, 0, _buffer, firstChunkLen, buffer._count - firstChunkLen);
                }
                else
                {
                    Array.Copy(buffer._buffer, buffer._offset, _buffer, 0, buffer._count);
                }
            }
            
            bool IEnumerator.MoveNext()
            {
                itemPointer++;
                return itemPointer < _buffer.Length;
            }

            void IEnumerator.Reset()
            {
                itemPointer = -1;
            }

            TItemType IEnumerator<TItemType>.Current => _buffer[itemPointer];

            object IEnumerator.Current => _buffer[itemPointer];

            void IDisposable.Dispose()
            {
            }
        }
    }
}