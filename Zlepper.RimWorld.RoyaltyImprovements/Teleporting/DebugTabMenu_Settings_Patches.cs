using System.Reflection;
using HarmonyLib;

namespace Zlepper.RimWorld.RoyaltyImprovements.Teleporting;

[HarmonyPatch(typeof(DebugTabMenu_Settings))]
public static class DebugTabMenu_Settings_Patches
{
    [HarmonyPatch(nameof(DebugTabMenu_Settings.InitActions), typeof(DebugActionNode))]
    [HarmonyPostfix]
    public static void Postfix(DebugActionNode __result, DebugActionNode __instance)
    {
        foreach (var fieldInfo in typeof(TeleporterDebugSettings).GetFields(BindingFlags.Public|BindingFlags.Static))
        {
            AddNode(__instance, fieldInfo, "View");
        }
    }

    [HarmonyPatch(nameof(AddNode), typeof(FieldInfo), typeof(string))]
    [HarmonyReversePatch]
    public static void AddNode(object instance, FieldInfo fi, string categoryLabel)
    {
        throw new InvalidOperationException("Reverse patch failed");
    }
}