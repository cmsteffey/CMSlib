using System;
using System.Collections;
using System.Collections.Generic;

namespace CMSlib.CollectionTypes
{
    public class FifoBuffer<T> : IEnumerable<T>
    {
        private readonly List<T> _internalBuffer;
        public FifoBuffer(int capacity)
        {
            _internalBuffer = new List<T>(capacity);
        }

        public void Add(T item)
        {
            if(_internalBuffer.Count == _internalBuffer.Capacity)
                _internalBuffer.RemoveAt(0);
            _internalBuffer.Add(item);
            
        }
        

        public IEnumerator<T> GetEnumerator()
        {
            return new FifoBufferEnumerator<T>(this);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private class FifoBufferEnumerator<T> : IEnumerator<T>
        {
            
            
            private int _internalPointer = -1;
            
            T IEnumerator<T>.Current => _internalBuffer[^(_internalPointer + 1)];
            object IEnumerator.Current => _internalBuffer[^(_internalPointer + 1)];
            bool IEnumerator.MoveNext()
            {
                _internalPointer++;
                return _internalPointer < _internalBuffer.Count;
            }

            void IEnumerator.Reset()
            {
                _internalPointer = -1;
            }

            internal FifoBufferEnumerator()
            {
                throw new NotSupportedException();
            }

            public void Dispose()
            {
                
            }

            private List<T> _internalBuffer;
            internal FifoBufferEnumerator(FifoBuffer<T> buffer)
            {
                _internalBuffer = buffer._internalBuffer;
            }

        }
    }

    
}