using System.Runtime.CompilerServices;
using Verse.AI;

namespace Zlepper.RimWorld.PerfFixes;

public sealed class FasterReachability
{
    private readonly Map _map;
    private readonly int _mapSizeX;

    private MapArray<ConnectedArea?> _connectedAreas;
    private readonly ObjectPool<ConnectedArea> _areaPool;

    public FasterReachability(Map map)
    {
        _map = map;
        _mapSizeX = map.Size.x;

        _connectedAreas = new MapArray<ConnectedArea?>(map);
        _areaPool = new ObjectPool<ConnectedArea>(() => new ConnectedArea(map.Size));
    }

    public bool CanReach(
        IntVec3 start,
        LocalTargetInfo dest,
        PathEndMode peMode,
        TraverseParms traverseParams)
    {
        throw new NotImplementedException();
    }

    internal void BuildConnectedAreas()
    {
        var sizeX = _map.Size.x;
        var sizeZ = _map.Size.z;


        for (var x = 0; x < sizeX; x++)
        {
            for (var z = 0; z < sizeZ; z++)
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
    }

    private ConnectedArea GetArea(int index)
    {
        ConnectedArea? areaNorth = null, areaEast = null;
        if (index > 0)
        {
            areaEast = _connectedAreas.Get(index - 1);
        }

        if (index > _mapSizeX)
        {
            areaNorth = _connectedAreas.Get(index - _mapSizeX);
        }

        if (areaEast == null)
        {
            if(areaNorth == null)
            {
                return _areaPool.Get();
            }
            
            return areaNorth;
        }

        if (areaNorth == null || areaEast.Equals(areaNorth))
        {
            return areaEast;
        }

        // Area to the north and east, merge them
        return MergeAreas(areaEast, areaNorth);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private int GetIndex(int x, int z)
    {
        return z * _mapSizeX + x;
    }

    private bool IsPassable(int index)
    {
        if (_map.fogGrid.IsFogged(index))
            return false;

        if (_map.terrainGrid.TerrainAt(index).passability == Traversability.Impassable)
            return false;

        var thingList = _map.thingGrid.ThingsListAtFast(index);

        var thingCount = thingList.Count;
        for (var i = 0; i < thingCount; i++)
        {
            if (thingList[i].def.passability == Traversability.Impassable)
            {
                return false;
            }
            
        }

        return true;
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