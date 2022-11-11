using HarmonyLib;

namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

[HarmonyPatch(typeof(CompPowerPlant), nameof(CompPowerPlant.UpdateDesiredPowerOutput))]
public static class CompPowerPlant_UpdateDesiredPowerOutput_Patches
{
    private static readonly ThingTickReducer _tickReducer = new(30, 60);

    
    [HarmonyPrefix]
    public static bool Prefix(CompPowerPlant __instance)
    {
        return _tickReducer.AllowTick(__instance.parent);
    }
}