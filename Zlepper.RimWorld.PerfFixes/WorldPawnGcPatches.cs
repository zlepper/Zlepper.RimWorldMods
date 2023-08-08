using HarmonyLib;
using RimWorld.Planet;

namespace Zlepper.RimWorld.PerfFixes;

[HarmonyPatch(typeof(WorldPawnGC))]
public static class WorldPawnGcPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(WorldPawnGC.AddAllRelationships))]
    static bool AddAllRelationshipsPrefix(Pawn pawn, Dictionary<Pawn, string> keptPawns)
    {
        return pawn.Faction == Faction.OfPlayer || pawn.Faction == Faction.OfEmpire;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(nameof(WorldPawnGC.AddAllMemories))]
    static bool AddAllMemoriesPrefix(Pawn pawn, Dictionary<Pawn, string> keptPawns)
    {
        return pawn.Faction == Faction.OfPlayer || pawn.Faction == Faction.OfEmpire;
    }
}