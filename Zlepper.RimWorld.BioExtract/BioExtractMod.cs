using System;
using System.Collections.Generic;
using System.Reflection;
using HugsLib;
using HugsLib.Utils;
using RimWorld;
using Verse;
using RecipeDefDatabase = Verse.DefDatabase<Verse.RecipeDef>;
using ThingDefDatabase = Verse.DefDatabase<Verse.ThingDef>;

namespace Zlepper.Rimworld.BioExtract;

[EarlyInit]
public class BioExtractMod : ModBase
{
    public override string ModIdentifier => "Zlepper.RimWorld.BioExtract";

    public override void EarlyInitialize()
    {
        _instance = this;
    }

    public override void Initialize()
    {
    }

    public override void DefsLoaded()
    {
        if (!ModIsActive)
        {
            return;
        }

        foreach (var traitDef in DefDatabase<TraitDef>.AllDefs)
        {
            foreach (var degreeData in traitDef.degreeDatas)
            {
                Logger.TraceFormat("Registering recipe for trait {0} of degree {1}", traitDef.defName,
                    degreeData.label);


                var traitItemThing = CreateCopy<TraitThingDef>(BioExtractModDefOf.SurgeryExtractBioTraitItem);
                traitItemThing.defName = $"trait{traitDef.defName}OfDegree{degreeData.degree}";
                traitItemThing.description = $"The trait '{degreeData.label}'.";
                traitItemThing.label = $"Trait ({degreeData.label})";
                traitItemThing.BaseMarketValue *= 1 + degreeData.marketValueFactorOffset;
                traitItemThing.Trait = traitDef;
                traitItemThing.TraitDegree = degreeData.degree;


                var extractTraitRecipe = CreateCopy<TraitRecipeDef>(BioExtractModDefOf.SurgeryExtractBioProperty);
                extractTraitRecipe.defName = $"harvestTrait{traitDef.defName}OfDegree{degreeData.degree}";
                extractTraitRecipe.label = $"Harvest trait ({degreeData.label})";
                extractTraitRecipe.description = $"Harvests the trait '{degreeData.label}', forcefully.";
                extractTraitRecipe.jobString = $"Harvesting {degreeData.label} trait";
                extractTraitRecipe.generated = true;
                extractTraitRecipe.Trait = traitDef;
                extractTraitRecipe.TraitDegree = degreeData.degree;
                extractTraitRecipe.TraitThing = traitItemThing;

                var installTraitRecipe = CreateCopy<TraitRecipeDef>(BioExtractModDefOf.SurgeryInstallBioProperty);
                installTraitRecipe.defName = $"installTrait{traitDef.defName}OfDegree{degreeData.degree}";
                installTraitRecipe.label = $"Install trait ({degreeData.label})";
                installTraitRecipe.description = $"Install the trait '{degreeData.label}'";
                installTraitRecipe.jobString = $"Install trait ({degreeData.label})";
                installTraitRecipe.generated = true;
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
                
                HyperlinkAll(traitItemThing, extractTraitRecipe, installTraitRecipe);
                
                RecipeDefDatabase.Add(extractTraitRecipe);
                RecipeDefDatabase.Add(installTraitRecipe);
                ThingDefDatabase.Add(traitItemThing);
            }
        }

        RemoveFromDatabase(BioExtractModDefOf.SurgeryExtractBioProperty);
        RemoveFromDatabase(BioExtractModDefOf.SurgeryInstallBioProperty);
        RemoveFromDatabase(BioExtractModDefOf.SurgeryExtractBioTraitItem);
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
                defI.descriptionHyperlinks.Add(new DefHyperlink(defs[j]));
            }
        }
    }
    
    private static void RemoveFromDatabase<T>(T def)
        where T : Def
    {
        const string removeMethodName = "Remove";
        var removeMethod = typeof(DefDatabase<T>).GetMethod(removeMethodName, BindingFlags.Static | BindingFlags.NonPublic);
        if (removeMethod == null)
        {
            throw new MissingMethodException(typeof(DefDatabase<T>).FullName, removeMethodName);
        }

        removeMethod.Invoke(null, new object[] {def});
    }

    public static ModLogger ModLogger => _instance.Logger;

    private static BioExtractMod _instance = null!;

    /// <summary>
    /// Creates a shallow copy of the specified element for further modification
    /// </summary>
    private static T CreateCopy<T>(object from)
        where T : notnull
    {
        if (from == null) throw new ArgumentNullException(nameof(from));

        var fromType = from.GetType();
        if (!fromType.IsAssignableFrom(typeof(T)))
        {
            throw new ArgumentException($"type of {typeof(T)} cannot be generated from {fromType}");
        }

        var newInstance = Activator.CreateInstance<T>();

        foreach (var field in fromType.GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            if (field.IsLiteral || field.IsInitOnly)
            {
                continue;
            }

            field.SetValue(newInstance, field.GetValue(from));
        }

        return newInstance;
    }
}