namespace Zlepper.RimWorld.PersonalitySurgery;

public sealed class PassionThingDef : ThingDef
{
    public SkillDef SkillDef = null!;
    public Passion Passion;
    
    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var configError in base.ConfigErrors())
        {
            yield return configError;
        }
        
        if (SkillDef == null!)
        {
            yield return "SkillDef is null";
        }
        
        if(!Enum.IsDefined(typeof(Passion), Passion))
        {
            yield return $"Passion {Passion} is not valid";
        }
    }
}