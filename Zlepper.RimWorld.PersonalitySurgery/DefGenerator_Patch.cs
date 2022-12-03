using HarmonyLib;

namespace Zlepper.RimWorld.PersonalitySurgery;

[HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve))]
public class DefGenerator_Patch
{
    [HarmonyPrefix]
    public static void Prefix()
    {
        try
        {
            foreach (var (extractRecipe, installRecipe, traitItem) in TraitRecipeGenerator.GenerateDefs())
            {
                DefGenerator.AddImpliedDef(extractRecipe);
                DefGenerator.AddImpliedDef<RecipeDef>(extractRecipe);
                DefGenerator.AddImpliedDef(installRecipe);
                DefGenerator.AddImpliedDef<RecipeDef>(installRecipe);
                DefGenerator.AddImpliedDef(traitItem);
                DefGenerator.AddImpliedDef<ThingDef>(traitItem);
            }

            
            foreach (var (extractRecipe, installRecipe, passionItem) in PassionRecipeGenerator.GenerateDefs())
            {
                DefGenerator.AddImpliedDef(extractRecipe);
                DefGenerator.AddImpliedDef<RecipeDef>(extractRecipe);
                DefGenerator.AddImpliedDef(installRecipe);
                DefGenerator.AddImpliedDef<RecipeDef>(installRecipe);
                
                DefGenerator.AddImpliedDef(passionItem);
                DefGenerator.AddImpliedDef<ThingDef>(passionItem);
            }

            DefGeneratorHelpers.RemoveFromDatabase(PersonalitySurgeryDefs.SurgeryExtractBioTraitItem);
            DefGeneratorHelpers.RemoveFromDatabase(PersonalitySurgeryDefs.SurgeryExtractBioProperty);
            DefGeneratorHelpers.RemoveFromDatabase(PersonalitySurgeryDefs.SurgeryInstallBioProperty);
        }
        catch (Exception e)
        {
            Log.Warning("Error in DefGenerator_Patch: " + e);
        }
    }
}