namespace Zlepper.RimWorld.PersonalitySurgery.Recipes;

public abstract class BaseTraitRecipeDef : BaseBioRecipeDef
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
        else
        {
            if (Trait.degreeDatas.All(d => d.degree != TraitDegree))
            {
                yield return $"Degree {TraitDegree} does not exist for trait {Trait.defName}";
            }
        }

        if (TraitThing == null!)
        {
            yield return $"TraitItem was not set for def '{defName}'";
        }
        
    }
}
