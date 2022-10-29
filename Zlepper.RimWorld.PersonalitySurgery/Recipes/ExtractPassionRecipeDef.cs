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
        
        var passionGenes = pawn.genes.GenesListForReading.Where(r => r.Active && r.def.passionMod?.skill == Skill).ToList();

        if (passionGenes.Any(g => g.def.passionMod.NewPassionForOnRemoval(matches) == Passion))
        {
            return true;
        }


        if (passionGenes.Count == 0)
        {
            return matches.passion == Passion;
        }

        return false;
    }

    public override void ApplyChanges(Pawn pawn, Pawn billDoer)
    {
        var skill = pawn.skills.GetSkill(Skill);

        if (skill != null && skill.passion - Passion > (byte)Passion.None)
        {
            GenSpawn.Spawn(PassionThing, billDoer.Position, billDoer.Map);
            skill.passion -= Passion;
    
            HealthUtility.GiveRandomSurgeryInjuries(pawn, 15, pawn.health.hediffSet.GetBrain());
        }
    }
}