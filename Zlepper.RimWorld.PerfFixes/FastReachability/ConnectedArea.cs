using System.Runtime.CompilerServices;

namespace Zlepper.RimWorld.PerfFixes.FastReachability;

public sealed class ConnectedArea : IPoolable
{
    private MapArray<bool> _includedCells;
    private static int _nextId;

    public int UniqueId { get; private set; }
    public int Size { get; private set; }

    public ConnectedArea(IntVec3 size)
    {
        _includedCells = new MapArray<bool>(size);
        UniqueId = _nextId++;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsCell(int x, int z)
    {
        return _includedCells.Get(x, z);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsCell(int index)
    {
        return _includedCells.Get(index);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddCell(int index)
    {
        _includedCells.Set(index, true);
        Size++;
    }

    public void Reset()
    {
        _includedCells.ResetAll(false);
    }


    public (int Min, int Max) GetMinMax()
    {
        return _includedCells.GetMinMax();
    }
}
