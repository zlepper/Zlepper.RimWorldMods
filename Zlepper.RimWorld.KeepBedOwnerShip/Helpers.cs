namespace Zlepper.RimWorld.KeepBedOwnerShip;

internal static class Helpers
{
    public static List<Building_Bed> PawnBedsOnMap(Pawn ___pawn, Map? map)
    {
        return map?.listerThings?.ThingsInGroup(ThingRequestGroup.Bed)
            .OfType<Building_Bed>()
            .Where(b => b.OwnersForReading != null && b.OwnersForReading.Contains(___pawn))
            .ToList() ?? new List<Building_Bed>();
    }

    public static void UnclaimBeds(Pawn pawn, IEnumerable<Building_Bed?> beds, ref Building_Bed? ___intOwnedBed)
    {
        foreach (var bed in beds)
        {
            bed?.CompAssignableToPawn?.ForceRemovePawn(pawn);
            if (pawn.ownership?.OwnedBed == bed)
            {
                ___intOwnedBed = null;
            }
        }
    }

    public static bool ShouldRunForPawn(Pawn? pawn)
    {
        return pawn is {IsFreeColonist: true, Dead: false};
    }

    public static bool ShouldRunForBed(Building_Bed? bed)
    {
        if (bed is not {Spawned: true} || bed.ForPrisoners || bed.Map == null || bed.Medical) return false;
        return !bed.GetType().ToString().Contains("WhatTheHack");
    }
}