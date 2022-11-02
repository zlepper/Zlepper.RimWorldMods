using HarmonyLib;
using RimWorld.Planet;

namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

[HarmonyPatch(typeof(WorldPawns), nameof(WorldPawns.GetSituation), typeof(Pawn))]
public static class WorldPawns_GetSituation_Patches
{

    private static readonly PawnLazyTicker<WorldPawnSituation> _ticker = new(60, 120);

    [HarmonyPrefix]
    public static bool GetSituation_Prefix(Pawn p, WorldPawns __instance, out WorldPawnSituation __result)
    {
        __result = _ticker.TickAndCalculate(p, () => GetSituation_Original(__instance, p));

        return false;
    }
    
    
    [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
    public static WorldPawnSituation GetSituation_Original(WorldPawns instance, Pawn p)
    {
        throw new NotImplementedException("This method is patched by Harmony");
    }
}