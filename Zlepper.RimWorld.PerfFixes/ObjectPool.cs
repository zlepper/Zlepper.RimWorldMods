using System.Runtime.CompilerServices;

namespace Zlepper.RimWorld.PerfFixes;

public class ObjectPool<T>
where T:IPoolable
{
    private readonly Stack<T> _pool = new(10);
    
    private readonly Func<T> _factory;

    public ObjectPool(Func<T> factory)
    {
        _factory = factory;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Get()
    {
        if (_pool.Count == 0)
        {
            return _factory();
        }
        return _pool.Pop();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Return(T obj)
    {
        obj.Reset();
        _pool.Push(obj);
    }
}

public interface IPoolable
{
    public void Reset();
}