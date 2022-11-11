using HarmonyLib;

namespace Zlepper.RimWorld.KeepBedOwnership;

[HarmonyPatch(typeof(Pawn_Ownership), nameof(Pawn_Ownership.ClaimBedIfNonMedical))]
internal class PatchClaimBedIfNonMedical
{
    [HarmonyPrefix]
    private static bool Prefix(Building_Bed? newBed, ref Pawn ___pawn, ref Building_Bed? ___intOwnedBed)
    {
        if (newBed == null
            || !Helpers.ShouldRunForPawn(___pawn)
            || !Helpers.ShouldRunForBed(newBed)
            || (newBed.OwnersForReading != null && newBed.OwnersForReading.Contains(___pawn) && ___pawn.ownership?.OwnedBed == newBed))
        {
            return true;
        }

        // Remove other pawn to make room in bed
        var pawn = ___pawn;
        if (newBed.OwnersForReading?.Count == newBed.SleepingSlotsCount
            && !newBed.OwnersForReading.Any(p => p == pawn)
            && newBed.OwnersForReading.Count > 0)
        {
            var pawnToRemove = newBed.OwnersForReading.LastOrDefault();
            pawnToRemove?.ownership?.UnclaimBed();
        }

        // Unclaim bed if pawn already has one on the map of the new bed
        var pawnBeds = Helpers.PawnBedsOnMap(___pawn, newBed.Map);
        if (pawnBeds.Any())
        {
            Helpers.UnclaimBeds(___pawn, pawnBeds, ref ___intOwnedBed);
        }

        // Claim new bed
        newBed.CompAssignableToPawn.ForceAddPawn(___pawn);
        // ... but only assign it if the pawn is on the same map
        if (___pawn.Map == newBed.Map)
        {
            ___intOwnedBed = newBed;
            ThoughtUtility.RemovePositiveBedroomThoughts(___pawn);
        }

        return false;
    }
}