using HarmonyLib;
using RimWorld.Planet;

namespace Zlepper.RimWorld.PerfFixes;

[HarmonyPatch(typeof(FormCaravanComp), nameof(FormCaravanComp.AnyActiveThreatNow), MethodType.Getter)]
public class FormCaravanComp_Patches
{
    [HarmonyPrefix]
    static bool AnyActiveThreatNowPrefix(ref bool __result, FormCaravanComp __instance)
    {
        var mapParent = (MapParent) __instance.parent;

        var map = mapParent.Map;
        if (map == null)
        {
            __result = false;
        }
        else
        {
            __result = GenHostility.AnyHostileActiveThreatToPlayer(map, true);
        }

        return false;
    }
}