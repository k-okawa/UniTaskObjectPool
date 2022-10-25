using System;
using System.Threading;
using Bg.UniTaskObjectPool;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Bg.UniTaskObjectPoolTest
{
    public class CommonTest
    {
        public class SamplePoolObj : IDisposable
        {
            public static int globalIndex = 0;
            public readonly int id;
            
            public SamplePoolObj()
            {
                id = ++globalIndex;
            }
            
            public async UniTask GetLog(CancellationToken ct)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: ct);
                Debug.Log($"Get from pool id:{id}");
            }
            
            public async UniTask ReleaseLog(CancellationToken ct)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: ct);
                Debug.Log($"Release id:{id}");
            }

            public void Dispose()
            {
                Debug.Log($"Disposed id:{id}");
            }
        }

        public static async UniTask<SamplePoolObj> CreateFunc(CancellationToken ct)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: ct);
            return new SamplePoolObj();
        }

        public static UniTask OnGet(SamplePoolObj element, CancellationToken ct)
        {
            return element.GetLog(ct);
        }
        
        public static UniTask OnRelease(SamplePoolObj element, CancellationToken ct)
        {
            return element.ReleaseLog(ct);
        }

        public static void Destroy(SamplePoolObj element)
        {
            element.Dispose();
        }

        public static async UniTask NormalTest(IAsyncObjectPool<SamplePoolObj> pool, CancellationToken ct = default)
        {
            var element = await pool.Get(ct);
            AsyncPooledObject<SamplePoolObj> pooledObject = await pool.GetPooledObject(ct);
            await UniTask.Delay(TimeSpan.FromSeconds(0.1f), cancellationToken: ct);
            await pool.Release(element, ct);
            await pooledObject.DisposeAsync(ct);
            Assert.IsTrue(pool.CountInactive == 2);
        }
    }
}