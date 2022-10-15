namespace Zlepper.RimWorld.PersonalitySurgery;

public class PersonalitySurgeryDefModExtension : DefModExtension
{
    public TraitDef Trait = null!;
    public int TraitDegree;

    public PersonalitySurgeryDefModExtension Copy()
    {
        return new PersonalitySurgeryDefModExtension()
        {
            Trait = Trait,
            TraitDegree = TraitDegree
        };
    }
}