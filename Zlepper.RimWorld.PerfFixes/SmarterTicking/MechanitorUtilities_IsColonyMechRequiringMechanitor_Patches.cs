using HarmonyLib;

namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

[HarmonyPatch(typeof(MechanitorUtility), nameof(MechanitorUtility.IsColonyMechRequiringMechanitor), typeof(Pawn))]
public static class MechanitorUtilities_IsColonyMechRequiringMechanitor_Patches
{
    private static readonly PawnLazyTicker<bool> _ticker = new(800, 1100);

    [HarmonyPrefix]
    public static bool IsColonyMechRequiringMechanitor_Patch(Pawn mech, out bool __result)
    {
        __result = _ticker.TickAndCalculate(mech, () => IsColonyMechRequiringMechanitor_Original(mech));

        return false;
    }

    [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
    private static bool IsColonyMechRequiringMechanitor_Original(Pawn mech)
    {
        throw new NotImplementedException("Copied in by harmony at runtime");
    }
}