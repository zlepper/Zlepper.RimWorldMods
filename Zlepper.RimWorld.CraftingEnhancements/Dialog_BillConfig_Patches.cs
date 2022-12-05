using HarmonyLib;
using UnityEngine;
using Verse.Sound;

namespace Zlepper.RimWorld.CraftingEnhancements;

[HarmonyPatch(typeof(Dialog_BillConfig), nameof(Dialog_BillConfig.DoWindowContents))]
public static class Dialog_BillConfig_Patches
{
    
    [TweakValue("Interface", 0.0f, 800f)]
    private static int CopyRepeatSettingsXOffset = 475;
    [TweakValue("Interface", 0.0f, 800f)]
    private static int CopyRepeatSettingsYOffset = 95;
    
    [HarmonyPatch(nameof(Dialog_BillConfig.DoWindowContents), typeof(Rect))]
    [HarmonyPostfix]
    public static void AddCopyButtons(Bill_Production? ___bill, Rect inRect, ref string ___targetCountEditBuffer, ref string ___unpauseCountEditBuffer)
    {
        if (___bill == null)
        {
            return;
        }

        if (___bill.repeatMode == BillRepeatModeDefOf.TargetCount)
        {
            var rect = new Rect(inRect.x + CopyRepeatSettingsXOffset, inRect.y + CopyRepeatSettingsYOffset, 24, 24);
            if (Widgets.ButtonImageFitted(rect, TexButton.Copy, Color.white))
            {
                Clipboard.RepeatSettings = new BillProductionRepeatSettings(___bill);
                SoundDefOf.Tick_High.PlayOneShotOnCamera();
            }
            TooltipHandler.TipRegionByKey(rect, "CopyBillTip");

            if (Clipboard.RepeatSettings != null)
            {
                rect.x += -rect.width - 4;
                
                if (Widgets.ButtonImageFitted(rect, TexButton.Paste, Color.white))
                {
                    Clipboard.RepeatSettings.ApplyToBill(___bill);
                    ___targetCountEditBuffer = Clipboard.RepeatSettings.targetCount.ToString();
                    ___unpauseCountEditBuffer = Clipboard.RepeatSettings.unpauseWhenYouHave.ToString();
                    SoundDefOf.Tick_High.PlayOneShotOnCamera();
                }
                TooltipHandler.TipRegionByKey(rect, "PasteBillTip");
            }
        }
        
    }
}