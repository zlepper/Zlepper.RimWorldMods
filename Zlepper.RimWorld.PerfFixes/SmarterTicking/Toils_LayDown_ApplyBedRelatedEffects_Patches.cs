using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;

namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

[HarmonyPatch(typeof(Toils_LayDown), "ApplyBedRelatedEffects", typeof(Pawn), typeof(Building_Bed), typeof(bool), typeof(bool), typeof(bool))]
public static class Toils_LayDown_ApplyBedRelatedEffects_Patches
{
    [HarmonyTranspiler]
    static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var getStatValueMethod = typeof(StatExtension).GetMethod(nameof(StatExtension.GetStatValue),
            BindingFlags.Public | BindingFlags.Static) ?? throw new MissingMethodException("Failed to find GetStatValue method for call when patching");
        var isColonyMechGetterMethod = typeof(Pawn).GetProperty(nameof(Pawn.IsColonyMech), BindingFlags.Instance|BindingFlags.Public)?.GetMethod
            ?? throw new MissingMethodException("Failed to find IsColonyMech getter method for call when patching");        
        
        var codes = instructions.ToList();

        var zFleckStartAtIndex = -1;
        var zFleckEndAtIndex = -1;
        var zFleckEndLabel = new Label();
        
        for (var i = 1; i < codes.Count; i++)
        {
            var code = codes[i];
            if (code.Calls(getStatValueMethod))
            {
                var previous = codes[i - 1];
                previous.opcode = OpCodes.Ldc_I4_S;
                previous.operand = 10;
            }

            if (code.IsLdarg(2) && i < codes.Count - 5)
            {
                if (codes[i + 1].Branches(out var endLabel) && endLabel.HasValue)
                {
                    if (codes[i + 2].IsLdarg(0))
                    {
                        if (codes[i + 3].Calls(isColonyMechGetterMethod))
                        {
                            if (codes[i + 4].Branches(out var secondEndLabel))
                            {
                                zFleckStartAtIndex = i;
                                zFleckEndLabel = endLabel.Value;
                            }
                        }
                    }
                }
            }

            if (code.labels.Contains(zFleckEndLabel))
            {
                zFleckEndAtIndex = i;
            }
        }
        
        if (zFleckStartAtIndex == -1 || zFleckEndAtIndex == -1)
        {
            throw new Exception("Failed to find ZFleck code to patch");
        }
        
        codes.RemoveRange(zFleckStartAtIndex, zFleckEndAtIndex - zFleckStartAtIndex);
        
        
        return codes.AsEnumerable();
    }
}