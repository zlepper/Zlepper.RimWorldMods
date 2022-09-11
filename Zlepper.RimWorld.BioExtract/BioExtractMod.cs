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

                traitItemThing.descriptionHyperlinks = new List<DefHyperlink>
                {
                    new(extractTraitRecipe),
                };
                extractTraitRecipe.descriptionHyperlinks = new List<DefHyperlink>
                {
                    new(traitItemThing)
                };


                RecipeDefDatabase.Add(extractTraitRecipe);
                ThingDefDatabase.Add(traitItemThing);
            }
        }

        RemoveFromDatabase(BioExtractModDefOf.SurgeryExtractBioProperty);
        RemoveFromDatabase(BioExtractModDefOf.SurgeryExtractBioTraitItem);
    }

    private void RemoveFromDatabase<T>(T def)
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
    private T CreateCopy<T>(object from)
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