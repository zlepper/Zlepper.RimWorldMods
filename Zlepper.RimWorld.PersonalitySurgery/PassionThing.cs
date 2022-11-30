namespace Zlepper.RimWorld.PersonalitySurgery;

[StaticConstructorOnStartup]
public sealed class PassionThing : PersonalityThing
{
    public Passion passion;
    public SkillDef? SkillDef;
    
    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref SkillDef, nameof(SkillDef));
        Scribe_Values.Look(ref passion, nameof(passion));
    }
    
    public override string LabelNoCount => $"{passion} passion for {SkillDef?.label}";

    protected override bool IsValid()
    {
        return SkillDef != null && Enum.IsDefined(typeof(Passion), passion);
    }

    protected override Command_Action GetBaseCancelCommand(Pawn targetPawn)
    {
        return new Command_Action()
        {
            defaultLabel = "Cancel installing passion",
            defaultDesc = $"Cancel installing {LabelNoCount} on {targetPawn.LabelCap}",
        };
    }

    protected override Command_Action GetBaseImplantCommand()
    {
        return new Command_Action()
        {
            defaultLabel = "Install passion",
            defaultDesc = $"Install {LabelNoCount}",
        };
    }

    protected override string? GetNotImplantableReason(Pawn pawn)
    {
        var skillRecord = pawn.skills.GetSkill(SkillDef);
        if (skillRecord.PermanentlyDisabled)
        {
            return $"Skill {skillRecord.def.LabelCap} is disabled";
        }

        var originalPassion = skillRecord.GetOriginalPassion();

        if (originalPassion == passion)
        {
            return "Already has this passion";
        }
        
        return null;
    }

    protected override string? GetWarningText(Pawn target)
    {
        var skillRecord = target.skills.GetSkill(SkillDef);
        var originalPassion = skillRecord.GetOriginalPassion();
        if (originalPassion > passion)
        {
            return "This will remove the higher passion";
        }
        
        foreach (var gene in target.genes.GenesListForReading)
        {
            if(!gene.Active) 
                continue;

            if (gene.def.passionMod != null)
            {
                if (gene.def.passionMod.modType == PassionMod.PassionModType.DropAll)
                {
                    return $"Gene {gene.LabelCap} will override this passion";
                }
            }
        }

        return null;
    }

    public override void OnSurgerySuccess(Pawn pawn)
    {
        var skillRecord = pawn.skills.GetSkill(SkillDef);
        skillRecord.passion = passion;
        
        foreach (var gene in pawn.genes.GenesListForReading)
        {
            if (!gene.Active)
            {
                continue;
            }

            if (gene.def.passionMod != null)
            {
                skillRecord.passion = gene.def.passionMod.NewPassionFor(skillRecord);
            }
        }
    }

    public override void PostMake()
    {
        base.PostMake();
        
        SkillDef = DefDatabase<SkillDef>.AllDefsListForReading.RandomElement();

        passion = Rand.Element(Passion.Minor, Passion.Minor, Passion.Minor, Passion.Major, Passion.Major);
    }

    protected override float CalculateBaseMarketValue(float originalValue)
    {
        return (byte) passion * originalValue;
    }
}