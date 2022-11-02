using HarmonyLib;

namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

[HarmonyPatch(typeof(Toils_Bed), nameof(Toils_Bed.BedNoLongerUsable), typeof(Pawn), typeof(Thing), typeof(bool))]
public class Toils_BedPatches
{
    private static readonly PawnLazyTicker<bool> _ticker = new(60, 120);

    [HarmonyPrefix]
    public static bool BedNoLongerUsable_Prefix(Pawn actor, Thing bedThing, bool forcePrisoner, out bool __result)
    {
        __result =  _ticker.TickAndCalculate(actor, () => BedNoLongerUsable_Original(actor, bedThing, forcePrisoner));

        return false;
    }

    [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
    private static bool BedNoLongerUsable_Original(Pawn actor, Thing bedThing, bool forcePrisoner)
    {
        throw new NotImplementedException("Copied in by harmony at runtime");
    }
}