using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Verse.AI;

namespace Zlepper.RimWorld.RoyaltyImprovements.Teleporting;

[HarmonyPatch(typeof(PathFinder), nameof(PathFinder.FindPath),
    typeof(IntVec3), typeof(LocalTargetInfo), typeof(TraverseParms), typeof(PathEndMode), typeof(PathFinderCostTuning)
)]
public static class PathFinder_FindPath_Patch
{
    private static readonly MethodInfo Verse_GenMath__OctileDistanceMethod;
    private static readonly MethodInfo CellToIndexMethod;

    static PathFinder_FindPath_Patch()
    {
        Verse_GenMath__OctileDistanceMethod =
            AccessTools.Method(typeof(GenMath), nameof(GenMath.OctileDistance),
                new[] {typeof(int), typeof(int), typeof(int), typeof(int)}) ??
            throw new Exception("Could not find OctileDistance method");
        CellToIndexMethod = AccessTools.Method(typeof(CellIndices), nameof(CellIndices.CellToIndex),
            new[] {typeof(int), typeof(int)}) ?? throw new Exception("Could not find CellToIndex method");
    }

    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
        ILGenerator ilGenerator)
    {
        var instructionsList = instructions.ToList();

        ReplaceHeuristicCalculations(instructionsList);
        InjectSkipdoorUsage(instructionsList);


        return instructionsList;
    }

    private static void InjectSkipdoorUsage(List<CodeInstruction> instructionsList)
    {
        var index = instructionsList.FindIndex(i => i.Calls(CellToIndexMethod));

        if (index == -1)
        {
            throw new Exception("Could not find CellToIndex call");
        }
        
        var newInstructions = new List<CodeInstruction>
        {
            new(OpCodes.Ldloca_S, instructionsList[index + 1].operand),
            new(OpCodes.Ldloc_0),
            new(OpCodes.Ldarg_2),
            CodeInstruction.Call(typeof(PathFinderInjectedMethods), nameof(PathFinderInjectedMethods.TryUseSkipdoor),
                new[] {typeof(int).MakeByRefType(), typeof(Pawn), typeof(LocalTargetInfo)})
        };

        instructionsList.InsertRange(index + 2, newInstructions);

    }

    private static void ReplaceHeuristicCalculations(List<CodeInstruction> instructionsList)
    {
        var callIndex = instructionsList.FindIndex(i => i.Calls(Verse_GenMath__OctileDistanceMethod));
        if (callIndex == -1)
            throw new Exception("Could not find OctileDistance call in FindPath method");


        var startIndex = -1;
        for (var i = callIndex; i > 0; i--)
        {
            var code = instructionsList[i];
            if (code.Branches(out _))
            {
                startIndex = i + 1;
                break;
            }
        }

        if (startIndex == -1) throw new Exception("Could not find start index for transpiler");

        var removedInstructions = instructionsList.Skip(startIndex).Take(callIndex - startIndex).ToList();
        instructionsList.RemoveRange(startIndex, callIndex - startIndex + 1);


        var newInstructions = new List<CodeInstruction>()
        {
            new(OpCodes.Ldloc_0),
            removedInstructions[0], // Load num9/fromX
            removedInstructions[4], // Load num10/fromZ
            removedInstructions[1], // Load x1/targetX
            removedInstructions[5], // Load z1/targetZ
            removedInstructions[removedInstructions.Count - 3], // Load num5/cardinalSpeed
            removedInstructions[removedInstructions.Count - 2], // Load num6/diagonalSpeed,
            CodeInstruction.Call(typeof(PathFinderInjectedMethods), nameof(PathFinderInjectedMethods.DistanceWithTeleport),
                new[] {typeof(Pawn), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int), typeof(int)})
        };

        instructionsList.InsertRange(startIndex, newInstructions);
    }

}