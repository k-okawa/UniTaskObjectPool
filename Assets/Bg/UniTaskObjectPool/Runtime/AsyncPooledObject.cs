using System;
using Cysharp.Threading.Tasks;

namespace Bg.UniTaskObjectPool
{
    public struct AsyncPooledObject<T> : IDisposable where T : class
    {
        readonly T m_ToReturn;
        readonly IAsyncObjectPool<T> m_Pool;
        
        internal AsyncPooledObject(T value, IAsyncObjectPool<T> pool)
        {
            m_ToReturn = value;
            m_Pool = pool;
        }

        void IDisposable.Dispose() => m_Pool.Release(m_ToReturn);
    }
}