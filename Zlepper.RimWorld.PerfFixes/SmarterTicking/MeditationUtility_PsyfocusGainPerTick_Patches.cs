using HarmonyLib;

namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

[HarmonyPatch(typeof(MeditationUtility), nameof(MeditationUtility.PsyfocusGainPerTick), typeof(Pawn), typeof(Thing))]
public class MeditationUtility_PsyfocusGainPerTick_Patches
{
    private static readonly Dictionary<int, Dictionary<int, float>> Cache = new();
    private static readonly TickQueue _tickQueue = new(120);


    public static void Tick()
    {
        foreach (var id in _tickQueue.Next())
        {
            Cache.Remove(id);
        }
    }

    [HarmonyPrefix]
    public static bool Prefix(Pawn pawn, Thing? focus, ref float __result)
    {
        var focusThingId = focus?.thingIDNumber ?? -1;
        if (Cache.TryGetValue(pawn.thingIDNumber, out var focusCache))
        {
            if (focusCache.TryGetValue(focusThingId, out __result))
            {
                return false;
            }
        }

        return true;
    }

    [HarmonyPostfix]
    public static void Postfix(Pawn pawn, Thing? focus, ref float __result, bool __runOriginal)
    {
        if (__runOriginal)
        {
            var focusThingId = focus?.thingIDNumber ?? -1;
            if (Cache.TryGetValue(pawn.thingIDNumber, out var focusCache))
            {
                focusCache[focusThingId] = __result;
            }
            else
            {
                _tickQueue.Add(pawn.thingIDNumber);
                Cache[pawn.thingIDNumber] = new Dictionary<int, float> { { focusThingId, __result } };
            }
        }
    }
}