using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Zlepper.RimWorld.PersonalitySurgery.Recipes;

public abstract class BaseTraitRecipeDef : BaseBioRecipeDef
{
    public TraitDef Trait = null!;
    public int TraitDegree;
    public ThingDef TraitThing = null!;

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var configError in base.ConfigErrors())
            yield return configError;

        if (Trait == null!)
        {
            yield return $"Trait was not set for def '{defName}'";
        }

        if (TraitThing == null!)
        {
            yield return $"TraitItem was not set for def '{defName}'";
        }
    }
}
