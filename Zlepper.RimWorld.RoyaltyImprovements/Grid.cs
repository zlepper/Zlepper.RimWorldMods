using System.Runtime.CompilerServices;
using Zlepper.RimWorld.RoyaltyImprovements.Teleporting;

namespace Zlepper.RimWorld.RoyaltyImprovements;

public struct Grid<T>
{
    public T[] Cells;
    private readonly int _mapSizeX;
    private readonly int _mapSizeZ;

    public Grid(int mapSizeX, int mapSizeZ)
    {
        _mapSizeX = mapSizeX;
        _mapSizeZ = mapSizeZ;
        Cells = new T[_mapSizeX * _mapSizeZ];
    }

    public Grid(IntVec3 size) : this(size.x, size.z)
    {
    }

    public Grid(MapAdapter map) : this(map.Size)
    {
    }

    public T this[int x, int z]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[x + z * _mapSizeX];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => this[x + z * _mapSizeX] = value;
    }

    public T this[IntVec3 cell]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => this[cell.x, cell.z];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => this[cell.x, cell.z] = value;
    }

    public T this[int index]
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Cells[index];
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        set => Cells[index] = value;
    }

    public int Length
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Cells.Length;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool InBounds(IntVec3 cell)
    {
        return cell.x >= 0 && cell.x < _mapSizeX && cell.z >= 0 && cell.z < _mapSizeZ;
    }

    public Grid<TResult> Select<TResult>(Func<T, TResult> selector)
    {
        var result = new Grid<TResult>(_mapSizeX, _mapSizeZ);
        for (var i = 0; i < Cells.Length; i++)
        {
            result.Cells[i] = selector(Cells[i]);
        }

        return result;
    }

    public void SetAll(T value)
    {
        for (var i = 0; i < Cells.Length; i++)
        {
            Cells[i] = value;
        }
    }
}