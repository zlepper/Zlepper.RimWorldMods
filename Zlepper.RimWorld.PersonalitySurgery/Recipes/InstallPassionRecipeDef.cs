using Verse;

namespace Zlepper.RimWorld.PersonalitySurgery.Recipes;

public class InstallPassionRecipeDef : BasePassionRecipeDef
{
    public override bool CheckIfSurgeryIsPossible(Pawn pawn)
    {
        var skill = pawn.skills.GetSkill(Skill);
        
        
        return !skill.TotallyDisabled && skill.passion != Passion;
    }

    public override void ApplyChanges(Pawn pawn, Pawn billDoer)
    {
        pawn.skills.GetSkill(Skill).passion = Passion;
    }
}