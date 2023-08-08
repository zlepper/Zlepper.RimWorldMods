using System.Reflection;
using HarmonyLib;
using UnityEngine;
using VanillaPsycastsExpanded.Skipmaster;
using VFECore;

namespace Zlepper.RimWorld.RoyaltyImprovements.Teleporting;

[HarmonyPatch(typeof(DoorTeleporter), nameof(DoorTeleporter.DrawAt), typeof(Vector3), typeof(bool))]
public static class Skipdoor_Patches
{
    private static readonly FieldInfo DistortionAmountField = AccessTools.Field(typeof(DoorTeleporter), "distortAmount") ??
                                                              throw new Exception(
                                                                  "Failed to get field 'distortAmount' from Skipdoor");

    private static readonly MethodInfo DrawMeshMethod = AccessTools.Method(typeof(Graphics), nameof(Graphics.DrawMesh),
        new[] {typeof(Mesh), typeof(Matrix4x4), typeof(Material), typeof(int)}) ?? throw new Exception("Failed to get method 'DrawMesh' from Graphics");

    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> Transpile(IEnumerable<CodeInstruction> instructions)
    {
        var list = instructions.ToList();

        var index = list.FindIndex(c => c.LoadsField(DistortionAmountField));

        if (index == -1)
            throw new Exception("Failed to find instruction 'ldfld Skipdoor.distortAmount'");

        var startIndex = -1;
        var endIndex = -1;
        
        for(var i = index; i >= 0; i--)
        {
            var op = list[i];
            if (op.Calls(DrawMeshMethod))
            {
                startIndex = i + 1;
                break;
            }
        }
        for(var i = index; i < list.Count; i++)
        {
            var op = list[i];
            if (op.Calls(DrawMeshMethod))
            {
                endIndex = i;
                break;
            }
        }

        if (startIndex == -1 || endIndex == -1)
        {
            throw new Exception("Failed to find positions for DrawMesh calls");
        }

        var toRemove = list.Skip(startIndex).Take(endIndex - startIndex).ToList();
        list.RemoveRange(startIndex, endIndex - startIndex + 1);

        var last = list.Last();
        
        foreach (var instruction in toRemove)
        {
            instruction.MoveLabelsTo(last);
        }

        return list;
    }
}