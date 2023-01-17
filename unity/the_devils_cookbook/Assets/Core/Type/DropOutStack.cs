using System;

namespace TDC.Core.Type
{
    public class DropOutStack<T>
    {
        private readonly T[] _Data;
        private int _TopIndex = 0;

        public int Count { get; private set; } = 0;

        public void Push(T item)
        {
            _Data[_TopIndex] = item;
            _TopIndex = (_TopIndex + 1) % _Data.Length;
            Count = Math.Min(Count + 1, _Data.Length);
        }

        public bool TryPop(out T item)
        {
            item = default;
            if (Count == 0) return false;
            _TopIndex = (_Data.Length + _TopIndex - 1) % _Data.Length;
            item = _Data[_TopIndex];
            Count--;
            return true;
        }
        
        public DropOutStack(uint capacity)
        {
            _Data = new T[capacity];
        }
    }
}