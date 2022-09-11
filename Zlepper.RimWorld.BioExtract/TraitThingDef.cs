using System.Collections.Generic;
using RimWorld;
using Verse;

namespace Zlepper.Rimworld.BioExtract;

public class TraitThingDef : ThingDef
{
    public TraitDef Trait = null!;

    public int TraitDegree;
    
    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var configError in base.ConfigErrors())
            yield return configError;

        if (Trait == null!)
        {
            yield return $"Trait was not set for def '{defName}'";
        }
    }
}