using HarmonyLib;
using RimWorld.Planet;

namespace Zlepper.RimWorld.RoyaltyImprovements.Teleporting;

[HarmonyPatch(typeof(MapInterface))]
public class MapInterface_MapInterfaceUpdate_Patch
{
    [HarmonyPatch(nameof(MapInterface.MapInterfaceUpdate))]
    [HarmonyPostfix]
    public static void Postfix_MapInterfaceUpdate()
    {
        if (TeleporterDebugSettings.DrawClosestTeleporterGrid)
        {
            var currentMap = Find.CurrentMap;
            if (currentMap == null)
            {
                return;
            }

            var tracker = currentMap.GetComponent<TeleporterDistanceTracker>();
            tracker?.DebugDrawClosestTeleporterGrid();
        }
    }

    [HarmonyPatch(nameof(MapInterface.MapInterfaceOnGUI_BeforeMainTabs))]
    [HarmonyPostfix]
    public static void Postfix_MapInterfaceOnGUI_BeforeMainTabs()
    {
        if (!WorldRendererUtility.WorldRenderedNow)
        {
            if (TeleporterDebugSettings.DrawTeleporterDistances)
            {
                Find.CurrentMap?.GetComponent<TeleporterDistanceTracker>()?.DebugDrawTeleporterDistances();
            }

            if (TeleporterDebugSettings.DrawBestTravelDirectionForTeleporter)
            {
                Find.CurrentMap?.GetComponent<TeleporterDistanceTracker>()?.DebugDrawBestTravelDirection();
            }
        }
    }
}