using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bg.UniTaskObjectPool
{
    public readonly struct AsyncPooledObject<T> where T : class
    {
        readonly T m_ToReturn;
        readonly IAsyncObjectPool<T> m_Pool;
        
        internal AsyncPooledObject(T value, IAsyncObjectPool<T> pool)
        {
            m_ToReturn = value;
            m_Pool = pool;
        }

        public UniTask DisposeAsync(CancellationToken ct = default)
        {
            return m_Pool.Release(m_ToReturn, ct);
        }
    }
}