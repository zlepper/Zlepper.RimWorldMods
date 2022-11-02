using System.Runtime.CompilerServices;
using Verse.AI;

namespace Zlepper.RimWorld.PerfFixes.FastReachability;

public sealed class FasterReachability
{
    private readonly Map _map;
    private readonly FogGrid _fogGrid;
    private readonly TerrainGrid _terrainGrid;
    private readonly ThingGrid _thingGrid;

    private readonly int _mapSizeX;
    private readonly int _mapSizeZ;

    private readonly MapArray<ConnectedArea?> _connectedAreas;
    private readonly ObjectPool<ConnectedArea> _areaPool;

    private bool _dirty = true;

    public FasterReachability(Map map)
    {
        _map = map ?? throw new ArgumentNullException(nameof(map));
        _mapSizeX = map.Size.x;
        _mapSizeZ = map.Size.z;
        _fogGrid = map.fogGrid ?? throw new ArgumentNullException(nameof(map.fogGrid));
        _terrainGrid = map.terrainGrid ?? throw new ArgumentNullException(nameof(map.terrainGrid));
        _thingGrid = map.thingGrid ?? throw new ArgumentNullException(nameof(map.thingGrid));

        _connectedAreas = new MapArray<ConnectedArea?>(map);
        _areaPool = new ObjectPool<ConnectedArea>(() => new ConnectedArea(map.Size));
    }

    public bool CanReach(
        IntVec3 start,
        LocalTargetInfo dest,
        PathEndMode peMode,
        TraverseParms traverseParams
    )
    {
        if (traverseParams.pawn != null)
        {
            if (!traverseParams.pawn.Spawned)
                return false;
            if (traverseParams.pawn.Map != _map)
            {
                Log.Error(
                    "Called CanReach() with a pawn spawned not on this map. This means that we can't check his reachability here. Pawn's current map should have been used instead of this one. pawn=" +
                    traverseParams.pawn + " pawn.Map=" + traverseParams.pawn.Map + " map=" + _map);
                return false;
            }
        }

        if (!start.InBounds(_map))
            return false;

        if (!dest.IsValid || dest.HasThing && dest.Thing.MapHeld != _map || !dest.Cell.InBounds(_map))
            return false;

        if (_dirty)
        {
            ReBuildConnectedAreas();
        }

        var startArea = _connectedAreas.Get(start.x, start.y);
        if (startArea == null)
        {
            Log.Warning($"Start cell {start} is not reachable. This should never happen.");
            return false;
        }

        foreach (var destIndex in PositionsToTry(peMode, dest.Cell))
        {
            var area = _connectedAreas.Get(destIndex);
            if (area == null)
            {
                continue;
            }

            if (startArea.UniqueId == area.UniqueId)
            {
                return true;
            }
        }

        return false;
    }

    private static readonly int[] _circleOffsets = {-1, 0, 1};

    private IEnumerable<int> PositionsToTry(PathEndMode peMode, IntVec3 dest)
    {
        var x = dest.x;
        var z = dest.z;
        switch (peMode)
        {
            case PathEndMode.OnCell:
            case PathEndMode.InteractionCell:
                yield return GetIndex(x, z);
                break;
            case PathEndMode.Touch:
            case PathEndMode.ClosestTouch:

                for (var i = 0; i < _circleOffsets.Length; i++)
                {
                    for (var j = 0; j < _circleOffsets.Length; j++)
                    {
                        var xOffset = x + _circleOffsets[i];
                        var zOffset = z + _circleOffsets[j];

                        if (xOffset < 0 || zOffset < 0 || xOffset >= _mapSizeX || zOffset >= _mapSizeZ)
                            continue;

                        yield return GetIndex(xOffset, zOffset);
                    }
                }

                break;
            case PathEndMode.None:
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(peMode), peMode,
                    $"Unknown path end mode: {peMode}, ({(int) peMode})");
        }
    }

    internal void ReBuildConnectedAreas()
    {
        Log.Message("Doing rebuild");
        _connectedAreas.ResetAll(null);
        BuildConnectedAreas();
    }

    internal void MarkDirty()
    {
        _dirty = true;
    }
    
    internal void BuildConnectedAreas()
    {
        var sizeX = _map.Size.x;
        var sizeZ = _map.Size.z;
        
        for (var z = 0; z < sizeZ; z++)
        {
            for (var x = 0; x < sizeX; x++)
            {
                var index = GetIndex(x, z);
                if (!IsPassable(index))
                {
                    continue;
                }

                var area = GetArea(index);

                area.AddCell(index);
                _connectedAreas.Set(index, area);
            }
        }

        _dirty = false;
    }

    private ConnectedArea GetArea(int index)
    {
        ConnectedArea? areaSouth = null, areaEast = null;
        var eastIndex = index - 1;
        var southIndex = index - _mapSizeX;

        if (eastIndex >= 0)
        {
            areaEast = _connectedAreas.Get(eastIndex);
        }

        if (southIndex >= 0)
        {
            areaSouth = _connectedAreas.Get(southIndex);
        }

        if (areaEast == null)
        {
            if (areaSouth == null)
            {
                return _areaPool.Get();
            }

            return areaSouth;
        }

        if (areaSouth == null)
        {
            return areaEast;
        }

        if (areaEast.Equals(areaSouth))
        {
            return areaEast;
        }


        // Area to the north and east, merge them
        return MergeAreas(areaEast, areaSouth);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetIndex(int x, int z)
    {
        return z * _mapSizeX + x;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool IsFogged(int index)
    {
        var grid = _fogGrid.fogGrid;
        if (grid == null)
        {
            return false;
        }

        return grid[index];
    }

    private bool IsPassable(int index)
    {
        try
        {
            if (IsFogged(index))
                return false;

            var terrainDef = _terrainGrid.TerrainAt(index);
            if (terrainDef == null)
            {
                Log.Message("TerrainDef is null");
            }

            if (terrainDef == null || terrainDef.passability == Traversability.Impassable)
                return false;

            var thingList = _thingGrid.ThingsListAtFast(index);
            if (thingList == null)
            {
                Log.WarningOnce($"ThingList is null at {index}", 12345652);
                return true;
            }

            var thingCount = thingList.Count;
            for (var i = 0; i < thingCount; i++)
            {
                var thingDef = thingList[i].def;
                if (thingDef == null)
                {
                    Log.Message("ThingDef is null");
                    continue;
                }

                if (thingDef.passability == Traversability.Impassable)
                {
                    return false;
                }
            }

            return true;
        }
        catch (NullReferenceException e)
        {
            Log.Error($"Something was null at index {index}: {e}");
            return false;
        }
    }

    private ConnectedArea MergeAreas(ConnectedArea largestArea, ConnectedArea smallestArea)
    {
        // Ensure we always merge the smallest area into the larger one
        if (largestArea.Size < smallestArea.Size)
        {
            (largestArea, smallestArea) = (smallestArea, largestArea);
        }

        var (min, max) = smallestArea.GetMinMax();
        for (var index = min; index < max; index++)
        {
            if (smallestArea.ContainsCell(index))
            {
                largestArea.AddCell(index);
                _connectedAreas.Set(index, largestArea);
            }
        }

        _areaPool.Return(smallestArea);

        return largestArea;
    }

    public ConnectedArea? GetConnectedArea(int cellX, int cellZ)
    {
        return _connectedAreas.Get(cellX, cellZ);
    }
}