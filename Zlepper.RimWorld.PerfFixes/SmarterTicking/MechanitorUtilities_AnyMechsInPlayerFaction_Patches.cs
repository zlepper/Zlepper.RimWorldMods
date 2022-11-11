using HarmonyLib;

namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

[HarmonyPatch(typeof(MechanitorUtility), nameof(MechanitorUtility.AnyMechsInPlayerFaction))]
public static class MechanitorUtilities_AnyMechsInPlayerFaction_Patches
{
    [HarmonyPrefix]
    public static bool AnyMechsInPlayerFaction_Patch(out bool __result)
    {
        __result = true;

        return false;
    }
}