using System.Linq;
using RimWorld;
using Verse;

namespace Zlepper.Rimworld.BioExtract;

public class Recipe_ExtractBioProperty : Recipe_UpdateTrait
{
    protected override bool CheckIfSurgeryIsPossible(Pawn pawn, TraitDef recipeTrait, int traitDegree)
    {
        
        var matches = pawn.story.traits.allTraits.FirstOrDefault(t =>
            t.def == recipeTrait && t.Degree == traitDegree);
        return matches != null;
    }

    protected override void ApplyTraitChanges(Pawn pawn, TraitRecipeDef traitRecipeDef, Pawn billDoer)
    {
        
        var trait = pawn.story.traits.allTraits.FirstOrDefault(t =>
            t.def == traitRecipeDef.Trait && t.Degree == traitRecipeDef.TraitDegree);
        if (trait != null)
        {
            GenSpawn.Spawn(traitRecipeDef.TraitThing, billDoer.Position, billDoer.Map);
            pawn.story.traits.RemoveTrait(trait);
        }
    }
}