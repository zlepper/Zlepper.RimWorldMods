using HarmonyLib;

namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

[HarmonyPatch(typeof(MechanitorUtility), nameof(MechanitorUtility.AnyMechanitorInPlayerFaction))]
public static class MechanitorUtilities_AnyMechanitorInPlayerFaction_Patches
{
    private static int _checkAgainIn = 0;

    private static bool _lastResult;

    [HarmonyPrefix]
    public static bool AnyMechanitorInPlayerFaction_Patch(out bool __result)
    {
        _checkAgainIn--;

        if (_checkAgainIn < 0)
        {
            _lastResult = AnyMechanitorInPlayerFaction_Original();
            if (_lastResult)
            {
                _checkAgainIn = int.MaxValue;
            }
            else
            {
                _checkAgainIn = 600;
            }
        }

        __result = _lastResult;

        return false;
    }

    [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
    private static bool AnyMechanitorInPlayerFaction_Original()
    {
        throw new NotImplementedException("Copied in by harmony at runtime");
    }
}