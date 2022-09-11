using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Zlepper.Rimworld.BioExtract;

public abstract class Recipe_UpdateTrait : Recipe_Surgery
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

        return CheckIfSurgeryIsPossible(pawn, recipeTrait, traitDegree);
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


            ApplyTraitChanges(pawn, traitRecipeDef, billDoer);
        }

        if (!IsViolationOnPawn(pawn, part, Faction.OfPlayer))
            return;
        ReportViolation(pawn, billDoer, pawn.HomeFaction, -70);
    }

    protected abstract bool CheckIfSurgeryIsPossible(Pawn pawn, TraitDef recipeTrait, int traitDegree);

    protected abstract void ApplyTraitChanges(Pawn pawn, TraitRecipeDef traitRecipeDef, Pawn billDoer);
}