namespace Zlepper.RimWorld.PersonalitySurgery.Recipes;

public class InstallTraitRecipeDef : BaseTraitRecipeDef
{
    
    public override bool CheckIfSurgeryIsPossible(Pawn pawn)
    {
        var existing = pawn.story.traits.allTraits.FirstOrDefault(t => t.def == Trait);
        return existing?.Degree != TraitDegree;
    }
    
    public override void ApplyChanges(Pawn pawn, BodyPartRecord bodyPartRecord, Pawn billDoer)
    {
        var traitsToRemove = GetTraitsToRemove(pawn);

        foreach (var trait in traitsToRemove)
        {
            pawn.story.traits.RemoveTrait(trait);
        }

        pawn.story.traits.GainTrait(new Trait(Trait, TraitDegree));
    }

    private List<Trait> GetTraitsToRemove(Pawn pawn)
    {
        var traitsToRemove = new List<Trait>();


        var sameTrait = pawn.story.traits.allTraits.FirstOrDefault(t => t.def == Trait);
        if (sameTrait != null)
        {
            traitsToRemove.Add(sameTrait);
        }

        var incompatibleTraits = pawn.story.traits.allTraits.Where(t =>
            Trait.ConflictsWith(t) || t.def.ConflictsWith(Trait));
        traitsToRemove.AddRange(incompatibleTraits);
        return traitsToRemove;
    }

    public override TaggedString GetConfirmation(Pawn pawn)
    {
        var traitsToRemove = GetTraitsToRemove(pawn);

        var suppressingGenes = pawn.genes.GenesListForReading
            .Where(g => g.Active && g.def.suppressedTraits != null && g.def.suppressedTraits.Any(x => x.def == Trait && x.degree == TraitDegree))
            .ToList();

        TaggedString warningText = "";
        if (traitsToRemove.Count > 0)
        {
            warningText += "InstallTraitWarning_TraitsWillBeOverridenTitle".Translate(pawn.LabelCap);
            warningText += "\n\t";
            warningText += traitsToRemove.Select(t => t.LabelCap).ToLineList("  - ");
        }

        if (suppressingGenes.Count > 0)
        {
            if (warningText.Length > 0)
            {
                warningText += "\n\n";
            }

            warningText += "InstallTraitWarning_GenesWillSuppressTraitTitle".Translate();
            warningText += "\n\t";
            warningText += suppressingGenes.Select(g => g.LabelCap).ToLineList("  - ");
        }

        if (warningText.Length > 0)
        {
            return warningText;
        }

        return null;
    }
}