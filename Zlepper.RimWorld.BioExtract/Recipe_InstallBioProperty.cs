using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Zlepper.Rimworld.BioExtract;

public class Recipe_InstallBioProperty : Recipe_Surgery
{
    public override bool AvailableOnNow(Thing thing, BodyPartRecord? part = null)
    {
        if (recipe is not TraitRecipeDef {Trait: { } recipeTrait, TraitDegree: var traitDegree})
        {
            BioExtractMod.ModLogger.Error(
                $"RecipeDef {recipe.defName} is not a TraitRecipeDef, got {recipe.GetType()}");
            return false;
        }


        if (thing is not Pawn pawn)
        {
            BioExtractMod.ModLogger.Trace($"Thing {thing.Label} is not a Pawn, got {thing.GetType()}");
            return false;
        }

        var existing = pawn.story.traits.allTraits.FirstOrDefault(t => t.def == recipeTrait);

        BioExtractMod.ModLogger.Message($"Checking if installation of trait '{recipeTrait.defName}' is available on pawn {pawn}. Found: {existing?.def};{existing?.Degree}");
        return existing?.Degree != traitDegree;
    }

    public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
    {
        return MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn, record =>
        {
            if (!pawn.health.hediffSet.GetNotMissingParts().Contains<BodyPartRecord>(record))
            {
                return false;
            }

            if (pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(record))
            {
                return false;
            }

            return !pawn.health.hediffSet.hediffs.Any(hediff =>
            {
                if (hediff.Part != record)
                    return false;

                return hediff.def == recipe.addsHediff || !recipe.CompatibleWithHediff(hediff.def);
            });
        });
    }

    public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn? billDoer, List<Thing> ingredients, Bill bill)
    {
        if (recipe is not TraitRecipeDef traitRecipeDef)
        {
            BioExtractMod.ModLogger.Warning(
                $"RecipeDef {recipe.defName} is not a TraitRecipeDef, got {recipe.GetType()}");
            return;
        }

        MedicalRecipesUtility.IsClean(pawn, part);
        if (billDoer != null)
        {
            if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                return;
            TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
            if (!pawn.health.hediffSet.GetNotMissingParts().Contains<BodyPartRecord>(part))
                return;


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

        if (!IsViolationOnPawn(pawn, part, Faction.OfPlayer))
            return;
        ReportViolation(pawn, billDoer, pawn.HomeFaction, -70);
    }
}