using System.Collections;
using Bg.UniTaskObjectPool;
using Cysharp.Threading.Tasks;
using NUnit.Framework;
using UnityEngine.TestTools;

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
        });
    }
}