namespace Zlepper.RimWorld.PersonalitySurgery;

public class Recipe_InstallBioPropertySurgery : Recipe_Surgery
{
    public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef recipe)
    {
        return MedicalRecipesUtility.GetFixedPartsToApplyOn(recipe, pawn, record =>
        {
            if (!pawn.health.hediffSet.GetNotMissingParts().Contains(record))
            {
                return false;
            }

            if (pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(record))
            {
                return false;
            }

            return !pawn.health.hediffSet.hediffs.Any(hediff => hediff.Part == record && (hediff.def == recipe.addsHediff || !recipe.CompatibleWithHediff(hediff.def)));
        });
    }

    public override AcceptanceReport AvailableReport(Thing thing, BodyPartRecord? part = null)
    {
        return AcceptanceReport.WasRejected;
    }

    protected override void OnSurgerySuccess(Pawn pawn, BodyPartRecord part, Pawn? billDoer, List<Thing> ingredients,
        Bill bill)
    {
        var personalityIngredient = ingredients.OfType<PersonalityThing>().FirstOrDefault();

        personalityIngredient?.OnSurgerySuccess(pawn);
    }

    public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn? billDoer, List<Thing> ingredients, Bill bill)
    {
        if (billDoer == null) 
            return;
        
        if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
            return;

        
        TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);

        OnSurgerySuccess(pawn, part, billDoer, ingredients, bill);
        
        if (!IsViolationOnPawn(pawn, part, Faction.OfPlayer))
            return;
            
        ReportViolation(pawn, billDoer, pawn.HomeFaction, -70);
    }

    public override string? LabelFromUniqueIngredients(Bill bill)
    {
        if (bill is Bill_Medical medical)
        {
            var ingredient = medical.uniqueRequiredIngredients.FirstOrDefault();
            if (ingredient != null)
            {
                switch (ingredient)
                {
                    case TraitThing:
                        return "Install trait " + ingredient.LabelCap;
                    case PassionThing:
                        return "Install passion " + ingredient.LabelCap;
                }
                PersonalitySurgeryMod.ModLogger.Warning($"Ingredient {ingredient.ToStringSafe()} is not a trait or passion");
            }
            else
            {
                PersonalitySurgeryMod.ModLogger.Warning($"Bill {bill.ToStringSafe()} had no unique ingredient");
            }
        }
        else
        {
            PersonalitySurgeryMod.ModLogger.Warning($"Bill {bill.ToStringSafe()} is not a Bill_Medical");
        }

        return null;
    }
}