using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Zlepper.Rimworld.BioExtract;

public class TraitRecipeDef : RecipeDef
{
    public TraitDef Trait = null!;

    public int TraitDegree;
    public TraitThingDef TraitThing = null!;

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