using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using HugsLib;
using HugsLib.Utils;
using RimWorld;
using Verse;
using Zlepper.RimWorld.PersonalitySurgery;

namespace Zlepper.RimWorld.TraitCrafting;

public class TraitCraftingMod : ModBase
{
    
    public override string ModIdentifier => "Zlepper.RimWorld.TraitCrafting";
    
    
    public override void DefsLoaded()
    {
        if (!ModIsActive)
        {
            return;
        }


        var relevantThings = DefDatabase<ThingDef>.AllDefs
            .Where(d => d.HasModExtension<PersonalitySurgeryDefModExtension>())
            .GroupBy(d => d.GetModExtension<PersonalitySurgeryDefModExtension>().Trait)
            .ToDictionary(g => g.Key, g => g.ToDictionary(d => d.GetModExtension<PersonalitySurgeryDefModExtension>().TraitDegree));

        foreach (var traitDef in DefDatabase<TraitDef>.AllDefs)
        {
            var positiveDegrees = traitDef.degreeDatas.Where(x => x.degree > 0).OrderBy(d => d.degree).ToList();
            var negativeDegrees = traitDef.degreeDatas.Where(x => x.degree < 0).OrderByDescending(d => d.degree).ToList();

            if (positiveDegrees.Count > 0)
            {
                for (var i = 1; i < positiveDegrees.Count; i++)
                {
                    var previousDegree = positiveDegrees[i - 1];
                    var previousThing = relevantThings.GetValueSafe(traitDef)?.GetValueSafe(previousDegree.degree);
                    
                    
                    var thisDegree = positiveDegrees[i];
                    var thisThing = relevantThings.GetValueSafe(traitDef)?.GetValueSafe(thisDegree.degree);

                    if (previousThing == null || thisThing == null)
                    {
                        continue;
                    }
                    
                    var recipe = CreateCopy<RecipeDef>(TraitCraftingModDefOf.TraitCraftingBaseRecipe);
                    
                    recipe.defName = $"combineTraits{traitDef.defName}Level{i - 1}To{i}Recipe";
                    recipe.label = $"Combine {previousDegree.label} to {thisDegree.label}";
                    recipe.description = $"Combine {previousDegree.label} to {thisDegree.label}.";
                    recipe.jobString = $"Combining {previousDegree.label} to {thisDegree.label}.";
                    recipe.generated = true;
                    
                    var filter = new ThingFilter();
                    filter.SetAllow(previousThing, true);
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
                        new(thisThing, 1)
                    };
                    HyperlinkAll(previousThing, thisThing, recipe);

                    InjectedDefHasher.GiveShortHashToDef(recipe, typeof(RecipeDef));
                    DefDatabase<RecipeDef>.Add(recipe);
                    
                    TraitCraftingModDefOf.TraitCombiner.recipes.Add(recipe);
                }
            }

            if (negativeDegrees.Count > 0)
            {
                
            }
        }
        
        RemoveFromDatabase(TraitCraftingModDefOf.TraitCraftingBaseRecipe);
    }
    
    private static void RemoveFromDatabase<T>(T def)
        where T : Def
    {
        const string removeMethodName = "Remove";
        var removeMethod =
            typeof(DefDatabase<T>).GetMethod(removeMethodName, BindingFlags.Static | BindingFlags.NonPublic);
        if (removeMethod == null)
        {
            throw new MissingMethodException(typeof(DefDatabase<T>).FullName, removeMethodName);
        }

        removeMethod.Invoke(null, new object[] {def});
    }

    private static void HyperlinkAll(params Def[] defs)
    {
        for (var i = 0; i < defs.Length; i++)
        {
            var defI = defs[i];
            defI.descriptionHyperlinks ??= new List<DefHyperlink>();
            for (var j = 0; j < defs.Length; j++)
            {
                if (i == j)
                {
                    continue;
                }

                defI.descriptionHyperlinks.AddDistinct(new DefHyperlink(defs[j]));
            }
        }
    }

    
    /// <summary>
    /// Creates a shallow copy of the specified element for further modification
    /// </summary>
    private static T CreateCopy<T>(Def from)
        where T : Def, new()
    {
        if (from == null) throw new ArgumentNullException(nameof(from));

        var fromType = from.GetType();
        if (!fromType.IsAssignableFrom(typeof(T)))
        {
            throw new ArgumentException($"type of {typeof(T)} cannot be generated from {fromType}");
        }

        var newInstance = new T();

        foreach (var field in fromType.GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            if (field.Name == nameof(Def.shortHash))
            {
                continue;
            }
            
            if (field.IsLiteral || field.IsInitOnly)
            {
                continue;
            }

            field.SetValue(newInstance, field.GetValue(from));
        }

        if (newInstance is BuildableDef newBuildable && from is BuildableDef fromBuildable)
        {
            newBuildable.statBases = fromBuildable.statBases.Select(s => new StatModifier()
            {
                stat = s.stat,
                value = s.value
            }).ToList();
        }

        return newInstance;
    }
}