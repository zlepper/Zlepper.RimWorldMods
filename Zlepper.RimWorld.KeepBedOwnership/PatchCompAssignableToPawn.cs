using HarmonyLib;

namespace Zlepper.RimWorld.KeepBedOwnership;

[HarmonyPatch(typeof(CompAssignableToPawn), nameof(CompAssignableToPawn.AssigningCandidates), MethodType.Getter)]
internal class PatchCompAssignableToPawn
{
    [HarmonyPrefix]
    private static bool Prefix(ref IEnumerable<Pawn> __result, CompAssignableToPawn __instance)
    {
        var bed = __instance.parent as Building_Bed;
        if (!Helpers.ShouldRunForBed(bed)) return true;

        // Allow selecting any colonist on permanent bases
        if (bed?.Map.IsPlayerHome ?? false)
        {
            __result = Find.ColonistBar.GetColonistsInOrder();
            return false;
        }
        return true;
    }
}