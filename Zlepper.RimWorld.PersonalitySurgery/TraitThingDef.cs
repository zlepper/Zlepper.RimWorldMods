namespace Zlepper.RimWorld.PersonalitySurgery;

public sealed class TraitThingDef : ThingDef
{
    public TraitDef Trait = null!;
    public int TraitDegree;

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var configError in base.ConfigErrors())
        {
            yield return configError;
        }
        
        if (Trait == null!)
        {
            yield return "Trait is null";
        }
        else
        {
            if (Trait.degreeDatas.All(d => d.degree != TraitDegree))
            {
                yield return $"TraitDegree {TraitDegree} is not valid for Trait {Trait.defName}";
            }
        }
    }
}