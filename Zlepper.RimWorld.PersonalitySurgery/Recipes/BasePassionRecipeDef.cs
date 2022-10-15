namespace Zlepper.RimWorld.PersonalitySurgery.Recipes;

public abstract class BasePassionRecipeDef : BaseBioRecipeDef
{
    public SkillDef Skill = null!;
    public Passion Passion;
    
    public ThingDef PassionThing = null!;

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var configError in base.ConfigErrors())
            yield return configError;

        if (Skill == null!)
        {
            yield return $"Skill was not set for def '{defName}'";
        }
        
        
        if (PassionThing == null!)
        {
            yield return $"PassionThing was not set for def '{defName}'";
        }
    }
}