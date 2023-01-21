using UnityEngine;

namespace Zlepper.RimWorld.RoyaltyImprovements.Teleporting;

public static class PathFinderInjectedMethods
{
    
    public static void TryUseSkipdoor(ref int gridIndex, Pawn? pawn, LocalTargetInfo destination)
    {
        if (pawn == null)
        {
            return;
        }
        var map = pawn.Map;
        if (map == null)
        {
            return;
        }

        if (!(pawn.Faction?.IsPlayer ?? false) || pawn.roping.HasAnyRope)
        {
            return;
        }

        var tracker = _cache.Get(map);
        if (tracker == null!)
        {
            return;
        }
        
        var skipdoor = tracker.GetTeleporter(gridIndex);
        if (skipdoor == null)
        {
            return;
        }

        var bestSkipDoor = tracker.GetClosestTeleporter(destination.Cell);
        if (bestSkipDoor == null || skipdoor == bestSkipDoor)
        {
            return;
        }

        gridIndex = map.cellIndices.CellToIndex(bestSkipDoor.Position);
    }

    private static readonly ValueCache<Map, TeleporterDistanceTracker>
        _cache = new(m => m.GetComponent<TeleporterDistanceTracker>());


    public static int DistanceWithTeleport(Pawn? traversingPawn, int fromCellX, int fromCellZ, int toCellX, int toCellZ,
        int cardinalSpeed, int ordinalSpeed)
    {
        var directDistance = GenMath.OctileDistance(Math.Abs(toCellX - fromCellX), Math.Abs(toCellZ - fromCellZ),
            cardinalSpeed, ordinalSpeed);
        if (traversingPawn?.Map == null || !(traversingPawn.Faction?.IsPlayer ?? false) || traversingPawn.roping.HasAnyRope)
        {
            return directDistance;
        }

        var tracker = _cache.Get(traversingPawn.Map);
        if (tracker == null!)
        {
            return directDistance;
        }

        var skipdoorDistance =
            tracker.GetTeleporterDistance(fromCellX, fromCellZ, toCellX, toCellZ, cardinalSpeed, ordinalSpeed);
        return Mathf.Min(skipdoorDistance, directDistance);
    }
}