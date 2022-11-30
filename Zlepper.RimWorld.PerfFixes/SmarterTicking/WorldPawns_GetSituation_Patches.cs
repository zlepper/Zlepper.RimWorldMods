using HarmonyLib;
using RimWorld.Planet;

namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

[HarmonyPatch(typeof(WorldPawns), nameof(WorldPawns.GetSituation), typeof(Pawn))]
public static class WorldPawns_GetSituation_Patches
{

    public static readonly PawnLazyTicker<WorldPawnSituation> Ticker = new(120);

    [HarmonyPrefix]
    public static bool GetSituation_Prefix(Pawn p, WorldPawns __instance, out WorldPawnSituation __result)
    {
        if (Ticker.TryGetValue(p, out __result)) return false;
        
        __result = GetSituation_Original(__instance, p);
        Ticker.Set(p, __result);

        return false;
    }
    
    
    [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
    public static WorldPawnSituation GetSituation_Original(WorldPawns instance, Pawn p)
    {
        throw new NotImplementedException("This method is patched by Harmony");
    }
}