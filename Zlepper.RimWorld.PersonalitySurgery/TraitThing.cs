
namespace Zlepper.RimWorld.PersonalitySurgery;

[StaticConstructorOnStartup]
public sealed class TraitThing : PersonalityThing
{
    public TraitDef? TraitDef;
    public int TraitDegree;

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Defs.Look(ref TraitDef, nameof(TraitDef));
        Scribe_Values.Look(ref TraitDegree, nameof(TraitDegree));
    }

    public override string LabelNoCount => TraitDef?.DataAtDegree(TraitDegree).label ?? "TraitDef is null";

    [Unsaved]
    private string? _cachedDescription;
    
    public override string DescriptionFlavor
    {
        get
        {
            if (_cachedDescription != null)
            {
                return _cachedDescription;
            }
            
            var samplePawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist);
            
            var trait = new Trait(TraitDef, TraitDegree);
            samplePawn.story.traits.GainTrait(trait, true);
            _cachedDescription = trait.TipString(samplePawn);
            return _cachedDescription;
        }
    }

    public override string DescriptionDetailed => DescriptionFlavor;

    protected override Command_Action GetBaseCancelCommand(Pawn targetPawn)
    {
        return new Command_Action()
        {
            defaultLabel = "Cancel installing trait",
            defaultDesc = $"Cancel installing trait {LabelNoCount} into {targetPawn.LabelCap}"
        };
    }

    protected override Command_Action GetBaseImplantCommand()
    {
        return new Command_Action()
        {
            defaultLabel = "Install trait",
            defaultDesc = $"Install trait {LabelShortCap}",
        };
    }

    protected override string? GetNotImplantableReason(Pawn pawn)
    {
        if (pawn.story.traits.HasTrait(TraitDef, TraitDegree))
        {
            return pawn.LabelCap + " already has trait " + LabelNoCount;
        }

        return null;
    }

    protected override string? GetWarningText(Pawn target)
    {
        var overridesTraits = GetOverwrittenTraits(target);
        var suppressingGenes = target.genes.GenesListForReading
            .Where(g => g.Active && g.def.suppressedTraits != null && g.def.suppressedTraits.Any(x => x.def == TraitDef && x.degree == TraitDegree))
            .ToList();

        var warningText = "";
        if (overridesTraits.Count > 0)
        {
            warningText += $"The following traits to be overridden and removed for {target.LabelCap}:\n\t";
            warningText += string.Join("\n\t", overridesTraits.Select(t => t.def.LabelCap));
        }

        if (suppressingGenes.Count > 0)
        {
            if (warningText.Length > 0)
            {
                warningText += "\n\n";
            }
            
            warningText += "The following genes will suppress the trait:\n\t";
            warningText += string.Join("\n\t", suppressingGenes.Select(g => g.LabelCap));
        }

        if (warningText.Length > 0)
        {
            return warningText;
        }

        return null;
    }

    private List<Trait> GetOverwrittenTraits(Pawn target)
    {
        return target.story.traits.allTraits
            .Where(t => t.sourceGene != null)
            .Where(t => t.def.ConflictsWith(TraitDef) || (t.def == TraitDef && t.Degree != TraitDegree))
            .ToList();
    }

    protected override bool IsValid()
    {
        return TraitDef != null && TraitDef.degreeDatas.Any(d => d.degree == TraitDegree);
    }

    public override void OnSurgerySuccess(Pawn pawn)
    {
        var overridesTraits = GetOverwrittenTraits(pawn);
        foreach (var trait in overridesTraits)
        {
            pawn.story.traits.allTraits.Remove(trait);
        }
        
        var t = new Trait(TraitDef, TraitDegree);
        pawn.story.traits.GainTrait(t);
    }

    public override void PostMake()
    {
        base.PostMake();
        
        var allTraits = DefDatabase<TraitDef>.AllDefsListForReading;
        TraitDef = allTraits.RandomElementByWeight(t => t.GetGenderSpecificCommonality(Gender.None));
        TraitDegree = PawnGenerator.RandomTraitDegree(TraitDef);
    }

    protected override float CalculateBaseMarketValue(float originalValue)
    {
        return ((TraitDef?.DataAtDegree(TraitDegree).marketValueFactorOffset ?? 0) + 1) * originalValue;
    }
}