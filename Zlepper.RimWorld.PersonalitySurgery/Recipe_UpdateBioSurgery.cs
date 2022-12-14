using Zlepper.RimWorld.PersonalitySurgery.Recipes;

namespace Zlepper.RimWorld.PersonalitySurgery;

public class Recipe_UpdateBioSurgery : Recipe_Surgery
{
    public override bool AvailableOnNow(Thing thing, BodyPartRecord? part = null)
    {
        if (recipe is not BaseBioRecipeDef bioRecipe)
        {
            PersonalitySurgeryMod.ModLogger.Error(
                $"RecipeDef {recipe.defName} is not a {typeof(BaseBioRecipeDef)}, got {recipe.GetType()}");
            return false;
        }


        if (thing is not Pawn pawn)
        {
            PersonalitySurgeryMod.ModLogger.Trace($"Thing {thing.Label} is not a Pawn, got {thing.GetType()}");
            return false;
        }

        return bioRecipe.CheckIfSurgeryIsPossible(pawn);
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
        if (recipe is not BaseBioRecipeDef bioRecipe)
        {
            PersonalitySurgeryMod.ModLogger.Warning(
                $"RecipeDef {recipe.defName} is not a {typeof(BaseBioRecipeDef)}, got {recipe.GetType()}");
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

            bioRecipe.ApplyChanges(pawn, part, billDoer);
        }

        if (!IsViolationOnPawn(pawn, part, Faction.OfPlayer))
            return;
        ReportViolation(pawn, billDoer, pawn.HomeFaction, -70);
    }

    public override TaggedString GetConfirmation(Pawn pawn)
    {
        if (recipe is not BaseBioRecipeDef bioRecipe)
        {
            PersonalitySurgeryMod.ModLogger.Warning(
                $"RecipeDef {recipe.defName} is not a {typeof(BaseBioRecipeDef)}, got {recipe.GetType()}");
            return null;
        }
        
        return bioRecipe.GetConfirmation(pawn);
    }
}