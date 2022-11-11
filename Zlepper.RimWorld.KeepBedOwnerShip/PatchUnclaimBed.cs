using HarmonyLib;

namespace Zlepper.RimWorld.KeepBedOwnerShip;

/// Temporarily replace pawns owned bed on their map with the bed owned on the current map in order to
/// unclaim the correct bed
[HarmonyPatch(typeof(Pawn_Ownership), nameof(Pawn_Ownership.UnclaimBed))]
internal class PatchUnclaimBed
{
    [HarmonyPrefix]
    private static bool Prefix(ref Pawn ___pawn, ref Building_Bed? ___intOwnedBed)
    {
        UnassignBedsForOldPawns(___pawn);
        if (!Helpers.ShouldRunForPawn(___pawn)) return true;

        var isTeleporting = ___pawn.teleporting;
        var isInShuttle = !___pawn.Spawned && ___pawn.SpawnedParentOrMe?.Label?.Contains("shuttle") == true;
        if (isInShuttle || isTeleporting)
        {
            ___intOwnedBed = null;
            return true;
        }

        // NOTE: If the bed is unclaimed (typically deconstructed/replaced) on another map this will cause the pawn
        // to unclaim the bed on CurrentMap. Since UnclaimBed doesn't specify bed we have to guess, and since it's
        // called from a bunch of places in vanilla (plus whatever from mods) I'd rather just take the occasional
        // unwanted unclaim instead of trying to patch everywhere.

        // Temporarily replace pawns owned bed on their map with the bed owned on the current map
        ClaimBedOnMapIfExists(___pawn, Find.CurrentMap, ref ___intOwnedBed);
        return true;
    }

    [HarmonyPostfix]
    private static void Postfix(ref Pawn ___pawn, ref Building_Bed? ___intOwnedBed)
    {
        var pawnMap = ___pawn.Map ?? ___pawn.MapHeld ?? ___pawn.SpawnedParentOrMe?.Map;
        // Return pawn owned bed to their current map
        if (Helpers.ShouldRunForPawn(___pawn) && pawnMap != null)
        {
            ClaimBedOnMapIfExists(___pawn, pawnMap, ref ___intOwnedBed);
        }
    }

    private static void ClaimBedOnMapIfExists(Pawn ___pawn, Map map, ref Building_Bed? ___intOwnedBed)
    {
        var pawnBedsOnMap = Helpers.PawnBedsOnMap(___pawn, map);
        var bed = pawnBedsOnMap.FirstOrDefault();
        if (bed != null)
        {
            ___intOwnedBed = bed;
            if (bed.CompAssignableToPawn != null && !bed.CompAssignableToPawn.AssignedPawnsForReading.Contains(___pawn))
            {
                bed.CompAssignableToPawn.ForceAddPawn(___pawn);
            }
        }
    }

    private static void UnassignBedsForOldPawns(Pawn? pawn)
    {
        if (pawn == null) return;
        var isDead = pawn.Dead;
        var isDespawnedNonColonist = !pawn.IsFreeColonist && !pawn.Spawned;
        var isImprisonedColonist = pawn.IsColonist && pawn.IsPrisoner;
        if (isDead || isDespawnedNonColonist || isImprisonedColonist)
        {
            var pawnBeds = Find.Maps.SelectMany(map => Helpers.PawnBedsOnMap(pawn, map));
            Building_Bed? noBed = null;
            Helpers.UnclaimBeds(pawn, pawnBeds, ref noBed);
        }
    }
}