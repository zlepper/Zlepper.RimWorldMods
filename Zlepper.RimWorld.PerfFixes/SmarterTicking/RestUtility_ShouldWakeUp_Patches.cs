using HarmonyLib;

namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

[HarmonyPatch(typeof(RestUtility), nameof(RestUtility.ShouldWakeUp), typeof(Pawn))]
public static class RestUtility_ShouldWakeUp_Patches
{
    public static readonly PawnLazyTicker<bool> Ticker = new(160);
    

    [HarmonyPrefix]
    public static bool ShouldWakeUp_Patch(Pawn pawn, out bool __result)
    {
        if (Ticker.TryGetValue(pawn, out __result)) return false;
        
        __result = ShouldWakeUp_Original(pawn);
        Ticker.Set(pawn, __result);

        return false;
    }

    [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
    private static bool ShouldWakeUp_Original(Pawn pawn)
    {
        throw new NotImplementedException("Copied in by harmony at runtime");
    }
}