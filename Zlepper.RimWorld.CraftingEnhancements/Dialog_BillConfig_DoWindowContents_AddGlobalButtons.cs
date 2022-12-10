using HarmonyLib;
using UnityEngine;
using Verse.Sound;

namespace Zlepper.RimWorld.CraftingEnhancements;

[HarmonyPatch(typeof(Dialog_BillConfig), nameof(Dialog_BillConfig.DoWindowContents), typeof(Rect))]
public class Dialog_BillConfig_DoWindowContents_AddGlobalButtons
{
    [HarmonyPostfix]
    public static void Postfix(Bill_Production ___bill)
    {
        var buttonRect = new Rect(20, 51, 24, 24);
        if (___bill.billStack.Count > 1)
        {
            if (___bill.repeatMode == BillRepeatModeDefOf.TargetCount)
            {
                if (Widgets.ButtonImageFitted(buttonRect, TexButton.Paste, Color.white))
                {
                    PasteRepeatParameters(___bill);
                }

                TooltipHandler.TipRegionByKey(buttonRect, "ZRCE_ApplyRepeatParametersToAllBillsInTable");

                buttonRect.x += 24;
            }

            if (Widgets.ButtonImageFitted(buttonRect, TexButton.Paste, Color.white))
            {
                PasteFilterParameters(___bill);
            }

            TooltipHandler.TipRegionByKey(buttonRect, "ZRCE_ApplyFitlerParametersToAllBillsInTable");
        }
    }

    private static void PasteRepeatParameters(Bill_Production bill)
    {
        var settings = new BillProductionRepeatSettings(bill);
        var pasteCount = 0;
        foreach (var otherBill in bill.billStack.Bills.OfType<Bill_Production>())
        {
            if (otherBill.recipe.WorkerCounter.CanCountProducts(otherBill))
            {
                otherBill.repeatMode = BillRepeatModeDefOf.TargetCount;
                settings.ApplyToBill(otherBill);
                pasteCount++;
            }
        }
        
        if (pasteCount == 0)
        {
            SoundDefOf.ClickReject.PlayOneShotOnCamera();
            Messages.Message("ZRCE_NoRecipesAvailableForPasting".Translate(), LookTargets.Invalid,
                MessageTypeDefOf.RejectInput, false);
        }
        else
        {
            SoundDefOf.Tick_High.PlayOneShotOnCamera();
            Messages.Message("ZRCE_ApplyRepeatToRecipesMessage".Translate(pasteCount), LookTargets.Invalid,
                MessageTypeDefOf.NeutralEvent, false);
        }
    }

    private static void PasteFilterParameters(Bill_Production bill)
    {
        var filter = bill.ingredientFilter;

        var pasteCount = 0;
        foreach (var otherBill in bill.billStack.Bills.OfType<Bill_Production>())
        {
            if (otherBill == bill)
            {
                continue;
            }
            
            otherBill.ingredientFilter.CopyAllowancesFrom(filter);
            pasteCount++;
        }

        if (pasteCount == 0)
        {
            SoundDefOf.ClickReject.PlayOneShotOnCamera();
            Messages.Message("ZRCE_NoRecipesAvailableForPasting".Translate(), LookTargets.Invalid,
                MessageTypeDefOf.RejectInput, false);
        }
        else
        {
            SoundDefOf.Tick_High.PlayOneShotOnCamera();
            Messages.Message("ZRCE_ApplyMaterialFilterToRecipesMessage".Translate(pasteCount), LookTargets.Invalid,
                MessageTypeDefOf.NeutralEvent, false);
        }
    }
}