using HarmonyLib;
using Verse.AI.Group;

namespace Zlepper.RimWorld.Warnings;

[HarmonyPatch(typeof(LordJob_StageThenAttack), nameof(LordJob_StageThenAttack.CreateGraph))]
public static class LordJob_StageThenAttack_CreateGraph_Patch
{
    [HarmonyPostfix]
    public static void Postfix(ref StateGraph __result)
    {
        var attackTransition = __result.transitions.FirstOrDefault(IsAttackTransition);
        if (attackTransition == null)
        {
            ZleppersWarningsMod.Logger.Warning("Could not find attack transition.");
            return;
        }
        
        var message = attackTransition.preActions.OfType<TransitionAction_Message>().First();
        
        attackTransition.AddPreAction(new TransitionAction_Custom(() =>
        {
            var label = string.IsNullOrWhiteSpace(message.type.label) ? message.message : message.type.label;

            var lookTargets = message.lookTargetGetter?.Invoke() ?? message.lookTarget;
            var letter = LetterMaker.MakeLetter(label, message.message, LetterDefOf.ThreatBig, lookTargets);
            Find.LetterStack.ReceiveLetter(letter);
        }));
    }

    private static bool IsAttackTransition(Transition transition)
    {
        return transition.preActions.Any(a => a is TransitionAction_Message) && transition.triggers.Any(trigger => trigger is Trigger_TicksPassed) && transition.triggers.Any(trigger => trigger is Trigger_FractionPawnsLost);
    }
}