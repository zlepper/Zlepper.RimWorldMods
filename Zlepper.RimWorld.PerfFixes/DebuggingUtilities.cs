using System.Diagnostics;
using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Zlepper.RimWorld.PerfFixes;

internal static class DebuggingUtilities
{
    public static bool DrawConnectedAreas = false;

    [DebugOutput("PerfFixes", true)]
    public static void ToggleDrawConnectedAreas()
    {
        DrawConnectedAreas = !DrawConnectedAreas;

        if (!DrawConnectedAreas)
        {
            DebuggingDrawingUtilities.ClearCache();
        }
        
    }
}

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
    
    private static readonly Dictionary<int, FasterReachability> _calculatedReachability = new();
    private static readonly Dictionary<int, Material> _areaColors = new();

    internal static void ClearCache()
    {
        _calculatedReachability.Clear();
        _areaColors.Clear();
    }

    private static void DoDraw()
    {
        var map = Find.CurrentMap;
        if (map == null)
        {
            return;
        }
        

        if (!_calculatedReachability.TryGetValue(map.uniqueID, out var reachability))
        {
            reachability = new FasterReachability(map);
            var sw = Stopwatch.StartNew();
            reachability.BuildConnectedAreas();
            sw.Stop();
            Log.Message($"Building connected areas took {sw.Elapsed}");
            _calculatedReachability[map.uniqueID] = reachability;
        }

        foreach (var cell in map.AllCells)
        {
            var area = reachability.GetConnectedArea(cell.x, cell.z);
            if (area == null)
            {
                continue;
            }

            if (!_areaColors.TryGetValue(area.UniqueId, out var color))
            {
                _areaColors[area.UniqueId] = color = DebugSolidColorMats.MaterialOf(new Color(Random.value, Random.value, Random.value, 0.2f));
            }
            
            CellRenderer.RenderCell(cell, color);
        }
        
        Log.WarningOnce($"Draw {_areaColors.Keys.Count} unique connected areas", 45896325);
        
        
    }
    
}