using System.Reflection.Emit;
using HarmonyLib;
using UnityEngine;
using Verse.Sound;

namespace Zlepper.RimWorld.CraftingEnhancements;

[HarmonyPatch(typeof(Dialog_BillConfig), nameof(Dialog_BillConfig.DoWindowContents))]
public static class Dialog_BillConfig_DoWindowContents_Patches
{
    [HarmonyPatch(nameof(Dialog_BillConfig.DoWindowContents), typeof(Rect))]
    [HarmonyTranspiler]
    public static IEnumerable<CodeInstruction> AddCopyButtons(IEnumerable<CodeInstruction> instructions)
    {
        var insts = instructions.ToList();
        var billFields = AccessTools.Field(typeof(Dialog_BillConfig), "bill");
        var repeatModeField = AccessTools.Field(typeof(Bill_Production), nameof(Bill_Production.repeatMode));
        var targetCountField = AccessTools.Field(typeof(BillRepeatModeDefOf), nameof(BillRepeatModeDefOf.TargetCount));
        var labelMethod = AccessTools.Method(typeof(Listing_Standard), nameof(Listing_Standard.Label),
            new[] {typeof(string), typeof(float), typeof(string)});
        var targetCountEditBufferField = AccessTools.Field(typeof(Dialog_BillConfig), "targetCountEditBuffer");
        var unpauseCountEditBufferField = AccessTools.Field(typeof(Dialog_BillConfig), "unpauseCountEditBuffer");

        for (var i = 0; i < insts.Count; i++)
        {
            var instruction = insts[i];

            if (instruction.IsLdarg(0)) // this
            {
                if (insts.Count > i + 4)
                {
                    if (insts[i + 1].Is(OpCodes.Ldfld, billFields)
                        && insts[i + 2].Is(OpCodes.Ldfld, repeatModeField)
                        && insts[i + 3].Is(OpCodes.Ldsfld, targetCountField)
                        && insts[i + 4].Branches(out _))
                    {
                        i += 5;
                        while (i < insts.Count)
                        {
                            if (insts[i].Calls(labelMethod))
                            {
                                var listingInstruction = insts[i - 4];
                                if (!listingInstruction.IsLdloc())
                                {
                                    throw new Exception("Code changed, expected a local variable");
                                }
                                
                                
                                
                                insts.InsertRange(i - 4, new []
                                {
                                    // Load "bill" field
                                    new CodeInstruction(OpCodes.Ldarg_0)
                                    {
                                        labels = listingInstruction.labels,
                                    }.MoveLabelsFrom(listingInstruction),
                                    CodeInstruction.LoadField(typeof(Dialog_BillConfig), "bill"), 
                                    // Load "targetCountEditBuffer" field
                                    new CodeInstruction(OpCodes.Ldarg_0),
                                    CodeInstruction.LoadField(typeof(Dialog_BillConfig), "targetCountEditBuffer", true), 
                                    // Load "unpauseCountEditBuffer" field
                                    new CodeInstruction(OpCodes.Ldarg_0),
                                    CodeInstruction.LoadField(typeof(Dialog_BillConfig), "unpauseCountEditBuffer", true),
                                    // Load "listing" local variable
                                    new CodeInstruction(listingInstruction.opcode, listingInstruction.operand),
                                    // Call injected method
                                    CodeInstruction.Call(typeof(Dialog_BillConfig_DoWindowContents_Patches),
                                        nameof(AddCopyButtons_New), new[]
                                        {
                                            typeof(Bill_Production),
                                            typeof(string).MakeByRefType(),
                                            typeof(string).MakeByRefType(),
                                            typeof(Listing_Standard)
                                        }), 
                                });
                                break;
                            }

                            i++;
                        }

                        break;
                    }
                }
            }
        }

        return insts;
    }

    private const int ButtonSize = 24;
    private const int ButtonGap = 4;

    public static void AddCopyButtons_New(Bill_Production? bill, ref string ___targetCountEditBuffer,
        ref string ___unpauseCountEditBuffer, Listing_Standard listing)
    {
        if (bill == null)
        {
            return;
        }
        
        if (bill.repeatMode == BillRepeatModeDefOf.TargetCount)
        {
            var rect = listing.GetRect(0);

            rect.x += rect.width - ButtonSize;
            rect.height = ButtonSize;
            rect.width = ButtonSize;
            
            if (Widgets.ButtonImageFitted(rect, TexButton.Copy, Color.white))
            {
                Clipboard.RepeatSettings = new BillProductionRepeatSettings(bill);
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
            }
        
            TooltipHandler.TipRegionByKey(rect, "ZRCE_CopyRepeatParameters");
        
            if (Clipboard.RepeatSettings != null)
            {
                rect.x += -rect.width - ButtonGap;
        
                if (Widgets.ButtonImageFitted(rect, TexButton.Paste, Color.white))
                {
                    Clipboard.RepeatSettings.ApplyToBill(bill);
                    ___targetCountEditBuffer = Clipboard.RepeatSettings.targetCount.ToString();
                    ___unpauseCountEditBuffer = Clipboard.RepeatSettings.unpauseWhenYouHave.ToString();
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                }
        
                TooltipHandler.TipRegionByKey(rect, "ZRCE_PasteRepeatParameters");
            }
        }
    }
}