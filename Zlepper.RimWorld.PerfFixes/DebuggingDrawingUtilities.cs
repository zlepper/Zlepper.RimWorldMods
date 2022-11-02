using HarmonyLib;
using UnityEngine;
using Zlepper.RimWorld.PerfFixes.FastReachability;

namespace Zlepper.RimWorld.PerfFixes;

[HarmonyPatch(typeof(MapInterface), nameof(MapInterface.MapInterfaceUpdate))]
internal static class DebuggingDrawingUtilities
{
    [HarmonyPostfix]
    internal static void Postfix()
    {
        if (DebuggingUtilities.DrawConnectedAreas)
        {
            DoDraw();
        }
    }
    
    private static readonly Dictionary<int, Material> _areaColors = new();

    private static void DoDraw()
    {
        var map = Find.CurrentMap;
        if (map == null)
        {
            return;
        }

        var reachability = map.GetComponent<FasterReachabilityComponent>().Reachability;
        
        foreach (var cell in map.AllCells)
        {
            var area = reachability.GetConnectedArea(cell.x, cell.z);
            if (area == null)
            {
                continue;
            }

            if (!_areaColors.TryGetValue(area.UniqueId, out var color))
            {
                _areaColors[area.UniqueId] = color = DebugSolidColorMats.MaterialOf(GetColor(area.UniqueId).ToTransparent(0.5f));
            }
            
            CellRenderer.RenderCell(cell, color);
        }
    }

    private static Color GetColor(int number)
    {
        switch (number % 6)
        {
            case 0:
                return Color.blue;
            case 1:
                return Color.cyan;
            case 2:
                return Color.green;
            case 3:
                return Color.magenta;
            case 4:
                return Color.red;
            default:
                return Color.yellow;
        }
    }
}