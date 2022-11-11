using HarmonyLib;

namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

[HarmonyPatch(typeof(MechanitorUtility), nameof(MechanitorUtility.IsOverseerSubject), typeof(Pawn))]
public static class MechanitorUtilities_IsOverseerSubject_Patches
{
    [HarmonyPrefix]
    public static bool IsOverseerSubject_Patch(Pawn pawn, out bool __result)
    {
        var context = ThingContext.Get(pawn);
        if (!context.IsOverseerSubject.HasValue)
        {
            context.IsOverseerSubject = IsOverseerSubject_Original(pawn);
        }
        
        __result = context.IsOverseerSubject.Value;

        return false;
    }

    [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
    private static bool IsOverseerSubject_Original(Pawn pawn)
    {
        throw new NotImplementedException("Copied in by harmony at runtime");
    }
}