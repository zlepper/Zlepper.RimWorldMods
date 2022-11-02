using System.Diagnostics;
using Zlepper.RimWorld.PerfFixes.FastReachability;
using Random = UnityEngine.Random;

namespace Zlepper.RimWorld.PerfFixes;

internal static class DebuggingUtilities
{
    public static bool DrawConnectedAreas = false;

    [DebugOutput("PerfFixes", true)]
    public static void ToggleDrawConnectedAreas()
    {
        DrawConnectedAreas = !DrawConnectedAreas;
    }

    [DebugOutput("PerfFixes", true)]
    public static void RecalculateReachabilityForCurrentMap()
    {
        var map = Find.CurrentMap;
        if (map == null)
        {
            return;
        }

        var c = map.GetComponent<FasterReachabilityComponent>().Reachability;
        c.ReBuildConnectedAreas();
    }

    [DebugOutput("PerfFixes", true)]
    public static void PickPosition()
    {
        Find.TilePicker.StartTargeting((_) => true, chosenIndex =>
        {
            var map = Find.CurrentMap;
            if (map == null)
            {
                return;
            }

            var pos = CellIndicesUtility.IndexToCell(chosenIndex, map.Size.x);
            
            Log.Message($"Picked: pos: {pos} at index: { chosenIndex }");
            
        });
    }
}