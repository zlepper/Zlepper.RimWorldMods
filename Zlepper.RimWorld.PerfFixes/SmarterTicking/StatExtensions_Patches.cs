using HarmonyLib;

namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

[HarmonyPatch(typeof(StatExtension), nameof(StatExtension.GetStatValue), typeof(Thing), typeof(StatDef), typeof(bool), typeof(int))]
public class StatExtensions_Patches
{
    [HarmonyPrefix]
    public static void Prefix(ref int cacheStaleAfterTicks)
    {
        if (cacheStaleAfterTicks == -1)
        {
            cacheStaleAfterTicks = 10;
        }
    }
}