namespace Zlepper.RimWorld.PersonalitySurgery;

[DefOf]
public static class PersonalitySurgeryModDefOf
{
    public static ThingDef SurgeryExtractBioTraitItem = null!;
    public static ThingDef SurgeryExtractBioPassionItem = null!;
    public static RecipeDef InstallBioPropertySurgery = null!;

    static PersonalitySurgeryModDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(PersonalitySurgeryModDefOf));
    }
}