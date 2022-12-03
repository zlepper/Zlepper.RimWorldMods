using System.Reflection;
using Zlepper.RimWorld.PersonalitySurgery.Recipes;

namespace Zlepper.RimWorld.PersonalitySurgery;

public static class PassionRecipeGenerator
{
    public static IEnumerable<(ExtractPassionRecipeDef extractRecipe, InstallPassionRecipeDef installRecipe, PassionThingDef passionItem)> GenerateDefs()
    {
        var thisAssembly = Assembly.GetExecutingAssembly();
        var thisMod = LoadedModManager.RunningMods.SingleOrDefault(m => m.assemblies.loadedAssemblies.Contains(thisAssembly));
        if (thisMod == null)
        {
            Log.Error("Could not find this mod 'PersonalitySurgery' in the loaded mods.");
            yield break;
        }
        
        var passionValues = new[] {Passion.Minor, Passion.Major};

        foreach (var skillDef in DefDatabase<SkillDef>.AllDefs)
        {
            foreach (var passion in passionValues)
            {
                var passionItemThing = DefGeneratorHelpers.CreateCopy<PassionThingDef>(PersonalitySurgeryDefs.SurgeryExtractBioTraitItem);
                passionItemThing.defName = $"skillPassion{skillDef.defName}OfDegree{passion}";
                passionItemThing.description = $"Passion{passion}ItemThingDescription".Translate(skillDef.label);
                passionItemThing.label = $"Passion{passion}ItemThingLabel".Translate(skillDef.label);
                passionItemThing.BaseMarketValue *= (float) passion;
                passionItemThing.thingCategories = passionItemThing.thingCategories.ToList();
                passionItemThing.thingCategories.Remove(PersonalitySurgeryDefs.PersonalitySurgeryTraitCategory);
                passionItemThing.thingCategories.Add(PersonalitySurgeryDefs.PersonalitySurgeryPassionCategory);
                passionItemThing.comps.Add(new CompPropertiesLabel());
                passionItemThing.modContentPack = thisMod;
                passionItemThing.SkillDef = skillDef;
                passionItemThing.Passion = passion;

                var extractPassionRecipe =
                    DefGeneratorHelpers.CreateCopy<ExtractPassionRecipeDef>(PersonalitySurgeryDefs.SurgeryExtractBioProperty);
                extractPassionRecipe.defName = $"harvestSkillPassion{skillDef.defName}OfDegree{passion}";
                extractPassionRecipe.label = $"Extract{passion}PassionRecipeLabel".Translate(skillDef.label);
                extractPassionRecipe.description =
                    $"Extract{passion}PassionRecipeDescription".Translate(skillDef.label);
                extractPassionRecipe.jobString = $"Extract{passion}PassionRecipeJobString".Translate(skillDef.label);
                extractPassionRecipe.Skill = skillDef;
                extractPassionRecipe.Passion = passion;
                extractPassionRecipe.PassionThing = passionItemThing;
                extractPassionRecipe.modContentPack = thisMod;

                var installPassionRecipe =
                    DefGeneratorHelpers.CreateCopy<InstallPassionRecipeDef>(PersonalitySurgeryDefs.SurgeryInstallBioProperty);
                installPassionRecipe.defName = $"installSkillPassion{skillDef.defName}OfDegree{passion}";
                installPassionRecipe.label = $"Install{passion}PassionRecipeLabel".Translate(skillDef.label);
                installPassionRecipe.description =
                    $"Install{passion}PassionRecipeDescription".Translate(skillDef.label);
                installPassionRecipe.jobString = $"Install{passion}PassionRecipeJobString".Translate(skillDef.label);
                installPassionRecipe.Skill = skillDef;
                installPassionRecipe.Passion = passion;
                installPassionRecipe.PassionThing = passionItemThing;
                installPassionRecipe.modContentPack = thisMod;
                var filter = new ThingFilter();
                filter.SetAllow(passionItemThing, true);
                installPassionRecipe.ingredients = new List<IngredientCount>(installPassionRecipe.ingredients)
                {
                    new()
                    {
                        filter = filter,
                    }
                };

                var original = installPassionRecipe.fixedIngredientFilter;
                installPassionRecipe.fixedIngredientFilter = new ThingFilter();
                installPassionRecipe.fixedIngredientFilter.CopyAllowancesFrom(original);

                installPassionRecipe.fixedIngredientFilter.SetAllow(passionItemThing, true);

                DefGeneratorHelpers.HyperlinkAll(passionItemThing, extractPassionRecipe, installPassionRecipe);

                yield return (extractPassionRecipe, installPassionRecipe, passionItemThing);
            }
        }
    }
}