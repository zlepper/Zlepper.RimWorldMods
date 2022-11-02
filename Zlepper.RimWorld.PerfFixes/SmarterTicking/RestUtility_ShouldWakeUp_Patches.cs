using HarmonyLib;

namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

[HarmonyPatch(typeof(RestUtility), nameof(RestUtility.ShouldWakeUp), typeof(Pawn))]
public static class RestUtility_ShouldWakeUp_Patches
{
    private static readonly PawnLazyTicker<bool> _cache = new(80, 160);
    

    [HarmonyPrefix]
    public static bool ShouldWakeUp_Patch(Pawn pawn, out bool __result)
    {
        __result = _cache.TickAndCalculate(pawn, () => RestUtility.ShouldWakeUp(pawn));

        return false;
    }

    [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
    private static bool ShouldWakeUp_Original(Pawn pawn)
    {
        throw new NotImplementedException("Copied in by harmony at runtime");
    }
}