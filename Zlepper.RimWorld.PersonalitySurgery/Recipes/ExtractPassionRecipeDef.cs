namespace Zlepper.RimWorld.PersonalitySurgery.Recipes;

public class ExtractPassionRecipeDef : BasePassionRecipeDef
{
    public override bool CheckIfSurgeryIsPossible(Pawn pawn)
    {
        var matches = pawn.skills.GetSkill(Skill);

        if(matches == null)
        {
            return false;
        }

        return matches.GetOriginalPassion() == Passion;
    }

    public override void ApplyChanges(Pawn pawn, BodyPartRecord bodyPartRecord, Pawn billDoer)
    {
        var skill = pawn.skills.GetSkill(Skill);

        if (skill != null && skill.passion > (byte)Passion.None)
        {
            GenSpawn.Spawn(PassionThing, billDoer.Position, billDoer.Map);
            skill.passion -= Passion;
            
            DamagePart(bodyPartRecord, pawn, billDoer);
        }
    }
}