namespace Zlepper.RimWorld.RoyaltyImprovements;

public abstract class MapAdapter
{
    public readonly IntVec3 Size;

    protected MapAdapter(IntVec3 size)
    {
        Size = size;
    }

    public abstract bool CanEnter(int index);

    public abstract int GetPathCost(int index, int previousIndex);

    public static implicit operator MapAdapter(Map map) => new RimWorldMapAdapter(map);

    private class RimWorldMapAdapter : MapAdapter
    {
        private readonly Map _map;
        private readonly Faction _playerFaction = Faction.OfPlayer;

        public RimWorldMapAdapter(Map map) : base(map.Size)
        {
            _map = map;
        }

        public override bool CanEnter(int index)
        {
            var building = _map.edificeGrid[index];
            if (building != null)
            {
                if (building is Building_Door door)
                {
                    if (door.IsForbidden(_playerFaction))
                    {
                        return false;
                    }
                }

                if (building.def.passability == Traversability.Impassable)
                {
                    return false;
                }
            }

            var terrainDef = _map.terrainGrid.TerrainAt(index);
            if (terrainDef.passability == Traversability.Impassable)
            {
                return false;
            }

            return true;
        }

        public override int GetPathCost(int index, int previousIndex)
        {
            // Most of this is copy-pasted from the PathGrid::CalculatedCostAt method.
            var terrainDef = _map.terrainGrid.TerrainAt(index);
            if (terrainDef == null || terrainDef.passability == Traversability.Impassable)
            {
                return 100000;
            }

            var cost = terrainDef.pathCost;
            var passedFromDoor = false;

            var things = _map.thingGrid.ThingsListAtFast(index);
            foreach (var thing in things)
            {
                if (thing.def.passability == Traversability.Impassable)
                {
                    return 100000;
                }

                if (!IsPathCostIgnoreRepeater(thing.def) || !ContainsPathCostIgnoreRepeater(previousIndex))
                {
                    if (thing.def.pathCost > cost)
                    {
                        cost = thing.def.pathCost;
                    }
                }

                if (thing is Building_Door)
                {
                    var edifice = _map.edificeGrid[previousIndex];
                    if (edifice is Building_Door)
                    {
                        passedFromDoor = true;
                    }
                }
            }

            if (passedFromDoor)
            {
                cost += 45;
            }

            return cost;
        }

        private bool ContainsPathCostIgnoreRepeater(int index)
        {
            var things = _map.thingGrid.ThingsListAtFast(index);
            for (var i = 0; i < things.Count; i++)
            {
                if (IsPathCostIgnoreRepeater(things[i].def))
                {
                    return true;
                }
            }

            return false;
        }

        private static bool IsPathCostIgnoreRepeater(ThingDef def) => def.pathCost >= 25 && def.pathCostIgnoreRepeat;
    }

    public bool CanEnter(IntVec3 cell)
    {
        return CanEnter(cell.x + cell.z * Size.x);
    }
}