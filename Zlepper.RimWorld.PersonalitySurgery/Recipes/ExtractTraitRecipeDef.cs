namespace Zlepper.RimWorld.PersonalitySurgery.Recipes;

public class ExtractTraitRecipeDef : BaseTraitRecipeDef
{
    
    public override bool CheckIfSurgeryIsPossible(Pawn pawn)
    {
        var matches = pawn.story.traits.allTraits.FirstOrDefault(t =>
            t.def == Trait && t.Degree == TraitDegree && t.sourceGene == null && !t.Suppressed);
        return matches != null;
    }
    
    public override void ApplyChanges(Pawn pawn, Pawn billDoer)
    {
        var trait = pawn.story.traits.allTraits.FirstOrDefault(t =>
            t.def == Trait && t.Degree == TraitDegree);
        if (trait != null)
        {
            GenSpawn.Spawn(TraitThing, billDoer.Position, billDoer.Map);
            pawn.story.traits.RemoveTrait(trait);
    
            HealthUtility.GiveRandomSurgeryInjuries(pawn, 15, pawn.health.hediffSet.GetBrain());
        }
    }

}