using System.Reflection.Emit;
using HarmonyLib;

namespace Zlepper.RimWorld.CraftingEnhancements;

[HarmonyPatch(typeof(BillRepeatModeUtility), nameof(BillRepeatModeUtility.MakeConfigFloatMenu),
    typeof(Bill_Production))]
public static class BillRepeatModeUtility_MakeConfigFloatMenu_Patches
{
    public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions,
        ILGenerator ilGenerator)
    {
        var windowStackProperty = typeof(Find).GetProperty(nameof(Find.WindowStack));
        if (windowStackProperty == null)
        {
            Log.Error($"Could not find property WindowStack on {typeof(Find)}");
            foreach (var instruction in instructions)
            {
                yield return instruction;
            }

            yield break;
        }

        var listCtor = typeof(List<FloatMenuOption>).GetConstructor(Type.EmptyTypes);

        var getter = windowStackProperty.GetMethod;

        var nextShouldSetLocal = false;
        var listLocalIndex = 1;

        foreach (var instruction in instructions)
        {
            if (nextShouldSetLocal)
            {
                if (instruction.opcode == OpCodes.Stloc_0) listLocalIndex = 0;
                if (instruction.opcode == OpCodes.Stloc_1) listLocalIndex = 1;
                if (instruction.opcode == OpCodes.Stloc_2) listLocalIndex = 2;
                if (instruction.opcode == OpCodes.Stloc_3) listLocalIndex = 3;
                if (instruction.opcode == OpCodes.Stloc) listLocalIndex = (int) instruction.operand;

                nextShouldSetLocal = false;
            }

            if (instruction.Is(OpCodes.Newobj, listCtor))
            {
                nextShouldSetLocal = true;
            }

            if (instruction.Calls(getter))
            {
                // Load the options list
                yield return new CodeInstruction(OpCodes.Ldloc, listLocalIndex);
                // Load the bill
                yield return new CodeInstruction(OpCodes.Ldarg_0);
                yield return CodeInstruction.Call(typeof(BillRepeatModeUtility_MakeConfigFloatMenu_Patches),
                    nameof(AddPasteOption), new[] {typeof(List<FloatMenuOption>), typeof(Bill_Production)});

                yield return instruction;
            }
            else
            {
                yield return instruction;
            }
        }
    }

    public static void AddPasteOption(List<FloatMenuOption> options, Bill_Production bill)
    {
        if (Clipboard.RepeatSettings != null && bill.recipe.WorkerCounter.CanCountProducts(bill))
        {
            options.Add(new FloatMenuOption("Paste from clipboard", () =>
            {
                bill.repeatMode = BillRepeatModeDefOf.TargetCount;
                Clipboard.RepeatSettings.ApplyToBill(bill);
            }));
        }
    }
}