using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace YouNameIt
{
    abstract class PoolFactory<T>
    {
        public T Create()
        {
            return OnCreate();
        }

        protected abstract T OnCreate();
    }

    sealed class Pool<T>
    {
        readonly PoolFactory<T> _factory;
        readonly T[] _recycle;
        int _count;

        public Pool(int count, PoolFactory<T> factory)
        {
            _recycle = new T[count];
            _count = count;
            _factory = factory;

            for (int i = 0; i < _count; ++i)
            {
                _recycle[i] = factory.Create();
            }
        }

        public T Rent()
        {
            if (_count <= 0)
            {
                return _factory.Create();
            }

            return _recycle[--_count];
        }

        public void Return(T value)
        {
            if (_count >= _recycle.Length)
            {
                return;
            }

            _recycle[_count++] = value;
        }
    }
}
