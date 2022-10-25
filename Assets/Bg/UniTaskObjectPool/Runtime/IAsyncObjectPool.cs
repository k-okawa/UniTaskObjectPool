using System.Threading;
using Cysharp.Threading.Tasks;

namespace Bg.UniTaskObjectPool
{
    public interface IAsyncObjectPool<T> where T : class
    {
        int CountInactive { get; }
        UniTask<T> Get(CancellationToken ct = default);
        UniTask<AsyncPooledObject<T>> GetPooledObject(CancellationToken ct = default);
        UniTask Release(T element, CancellationToken ct = default);
        void Clear();
    }
}
