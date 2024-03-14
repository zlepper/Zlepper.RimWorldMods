using HarmonyLib;

namespace Zlepper.RimWorld.PersonalitySurgery;

[HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve), typeof(bool))]
public class DefGenerator_Patch
{
    [HarmonyPrefix]
    public static void Prefix()
    {
        try
        {
            Log.Message("DefGenerator_Patch.Prefix");
            var recipes = new List<RecipeDef>();
            foreach (var (extractRecipe, installRecipe, traitItem) in TraitRecipeGenerator.GenerateDefs())
            {
                DefGenerator.AddImpliedDef(extractRecipe);
                recipes.Add(extractRecipe);
                DefGenerator.AddImpliedDef(installRecipe);
                recipes.Add(installRecipe);
                DefGenerator.AddImpliedDef(traitItem);
                DefGenerator.AddImpliedDef<ThingDef>(traitItem);
            }

            
            foreach (var (extractRecipe, installRecipe, passionItem) in PassionRecipeGenerator.GenerateDefs())
            {
                DefGenerator.AddImpliedDef(extractRecipe);
                recipes.Add(extractRecipe);
                DefGenerator.AddImpliedDef(installRecipe);
                recipes.Add(installRecipe);
                
                DefGenerator.AddImpliedDef(passionItem);
                DefGenerator.AddImpliedDef<ThingDef>(passionItem);
            }

            recipes.SortBy(r => r.defName);
            
            foreach (var recipe in recipes)
            {
                DefGenerator.AddImpliedDef(recipe);
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