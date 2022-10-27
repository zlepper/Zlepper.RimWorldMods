using HarmonyLib;

namespace Zlepper.RimWorld.Warnings;

[HarmonyPatch(typeof(Messages), nameof(Messages.Message), typeof(Message), typeof(bool))]
public static class Messages_Message_Patch
{

    [HarmonyPrefix]
    public static bool LogMessageInfo(Message msg)
    {
        LetterDef? letterDef = null;

        if (msg.def.defName == MessageTypeDefOf.ThreatBig.defName)
        {
            letterDef = LetterDefOf.ThreatBig;
        } else if (msg.def.defName == MessageTypeDefOf.ThreatSmall.defName)
        {
            letterDef = LetterDefOf.ThreatSmall;
        }

        if (letterDef == null) return true;
        
        ZleppersWarningsMod.Logger.Trace($"Converting message to letter: '{msg.text}'. DefName: '{msg.def.defName}'. Label: '{msg.def.label}'.");

        var label = string.IsNullOrWhiteSpace(msg.def.label) ? msg.text : msg.def.label;
            
        var letter = LetterMaker.MakeLetter(label, msg.text, letterDef, msg.lookTargets, null, msg.quest);
        Find.LetterStack.ReceiveLetter(letter);
        return false;

    }
    
}