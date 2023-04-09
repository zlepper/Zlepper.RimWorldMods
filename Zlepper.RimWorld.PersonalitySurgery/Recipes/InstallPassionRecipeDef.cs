namespace Zlepper.RimWorld.PersonalitySurgery.Recipes;

public class InstallPassionRecipeDef : BasePassionRecipeDef
{
    public override bool CheckIfSurgeryIsPossible(Pawn pawn)
    {
        var skill = pawn.skills.GetSkill(Skill);
        
        
        return !skill.TotallyDisabled && skill.GetOriginalPassion() < Passion;
    }

    public override void ApplyChanges(Pawn pawn, BodyPartRecord bodyPartRecord, Pawn billDoer)
    {
        var skillRecord = pawn.skills.GetSkill(Skill);
        skillRecord.passion = Passion;
        foreach (var gene in pawn.genes.GenesListForReading)
        {
            if (gene.Active)
            {
                if (gene.def.passionMod != null && gene.def.passionMod.skill == skillRecord.def)
                {
                    skillRecord.passion = gene.def.passionMod.NewPassionFor(skillRecord);
                }
            }
        }
    }

    public override TaggedString GetConfirmation(Pawn pawn)
    {
        var skillRecord = pawn.skills.GetSkill(Skill);
        var originalPassion = skillRecord.GetOriginalPassion();
        if (originalPassion > Passion)
        {
            return "InstallPassionWarning_WillRemoveHigherPassion".Translate();
        }
        
        foreach (var gene in pawn.genes.GenesListForReading)
        {
            if(!gene.Active) 
                continue;

            if (gene.def.passionMod != null)
            {
                if (gene.def.passionMod.modType == PassionMod.PassionModType.DropAll)
                {
                    return $"InstallPassionWarning_GeneWillOverridePassion".Translate(gene.LabelCap);
                }
            }
        }

        return null;
    }
}