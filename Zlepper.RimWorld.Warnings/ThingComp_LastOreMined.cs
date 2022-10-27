namespace Zlepper.RimWorld.Warnings;

public class ThingComp_LastOreMined : ThingComp
{
    public override void PostDestroy(DestroyMode mode, Map previousMap)
    {
        // Never send alerts on home map
        if (previousMap.IsPlayerHome)
        {
            return;
        }

        if (HasOre(previousMap)) return;


        var quest = Find.QuestManager.questsInDisplayOrder.Find(q => q.QuestLookTargets.Contains(previousMap.Parent));

        var letter = LetterMaker.MakeLetter("Letter_LastOreMined_Label".Translate(), "Letter_LastOreMined_Text".Translate(),
            LetterDefOf.NeutralEvent, previousMap.Parent, null, quest);
        Find.LetterStack.ReceiveLetter(letter);
    }

    private static bool HasOre(Map map)
    {
        foreach (var thing in map.listerThings.AllThings)
        {
            if (thing.def.building?.isResourceRock is true)
            {
                return true;
            }
        }

        return false;
    }
}