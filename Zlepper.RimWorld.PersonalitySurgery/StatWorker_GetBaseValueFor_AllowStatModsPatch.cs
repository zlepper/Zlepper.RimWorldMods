using HarmonyLib;

namespace Zlepper.RimWorld.PersonalitySurgery;

[HarmonyPatch(typeof(StatWorker), nameof(StatWorker.GetBaseValueFor), typeof(StatRequest))]
public class StatWorker_GetBaseValueFor_AllowStatModsPatch
{
    [HarmonyPostfix]
    public static void Postfix(StatDef ___stat, ref float __result, StatRequest request)
    {
        if (request.Thing is IModifyStat modifier)
        {
            __result = modifier.PostGetBaseValueFor(___stat, __result);
        }
    }
}

public interface IModifyStat
{
    float PostGetBaseValueFor(StatDef stat, float baseValue);
}