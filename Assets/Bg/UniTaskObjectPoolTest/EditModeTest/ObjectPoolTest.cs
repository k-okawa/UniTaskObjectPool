using System;
using System.Collections;
using Bg.UniTaskObjectPool;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;
using Assert = UnityEngine.Assertions.Assert;

namespace Bg.UniTaskObjectPoolTest
{
    public class ObjectPoolTest
    {

        [SetUp]
        public void SetUp()
        {
            CommonTest.SamplePoolObj.globalIndex = 0;
        }
        
        [UnityTest]
        public IEnumerator GetTest() => UniTask.ToCoroutine(async () =>
        {
            var objectPool = new AsyncObjectPool<CommonTest.SamplePoolObj>(
                    CommonTest.CreateFunc,
                    CommonTest.OnGet,
                    CommonTest.OnRelease,
                    CommonTest.Destroy
                );
            await CommonTest.NormalTest(objectPool);

            objectPool = new AsyncObjectPool<CommonTest.SamplePoolObj>(
                CommonTest.CreateFunc
            );
            await CommonTest.NormalTest(objectPool);
            
            objectPool = new AsyncObjectPool<CommonTest.SamplePoolObj>(
                CommonTest.CreateFunc,
                CommonTest.OnGet
            );
            await CommonTest.NormalTest(objectPool);
            
            objectPool = new AsyncObjectPool<CommonTest.SamplePoolObj>(
                CommonTest.CreateFunc,
                CommonTest.OnGet,
                CommonTest.OnRelease
            );
            await CommonTest.NormalTest(objectPool);
            
            objectPool = new AsyncObjectPool<CommonTest.SamplePoolObj>(
                CommonTest.CreateFunc,
                CommonTest.OnGet,
                CommonTest.OnRelease,
                CommonTest.Destroy,
                true, 0, 2
            );
            await CommonTest.NormalTest(objectPool);
        });
        
        [UnityTest]
        public IEnumerator LoopTest() => UniTask.ToCoroutine(async () =>
        {
            var objectPool = new AsyncObjectPool<CommonTest.SamplePoolObj>(
                CommonTest.CreateFunc,
                CommonTest.OnGet,
                CommonTest.OnRelease,
                CommonTest.Destroy
            );
            await CommonTest.LoopTest(objectPool, 10);
        });
        
        [UnityTest]
        public IEnumerator ExceptionTest() => UniTask.ToCoroutine(async () =>
        {
            bool hasException = false;
            try
            {
                var objectPool = new AsyncObjectPool<CommonTest.SamplePoolObj>(null);
                await CommonTest.LoopTest(objectPool, 10);
            }
            catch (ArgumentNullException)
            {
                hasException = true;
            }
            Assert.IsTrue(hasException);
            hasException = false;
            
            try
            {
                var objectPool = new AsyncObjectPool<CommonTest.SamplePoolObj>(CommonTest.CreateFunc, maxSize: 0);
                await CommonTest.LoopTest(objectPool, 10);
            }
            catch (ArgumentException)
            {
                hasException = true;
            }
            Assert.IsTrue(hasException);
            hasException = false;
            
            try
            {
                var objectPool = new AsyncObjectPool<CommonTest.SamplePoolObj>(CommonTest.CreateFunc);
                var obj = await objectPool.Get();
                await objectPool.Release(obj);
                await objectPool.Release(obj);
            }
            catch (InvalidOperationException)
            {
                hasException = true;
            }
            Assert.IsTrue(hasException);
            hasException = false;
        });
    }
}