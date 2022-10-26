using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bg.UniTaskObjectPool
{
    public class AsyncObjectPool<T> : IDisposable, IAsyncObjectPool<T> where T : class
    {
        internal readonly Stack<T> m_Stack;
        private readonly Func<CancellationToken, UniTask<T>> m_CreateFunc;
        private readonly Func<T, CancellationToken, UniTask> m_ActionOnGet;
        private readonly Func<T, CancellationToken, UniTask> m_ActionOnRelease;
        private readonly Action<T> m_ActionOnDestroy;
        private readonly int m_MaxSize;
        internal bool m_CollectionCheck;

        public int CountAll { get; private set; }
        public int CountActive => CountAll - CountInactive;
        public int CountInactive => m_Stack.Count;

        private CancellationTokenSource m_Cancellation = new CancellationTokenSource();

        public AsyncObjectPool(Func<CancellationToken, UniTask<T>> createFunc, Func<T, CancellationToken, UniTask> actionOnGet = null, Func<T, CancellationToken, UniTask> actionOnRelease = null, Action<T> actionOnDestroy = null, bool collectionCheck = true, int defaultCapacity = 10, int maxSize = 10000)
        {
            if (createFunc == null)
                throw new ArgumentNullException(nameof(createFunc));

            if (maxSize <= 0)
                throw new ArgumentException("Max Size must be greater than 0", nameof(maxSize));

            m_Stack = new Stack<T>(defaultCapacity);
            m_CreateFunc = createFunc;
            m_MaxSize = maxSize;
            m_ActionOnGet = actionOnGet;
            m_ActionOnRelease = actionOnRelease;
            m_ActionOnDestroy = actionOnDestroy;
            m_CollectionCheck = collectionCheck;
        }

        public async UniTask<T> Get(CancellationToken ct = default)
        {
            var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(ct);
            
            T element;
            if (m_Stack.Count == 0)
            {
                element = await m_CreateFunc(linkedCancellation.Token);
                CountAll++;
            }
            else
            {
                element = m_Stack.Pop();
            }
            if (m_ActionOnGet != null)
                await m_ActionOnGet(element, linkedCancellation.Token);
            return element;
        }

        public async UniTask<AsyncPooledObject<T>> GetPooledObject(CancellationToken ct = default)
        {
            var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(ct);
            var element = await Get(linkedCancellation.Token);
            return new AsyncPooledObject<T>(element, this);
        }

        public async UniTask Release(T element, CancellationToken ct = default)
        {
            var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(ct);
            if (m_CollectionCheck && m_Stack.Count > 0)
            {
                if (m_Stack.Contains(element))
                    throw new InvalidOperationException("Trying to release an object that has already been released to the pool.");
            }

            if(m_ActionOnRelease != null)
                await m_ActionOnRelease(element, linkedCancellation.Token);

            if (CountInactive < m_MaxSize)
            {
                m_Stack.Push(element);
            }
            else
            {
                m_ActionOnDestroy?.Invoke(element);
            }
        }
        
        public void Clear()
        {
            m_Cancellation.Cancel();
            if (m_ActionOnDestroy != null)
            {
                foreach (var item in m_Stack)
                {
                    m_ActionOnDestroy?.Invoke(item);
                }
            }
            m_Stack.Clear();
            CountAll = 0;
            m_Cancellation = new CancellationTokenSource();
        }

        public void Dispose()
        {
            Clear();
        }
    }
}