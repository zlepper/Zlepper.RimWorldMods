using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Zlepper.Rimworld.BioExtract;

public class Recipe_ExtractBioProperty : Recipe_Surgery
{
    public override bool AvailableOnNow(Thing thing, BodyPartRecord? part = null)
    {
        if (recipe is not TraitRecipeDef traitRecipeDef)
        {
            BioExtractMod.ModLogger.Warning(
                $"RecipeDef {recipe.defName} is not a TraitRecipeDef, got {recipe.GetType()}");
            return false;
        }

        if (traitRecipeDef.Trait == null!)
        {
            return false;
        }

        if (thing is not Pawn pawn)
        {
            BioExtractMod.ModLogger.Trace($"Thing {thing.Label} is not a Pawn, got {thing.GetType()}");
            return false;
        }

        var matches = pawn.story.traits.allTraits.FirstOrDefault(t =>
            t.def == traitRecipeDef.Trait && t.Degree == traitRecipeDef.TraitDegree);
        return matches != null;
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

        BioExtractMod.ModLogger.Message($"Applying on pawn {pawn}");
        MedicalRecipesUtility.IsClean(pawn, part);
        if (billDoer != null)
        {
            if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
                return;
            TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);
            if (!pawn.health.hediffSet.GetNotMissingParts().Contains<BodyPartRecord>(part))
                return;
            var trait = pawn.story.traits.allTraits.FirstOrDefault(t =>
                t.def == traitRecipeDef.Trait && t.Degree == traitRecipeDef.TraitDegree);
            if (trait != null)
            {
                GenSpawn.Spawn(traitRecipeDef.TraitThing, billDoer.Position, billDoer.Map);
                pawn.story.traits.RemoveTrait(trait);
            }
        }
        if (!IsViolationOnPawn(pawn, part, Faction.OfPlayer))
            return;
        ReportViolation(pawn, billDoer, pawn.HomeFaction, -70);
    }
}