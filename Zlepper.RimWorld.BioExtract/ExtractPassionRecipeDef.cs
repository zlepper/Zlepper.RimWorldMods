using RimWorld;
using Verse;

namespace Zlepper.Rimworld.BioExtract;

public class ExtractPassionRecipeDef : BasePassionRecipeDef
{
    public override bool CheckIfSurgeryIsPossible(Pawn pawn)
    {
        var matches = pawn.skills.GetSkill(Skill);
        return matches?.passion == Passion;
    }

    public override void ApplyChanges(Pawn pawn, Pawn billDoer)
    {
        var skill = pawn.skills.GetSkill(Skill);

        if (skill != null && skill.passion == Passion)
        {
            GenSpawn.Spawn(PassionThing, billDoer.Position, billDoer.Map);
            skill.passion = Passion.None;
    
            HealthUtility.GiveInjuriesOperationFailureMinor(pawn, pawn.health.hediffSet.GetBrain());
        }
    }
}