using System.Reflection;
using Zlepper.RimWorld.PersonalitySurgery;

namespace Zlepper.RimWorld.TraitCrafting;

public static class RecipeDefGenerator
{
    public static IEnumerable<RecipeDef> GenerateRecipes()
    {
        var thisAssembly = Assembly.GetExecutingAssembly();
        var thisMod = LoadedModManager.RunningMods.SingleOrDefault(m => m.assemblies.loadedAssemblies.Contains(thisAssembly));
        if (thisMod == null)
        {
            Log.Error("Could not find this mod 'Trait crafting' in the loaded mods.");
            yield break;
        }

        var relevantThings = DefDatabase<TraitThingDef>.AllDefs
            .GroupBy(d => d.Trait);

        foreach (var group in relevantThings)
        {
            var things = group.OrderBy(t => t.TraitDegree).ToList();

            if (things.Count <= 1)
            {
                continue;
            }

            var fromThings = things.SkipLast(1);
            var toThings = things.Skip(1);
            
            foreach (var (from, to) in fromThings.Zip(toThings, (from, to) => (from, to)))
            {
                
                var recipe = DefGeneratorHelpers.CreateCopy<RecipeDef>(TraitCraftingModDefs.TraitCraftingBaseRecipe);
                    
                recipe.defName = $"combineTrait_{from.defName}_{to.defName}";
                recipe.label = $"Combine {from.label} to {to.label}";
                recipe.description = $"Combine {from.label} to {to.label}.";
                recipe.jobString = $"Combining {from.label} to {to.label}.";
                recipe.modContentPack = thisMod;

                var filter = new ThingFilter();
                filter.SetAllow(from, true);
                var traitIngredient = new IngredientCount()
                {
                    filter = filter,
                };
                traitIngredient.SetBaseCount(2);
                recipe.ingredients = new List<IngredientCount>(recipe.ingredients)
                {
                    traitIngredient
                };
                recipe.products = new List<ThingDefCountClass>
                {
                    new(to, 1)
                };
                DefGeneratorHelpers.HyperlinkAll(from, to, recipe);

                yield return recipe;
            }
        }

        DefGeneratorHelpers.RemoveFromDatabase(TraitCraftingModDefs.TraitCraftingBaseRecipe);
    }
}