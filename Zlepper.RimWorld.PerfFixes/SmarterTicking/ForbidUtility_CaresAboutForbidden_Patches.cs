using HarmonyLib;

namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

[HarmonyPatch(typeof(ForbidUtility), nameof(ForbidUtility.CaresAboutForbidden), typeof(Pawn), typeof(bool))]
public class ForbidUtility_CaresAboutForbidden_Patches
{
    private static readonly PawnLazyTicker<bool> _callTarget_true = new(120);
    private static readonly PawnLazyTicker<bool> _callTarget_false = new(120);

    public static void Tick()
    {
        _callTarget_true.Tick();
        _callTarget_false.Tick();
    }

    public static void Reset()
    {
        _callTarget_true.Reset();
        _callTarget_false.Reset();
    }

    [HarmonyPrefix]
    public static bool CaresAboutForbidden_Patch(Pawn pawn, bool cellTarget, out bool __result)
    {
        var ticker = cellTarget ? _callTarget_true : _callTarget_false;
        
        if (ticker.TryGetValue(pawn, out __result)) return false;
        
        __result = CaresAboutForbidden_Original(pawn, cellTarget);
        ticker.Set(pawn, __result);

        return false;
    }

    [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
    private static bool CaresAboutForbidden_Original(Pawn pawn, bool cellTarget)
    {
        throw new NotImplementedException("Copied in by harmony at runtime");
    }
}