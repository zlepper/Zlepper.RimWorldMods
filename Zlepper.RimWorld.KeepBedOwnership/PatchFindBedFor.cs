using HarmonyLib;

namespace Zlepper.RimWorld.KeepBedOwnership;

/// Make sure pawns pick their current beds when looking for a normal bed to sleep
[HarmonyPatch(typeof(RestUtility), nameof(RestUtility.FindBedFor), typeof(Pawn), typeof(Pawn), typeof(bool), typeof(bool), typeof(GuestStatus?))]
internal class PatchFindBedFor
{
    [HarmonyPostfix]
    private static void Postfix(Pawn sleeper, ref Building_Bed? __result)
    {
        if (__result == null || !Helpers.ShouldRunForBed(__result) || !Helpers.ShouldRunForPawn(sleeper)) return;
        var currentBeds = Helpers.PawnBedsOnMap(sleeper, sleeper.Map);
        
        currentBeds = currentBeds
            .Where(bed =>
                sleeper.Map.reachability.CanReach(sleeper.Position, new LocalTargetInfo(bed), Verse.AI.PathEndMode.OnCell, TraverseParms.For(sleeper))
                && ForbidUtility.InAllowedArea(bed.Position, sleeper))
            .ToList();
        
        if (currentBeds.Count > 0)
        {
            __result = currentBeds[0];
        }
    }
}