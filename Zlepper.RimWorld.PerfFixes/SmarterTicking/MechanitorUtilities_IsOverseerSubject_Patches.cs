using HarmonyLib;

namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

[HarmonyPatch(typeof(MechanitorUtility), nameof(MechanitorUtility.IsOverseerSubject), typeof(Pawn))]
public static class MechanitorUtilities_IsOverseerSubject_Patches
{
    private static readonly VeryHorribleIntDictionary<bool> _cache = new();
    

    [HarmonyPrefix]
    public static bool IsOverseerSubject_Patch(Pawn pawn, out bool __result)
    {
        var id = pawn.thingIDNumber;
        if (!_cache.TryGetValue(id, out __result))
        {
            __result = IsOverseerSubject_Original(pawn);
            _cache.Set(id, __result);
        }

        return false;
    }

    [HarmonyReversePatch(HarmonyReversePatchType.Snapshot)]
    private static bool IsOverseerSubject_Original(Pawn pawn)
    {
        throw new NotImplementedException("Copied in by harmony at runtime");
    }
}