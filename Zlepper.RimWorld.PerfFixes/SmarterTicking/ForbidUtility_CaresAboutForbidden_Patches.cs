using HarmonyLib;

namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

[HarmonyPatch(typeof(ForbidUtility), nameof(ForbidUtility.CaresAboutForbidden), typeof(Pawn), typeof(bool))]
public class ForbidUtility_CaresAboutForbidden_Patches
{
    private static readonly PawnLazyTicker<bool> _callTarget_true = new(800, 1100);
    private static readonly PawnLazyTicker<bool> _callTarget_false = new(800, 1100);

    [HarmonyPrefix]
    public static bool CaresAboutForbidden_Patch(Pawn pawn, bool cellTarget, out bool __result)
    {
        var ticker = cellTarget ? _callTarget_true : _callTarget_false;
        __result = ticker.TickAndCalculate(pawn, () => CaresAboutForbidden_Original(pawn, cellTarget));

        return false;
    }

    [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
    private static bool CaresAboutForbidden_Original(Pawn pawn, bool cellTarget)
    {
        throw new NotImplementedException("Copied in by harmony at runtime");
    }
}