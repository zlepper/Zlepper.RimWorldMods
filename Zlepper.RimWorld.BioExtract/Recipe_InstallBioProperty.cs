using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Zlepper.Rimworld.BioExtract;

public class Recipe_InstallBioProperty : Recipe_UpdateTrait
{
    protected override bool CheckIfSurgeryIsPossible(Pawn pawn, TraitDef recipeTrait, int traitDegree)
    {
        var existing = pawn.story.traits.allTraits.FirstOrDefault(t => t.def == recipeTrait);

        BioExtractMod.ModLogger.Message(
            $"Checking if installation of trait '{recipeTrait.defName}' is available on pawn {pawn}. Found: {existing?.def};{existing?.Degree}");
        return existing?.Degree != traitDegree;
    }
    
    protected override void ApplyTraitChanges(Pawn pawn, TraitRecipeDef traitRecipeDef, Pawn billDoer)
    {
        var newTrait = traitRecipeDef.Trait;

        var traitsToRemove = new List<Trait>();


        var sameTrait = pawn.story.traits.allTraits.FirstOrDefault(t => t.def == newTrait);
        if (sameTrait != null)
        {
            traitsToRemove.Add(sameTrait);
        }

        var incompatibleTraits = pawn.story.traits.allTraits.Where(t =>
            newTrait.conflictingTraits.Contains(t.def) || t.def.conflictingTraits.Contains(newTrait));
        traitsToRemove.AddRange(incompatibleTraits);

        foreach (var trait in traitsToRemove)
        {
            pawn.story.traits.RemoveTrait(trait);
        }

        pawn.story.traits.GainTrait(new Trait(newTrait, traitRecipeDef.TraitDegree));
    }
}