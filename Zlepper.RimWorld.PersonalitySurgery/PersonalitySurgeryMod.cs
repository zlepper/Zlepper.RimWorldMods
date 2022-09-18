using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HugsLib;
using HugsLib.Utils;
using RimWorld;
using Verse;
using Zlepper.RimWorld.PersonalitySurgery.Recipes;
using RecipeDefDatabase = Verse.DefDatabase<Verse.RecipeDef>;
using ThingDefDatabase = Verse.DefDatabase<Verse.ThingDef>;

namespace Zlepper.RimWorld.PersonalitySurgery;

public class PersonalitySurgeryMod : ModBase
{
    internal const string ModIdentifierValue = "Zlepper.RimWorld.PersonalitySurgery";
    
    public override string ModIdentifier => ModIdentifierValue;

    public override void DefsLoaded()
    {
        if (!ModIsActive)
        {
            return;
        }
        

        RegisterTraitRecipes();

        RegisterPassionRecipes();

        foreach (var cat in PersonalitySurgeryModDefOf.SurgeryExtractBioTraitItem.thingCategories)
        {
            cat.childThingDefs.Remove(PersonalitySurgeryModDefOf.SurgeryExtractBioTraitItem);
        }
        
        RemoveFromDatabase(PersonalitySurgeryModDefOf.SurgeryExtractBioProperty);
        RemoveFromDatabase(PersonalitySurgeryModDefOf.SurgeryInstallBioProperty);
        RemoveFromDatabase(PersonalitySurgeryModDefOf.SurgeryExtractBioTraitItem);

        PersonalitySurgeryModDefOf.PersonalitySurgeryTraitCategory.ResolveReferences();
        PersonalitySurgeryModDefOf.PersonalitySurgeryPassionCategory.ResolveReferences();

    }

    private void RegisterPassionRecipes()
    {
        var passionValues = new[] {Passion.Minor, Passion.Major};

        foreach (var skillDef in DefDatabase<SkillDef>.AllDefs)
        {
            foreach (var passion in passionValues)
            {
                var passionItemThing = CreateCopy<ThingDef>(PersonalitySurgeryModDefOf.SurgeryExtractBioTraitItem);
                passionItemThing.defName = $"skillPassion{skillDef.defName}OfDegree{passion}";
                passionItemThing.description = $"Passion{passion}ItemThingDescription".Translate(skillDef.label);
                passionItemThing.label = $"Passion{passion}ItemThingLabel".Translate(skillDef.label);
                passionItemThing.BaseMarketValue *= (float) passion;
                passionItemThing.thingCategories = passionItemThing.thingCategories.ToList();
                passionItemThing.thingCategories.Remove(PersonalitySurgeryModDefOf.PersonalitySurgeryTraitCategory);
                passionItemThing.thingCategories.Add(PersonalitySurgeryModDefOf.PersonalitySurgeryPassionCategory);

                var extractPassionRecipe =
                    CreateCopy<ExtractPassionRecipeDef>(PersonalitySurgeryModDefOf.SurgeryExtractBioProperty);
                extractPassionRecipe.defName = $"harvestSkillPassion{skillDef.defName}OfDegree{passion}";
                extractPassionRecipe.label = $"Extract{passion}PassionRecipeLabel".Translate(skillDef.label);
                extractPassionRecipe.description =
                    $"Extract{passion}PassionRecipeDescription".Translate(skillDef.label);
                extractPassionRecipe.jobString = $"Extract{passion}PassionRecipeJobString".Translate(skillDef.label);
                extractPassionRecipe.generated = true;
                extractPassionRecipe.Skill = skillDef;
                extractPassionRecipe.Passion = passion;
                extractPassionRecipe.PassionThing = passionItemThing;

                var installPassionRecipe =
                    CreateCopy<InstallPassionRecipeDef>(PersonalitySurgeryModDefOf.SurgeryInstallBioProperty);
                installPassionRecipe.defName = $"installSkillPassion{skillDef.defName}OfDegree{passion}";
                installPassionRecipe.label = $"Install{passion}PassionRecipeLabel".Translate(skillDef.label);
                installPassionRecipe.description =
                    $"Install{passion}PassionRecipeDescription".Translate(skillDef.label);
                installPassionRecipe.jobString = $"Install{passion}PassionRecipeJobString".Translate(skillDef.label);
                installPassionRecipe.generated = true;
                installPassionRecipe.Skill = skillDef;
                installPassionRecipe.Passion = passion;
                installPassionRecipe.PassionThing = passionItemThing;
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

                HyperlinkAll(passionItemThing, extractPassionRecipe, installPassionRecipe);

                GiveShortHash(passionItemThing);
                GiveShortHash<RecipeDef>(extractPassionRecipe);
                GiveShortHash<RecipeDef>(installPassionRecipe);
                RecipeDefDatabase.Add(extractPassionRecipe);
                RecipeDefDatabase.Add(installPassionRecipe);
                ThingDefDatabase.Add(passionItemThing);
                
                foreach (var cat in passionItemThing.thingCategories)
                {
                    cat.childThingDefs.Add(passionItemThing);
                }
            }
        }
    }

    private void GiveShortHash<TDef>(TDef def)
        where TDef : Def
    {
        InjectedDefHasher.GiveShortHashToDef(def, typeof(TDef));
        
    }

    private void RegisterTraitRecipes()
    {
        foreach (var traitDef in DefDatabase<TraitDef>.AllDefs)
        {
            foreach (var degreeData in traitDef.degreeDatas)
            {
                var traitItemThing = CreateCopy<ThingDef>(PersonalitySurgeryModDefOf.SurgeryExtractBioTraitItem);
                traitItemThing.defName = $"trait{traitDef.defName}OfDegree{degreeData.degree}";
                traitItemThing.description = "TraitItemThingDescription".Translate(degreeData.label);
                traitItemThing.label = "TraitItemThingLabel".Translate(degreeData.label);
                traitItemThing.BaseMarketValue *= 1 + degreeData.marketValueFactorOffset;
                
                traitItemThing.modExtensions = traitItemThing.modExtensions?.ToList() ?? new();
                var modExtension = traitItemThing.GetModExtension<PersonalitySurgeryDefModExtension>()?.Copy() ?? new();
                modExtension.Trait = traitDef;
                modExtension.TraitDegree = degreeData.degree;
                traitItemThing.modExtensions.Remove(modExtension);
                traitItemThing.modExtensions.Add(modExtension);
                traitItemThing.comps = traitItemThing.comps.ToList();
                traitItemThing.comps.Add(new CompPropertiesLabel(degreeData.LabelCap));
                

                var extractTraitRecipe =
                    CreateCopy<ExtractTraitRecipeDef>(PersonalitySurgeryModDefOf.SurgeryExtractBioProperty);
                extractTraitRecipe.defName = $"harvestTrait{traitDef.defName}OfDegree{degreeData.degree}";
                extractTraitRecipe.label = "ExtractTraitRecipeLabel".Translate(degreeData.label);
                extractTraitRecipe.description = "ExtractTraitRecipeDescription".Translate(degreeData.label);
                extractTraitRecipe.jobString = "ExtractTraitRecipeJobString".Translate(degreeData.label);
                extractTraitRecipe.generated = true;
                extractTraitRecipe.Trait = traitDef;
                extractTraitRecipe.TraitDegree = degreeData.degree;
                extractTraitRecipe.TraitThing = traitItemThing;

                var installTraitRecipe =
                    CreateCopy<InstallTraitRecipeDef>(PersonalitySurgeryModDefOf.SurgeryInstallBioProperty);
                installTraitRecipe.defName = $"installTrait{traitDef.defName}OfDegree{degreeData.degree}";
                installTraitRecipe.label = "InstallTraitRecipeLabel".Translate(degreeData.label);
                installTraitRecipe.description = "InstallTraitRecipeDescription".Translate(degreeData.label);
                installTraitRecipe.jobString = "InstallTraitRecipeJobString".Translate(degreeData.label);
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

                GiveShortHash(traitItemThing);
                GiveShortHash<RecipeDef>(extractTraitRecipe);
                GiveShortHash<RecipeDef>(installTraitRecipe);
                RecipeDefDatabase.Add(extractTraitRecipe);
                RecipeDefDatabase.Add(installTraitRecipe);
                ThingDefDatabase.Add(traitItemThing);
             
                foreach (var cat in traitItemThing.thingCategories)
                {
                    cat.childThingDefs.Add(traitItemThing);
                }
            }
        }
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
        var removeMethod =
            typeof(DefDatabase<T>).GetMethod(removeMethodName, BindingFlags.Static | BindingFlags.NonPublic);
        if (removeMethod == null)
        {
            throw new MissingMethodException(typeof(DefDatabase<T>).FullName, removeMethodName);
        }

        removeMethod.Invoke(null, new object[] {def});
    }

    public static ModLogger ModLogger => new(ModIdentifierValue);

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