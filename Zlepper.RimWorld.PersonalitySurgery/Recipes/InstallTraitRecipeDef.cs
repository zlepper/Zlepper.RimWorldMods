namespace Zlepper.RimWorld.PersonalitySurgery.Recipes;

public class InstallTraitRecipeDef : BaseTraitRecipeDef
{
    
    public override bool CheckIfSurgeryIsPossible(Pawn pawn)
    {
        var existing = pawn.story.traits.allTraits.FirstOrDefault(t => t.def == Trait);
        return existing?.Degree != TraitDegree;
    }
    
    public override void ApplyChanges(Pawn pawn, Pawn billDoer)
    {
        var traitsToRemove = new List<Trait>();


        var sameTrait = pawn.story.traits.allTraits.FirstOrDefault(t => t.def == Trait);
        if (sameTrait != null)
        {
            traitsToRemove.Add(sameTrait);
        }

        var incompatibleTraits = pawn.story.traits.allTraits.Where(t =>
            Trait.ConflictsWith(t)|| t.def.ConflictsWith(Trait));
        traitsToRemove.AddRange(incompatibleTraits);

        foreach (var trait in traitsToRemove)
        {
            pawn.story.traits.RemoveTrait(trait);
        }

        pawn.story.traits.GainTrait(new Trait(Trait, TraitDegree));
    }

}