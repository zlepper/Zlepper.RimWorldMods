using HarmonyLib;

namespace Zlepper.RimWorld.KeepBedOwnerShip;

[HarmonyPatch(typeof(CompAssignableToPawn_Bed), nameof(CompAssignableToPawn_Bed.AssignedAnything), typeof(Pawn))]
internal class PatchCompAssignableToPawn_Bed
{
    [HarmonyPrefix]
    private static bool Prefix(ref bool __result, Pawn pawn)
    {
        if (!Helpers.ShouldRunForPawn(pawn)) return true;
        // This is only used to display pawn list, so use the pawn ownership on the current map instead of their current bed
        var pawnBeds = Helpers.PawnBedsOnMap(pawn, Find.CurrentMap);
        __result = pawnBeds.Any();
        return false;
    }
}