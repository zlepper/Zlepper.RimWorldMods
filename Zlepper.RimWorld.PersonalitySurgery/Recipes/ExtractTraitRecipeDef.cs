namespace Zlepper.RimWorld.PersonalitySurgery.Recipes;

public class ExtractTraitRecipeDef : BaseTraitRecipeDef
{
    
    public override bool CheckIfSurgeryIsPossible(Pawn pawn)
    {
        var matches = pawn.story.traits.allTraits.FirstOrDefault(t =>
            t.def == Trait && t.Degree == TraitDegree && t.sourceGene == null && !t.Suppressed);
        return matches != null;
    }
    
    public override void ApplyChanges(Pawn pawn, BodyPartRecord bodyPartRecord, Pawn billDoer)
    {
        var trait = pawn.story.traits.allTraits.FirstOrDefault(t =>
            t.def == Trait && t.Degree == TraitDegree);
        if (trait != null)
        {
            GenSpawn.Spawn(TraitThing, billDoer.Position, billDoer.Map);
            pawn.story.traits.RemoveTrait(trait);
    
            DamagePart(bodyPartRecord, pawn, billDoer);
        }
    }

}