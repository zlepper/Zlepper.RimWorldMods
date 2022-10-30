using System.Runtime.CompilerServices;

namespace Zlepper.RimWorld.PerfFixes;

public sealed class MapArray<T>
{
    private readonly T[] _array;
    private readonly int _sizeX;

    private int _minIndex = int.MaxValue;
    private int _maxIndex = int.MinValue;
    
    public MapArray(Map map) : this(map.Size)
    {
        
    }

    public MapArray(IntVec3 size) : this(size.x, size.z)
    {
        
    }
    
    public MapArray(int x, int z)
    {
        _array = new T[x * z];
        _sizeX = x;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Get(int x, int z)
    {
        return Get(GetIndex(x, z));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Get(int index)
    {
        return _array[index];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(int index, T value)
    {
        _array[index] = value;
        if (index < _minIndex)
            _minIndex = index;
        if (index > _maxIndex) 
            _maxIndex = index;
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetIndex(int x, int z)
    {
        return z * _sizeX + +x;
    }
    
    public void ResetAll(T value)
    {
        for (var i = _minIndex; i < _maxIndex; i++)
        {
            _array[i] = value;
        }

        _minIndex = int.MaxValue;
        _maxIndex = int.MinValue;
    }
    
    public (int Min, int Max) GetMinMax()
    {
        return (_minIndex, _maxIndex);
    }
}