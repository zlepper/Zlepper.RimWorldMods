using System.Reflection;
using Zlepper.RimWorld.PersonalitySurgery.Recipes;

namespace Zlepper.RimWorld.PersonalitySurgery;

public static class TraitRecipeGenerator
{
    public static IEnumerable<(ExtractTraitRecipeDef extractRecipe, InstallTraitRecipeDef installRecipe, TraitThingDef traitItem)> GenerateDefs()
    {
        var thisAssembly = Assembly.GetExecutingAssembly();
        var thisMod = LoadedModManager.RunningMods.SingleOrDefault(m => m.assemblies.loadedAssemblies.Contains(thisAssembly));
        if (thisMod == null)
        {
            Log.Error("Could not find this mod 'PersonalitySurgery' in the loaded mods.");
            yield break;
        }

        foreach (var traitDef in DefDatabase<TraitDef>.AllDefs)
        {
            if (traitDef.GetGenderSpecificCommonality(Gender.None) == 0)
            {
                continue;
            }
            
            foreach (var degreeData in traitDef.degreeDatas)
            {
                var traitItemThing = DefGeneratorHelpers.CreateCopy<TraitThingDef>(PersonalitySurgeryDefs.SurgeryExtractBioTraitItem);
                traitItemThing.defName = $"trait{traitDef.defName}OfDegree{degreeData.degree}Item";
                traitItemThing.description = "TraitItemThingDescription".Translate(degreeData.label);
                traitItemThing.label = degreeData.label;
                traitItemThing.BaseMarketValue *= 1 + degreeData.marketValueFactorOffset;
                traitItemThing.modContentPack = thisMod;
                traitItemThing.Trait = traitDef;
                traitItemThing.TraitDegree = degreeData.degree;
                traitItemThing.comps.Add(new CompPropertiesLabel());
                

                var extractTraitRecipe =
                    DefGeneratorHelpers.CreateCopy<ExtractTraitRecipeDef>(PersonalitySurgeryDefs.SurgeryExtractBioProperty);
                extractTraitRecipe.defName = $"harvestTrait{traitDef.defName}OfDegree{degreeData.degree}Recipe";
                extractTraitRecipe.label = "ExtractTraitRecipeLabel".Translate(degreeData.label);
                extractTraitRecipe.description = "ExtractTraitRecipeDescription".Translate(degreeData.label);
                extractTraitRecipe.jobString = "ExtractTraitRecipeJobString".Translate(degreeData.label);
                extractTraitRecipe.Trait = traitDef;
                extractTraitRecipe.TraitDegree = degreeData.degree;
                extractTraitRecipe.TraitThing = traitItemThing;
                extractTraitRecipe.modContentPack = thisMod;

                var installTraitRecipe =
                    DefGeneratorHelpers.CreateCopy<InstallTraitRecipeDef>(PersonalitySurgeryDefs.SurgeryInstallBioProperty);
                installTraitRecipe.defName = $"installTrait{traitDef.defName}OfDegree{degreeData.degree}Recipe";
                installTraitRecipe.label = "InstallTraitRecipeLabel".Translate(degreeData.label);
                installTraitRecipe.description = "InstallTraitRecipeDescription".Translate(degreeData.label);
                installTraitRecipe.jobString = "InstallTraitRecipeJobString".Translate(degreeData.label);
                installTraitRecipe.modContentPack = thisMod;
                installTraitRecipe.Trait = traitDef;
                installTraitRecipe.TraitDegree = degreeData.degree;
                installTraitRecipe.TraitThing = traitItemThing;
                var filter = new ThingFilter();
                filter.SetAllow(traitItemThing, true);
                installTraitRecipe.ingredients = new List<IngredientCount>(installTraitRecipe.ingredients)
                {
                    new()
                    {
                        filter = filter,
                    }
                };

                var original = installTraitRecipe.fixedIngredientFilter;
                installTraitRecipe.fixedIngredientFilter = new ThingFilter();
                installTraitRecipe.fixedIngredientFilter.CopyAllowancesFrom(original);

                installTraitRecipe.fixedIngredientFilter.SetAllow(traitItemThing, true);

                DefGeneratorHelpers.HyperlinkAll(traitItemThing, extractTraitRecipe, installTraitRecipe);
                
                yield return (extractTraitRecipe, installTraitRecipe, traitItemThing);
            }
        }
    }
}