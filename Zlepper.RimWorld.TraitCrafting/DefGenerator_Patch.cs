using HarmonyLib;
using Zlepper.RimWorld.PersonalitySurgery;

namespace Zlepper.RimWorld.TraitCrafting;

[HarmonyAfter(PersonalitySurgeryMod.ModIdentifierValue)]
[HarmonyPatch(typeof(DefGenerator), nameof(DefGenerator.GenerateImpliedDefs_PreResolve))]
public class DefGenerator_Patch
{
    [HarmonyPrefix]
    public static void Prefix()
    {
        try
        {
            foreach (var recipe in RecipeDefGenerator.GenerateRecipes())
            {
                DefGenerator.AddImpliedDef(recipe);
            }
        }
        catch (Exception e)
        {
            Log.Error("Error while generating implied defs for TraitCrafting: " + e);
        }
    }
}