using HarmonyLib;

namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

[HarmonyPatch(typeof(MechanitorUtility), nameof(MechanitorUtility.AnyMechsInPlayerFaction))]
public static class MechanitorUtilities_AnyMechsInPlayerFaction_Patches
{
    private static int _checkAgainIn = 0;

    private static bool _lastResult;

    [HarmonyPrefix]
    public static bool AnyMechsInPlayerFaction_Patch(out bool __result)
    {
        _checkAgainIn--;

        if (_checkAgainIn < 0)
        {
            _lastResult = AnyMechsInPlayerFaction_Original();
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
    private static bool AnyMechsInPlayerFaction_Original()
    {
        throw new NotImplementedException("Copied in by harmony at runtime");
    }
}