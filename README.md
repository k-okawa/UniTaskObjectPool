# UniTaskObjectPool

ObjectPool for UniTask.

AsyncObjectPool in this package conform to [UnityEngine.Pool.ObjectPool<T>](https://docs.unity3d.com/ScriptReference/Pool.ObjectPool_1.html)

Differences is bellow

- Create, OnGet and OnRelease return value is UniTask
- Create, OnGet and OnRelease argument includes CancellationToken

## Installation
### Dependency
Using UniTask in the package, UniTask must be added in the project.

[UniTask](https://github.com/Cysharp/UniTask)

### PackageManager

#### Install via git url

Open Window/Package Manager, and add package from git URL...

```
https://github.com/k-okawa/UniTaskObjectPool.git?path=Assets/Bg/UniTaskObjectPool
```

#### Install via OpenUPM

```
openupm add com.bg.unitaskobjectpool
```

### UnityPackage

You can download unity package in [release page](https://github.com/k-okawa/UniTaskObjectPool/releases).

## Sample Code

```csharp
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
        await UniTask.Delay(TimeSpan.FromSeconds(0.02f), cancellationToken: ct);
        Debug.Log($"Get from pool id:{id}");
    }
    
    public async UniTask ReleaseLog(CancellationToken ct)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(0.02f), cancellationToken: ct);
        Debug.Log($"Release id:{id}");
    }

    public void Dispose()
    {
        Debug.Log($"Disposed id:{id}");
    }
}

public static async UniTask<SamplePoolObj> CreateFunc(CancellationToken ct)
{
    await UniTask.Delay(TimeSpan.FromSeconds(0.02f), cancellationToken: ct);
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
```


```csharp
public async UniTask Sample(CancellationToken ct) 
{
    var objectPool = new AsyncObjectPool<SamplePoolObj>(
                    CreateFunc,
                    OnGet,
                    OnRelease,
                    Destroy,
                    true, 10, 100
                );
    
    var element = await objectPool.Get(ct);
    await objectPool.Release(element, ct);
    
    var pooledObject = await objectPool.GetPooledObject(ct);
    // call element function sample
    await pooledObject.Get().GetLog(ct);
    // return to pool
    await pooledObject.DisposeAsync(ct);
}
```