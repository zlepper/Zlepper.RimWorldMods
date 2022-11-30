using HarmonyLib;

namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

[HarmonyPatch(typeof(Toils_Bed), nameof(Toils_Bed.BedNoLongerUsable), typeof(Pawn), typeof(Thing), typeof(bool))]
public class Toils_BedPatches
{
    public static readonly PawnLazyTicker<bool> Ticker = new(120);

    [HarmonyPrefix]
    public static bool BedNoLongerUsable_Prefix(Pawn actor, Thing bedThing, bool forcePrisoner, out bool __result)
    {
        if (Ticker.TryGetValue(actor, out __result)) return false;
        
        __result = BedNoLongerUsable_Original(actor, bedThing, forcePrisoner);
        Ticker.Set(actor, __result);

        return false;
    }

    [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
    private static bool BedNoLongerUsable_Original(Pawn actor, Thing bedThing, bool forcePrisoner)
    {
        throw new NotImplementedException("Copied in by harmony at runtime");
    }
}