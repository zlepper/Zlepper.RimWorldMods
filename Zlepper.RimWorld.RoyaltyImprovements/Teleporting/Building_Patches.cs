using HarmonyLib;

namespace Zlepper.RimWorld.RoyaltyImprovements.Teleporting;

[HarmonyPatch(typeof(Building))]
public static class Building_Patches
{
    [HarmonyPatch(nameof(Building.SpawnSetup), typeof(Map), typeof(bool))]
    [HarmonyPostfix]
    public static void AfterSpawn(Building __instance, Map map, bool respawningAfterLoad)
    {
        if (respawningAfterLoad)
        {
            return;
        }
        
        map.GetComponent<TeleporterDistanceTracker>()?.BuildingDirty(__instance);
    }

    [HarmonyPatch(nameof(Building.DeSpawn), typeof(DestroyMode))]
    [HarmonyPrefix]
    public static void BeforeDeSpawn(Building __instance)
    {
        __instance.Map?.GetComponent<TeleporterDistanceTracker>().BuildingDirty(__instance);
    }
    
}