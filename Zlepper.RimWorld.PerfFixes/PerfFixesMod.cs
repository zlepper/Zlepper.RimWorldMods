using System.Collections.Generic;
using HarmonyLib;
using HugsLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace Zlepper.RimWorld.PerfFixes;

public class PerfFixesMod : ModBase
{
    public override void WorldLoaded()
    {
        
    }
}

[HarmonyPatch(typeof(WorldPawnGC))]
public static class WorldPawnGcPatches
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(WorldPawnGC.AddAllRelationships))]
    static bool AddAllRelationshipsPrefix(Pawn pawn, Dictionary<Pawn, string> keptPawns)
    {
        return pawn.Faction == Faction.OfPlayer;
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(nameof(WorldPawnGC.AddAllMemories))]
    static bool AddAllMemoriesPrefix(Pawn pawn, Dictionary<Pawn, string> keptPawns)
    {
        return pawn.Faction == Faction.OfPlayer;
    }
}