namespace Zlepper.RimWorld.BioTechImprovements;

[DefOf]
public static class BioTechImprovementsWorkTypeDefOf
{
    public static WorkTypeDef Mechanitor = null!;
    
    static BioTechImprovementsWorkTypeDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(BioTechImprovementsWorkTypeDefOf));
    }
}