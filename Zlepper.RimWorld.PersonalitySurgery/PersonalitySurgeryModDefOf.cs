namespace Zlepper.RimWorld.PersonalitySurgery;

public static class PersonalitySurgeryDefs
{
    public static RecipeDef SurgeryExtractBioProperty => DefDatabase<RecipeDef>.GetNamed(nameof(SurgeryExtractBioProperty));
    public static RecipeDef SurgeryInstallBioProperty => DefDatabase<RecipeDef>.GetNamed(nameof(SurgeryInstallBioProperty));
    public static ThingDef SurgeryExtractBioTraitItem => DefDatabase<ThingDef>.GetNamed(nameof(SurgeryExtractBioTraitItem));
    public static ThingCategoryDef PersonalitySurgeryTraitCategory => DefDatabase<ThingCategoryDef>.GetNamed(nameof(PersonalitySurgeryTraitCategory));

    public static ThingCategoryDef PersonalitySurgeryPassionCategory =>
        DefDatabase<ThingCategoryDef>.GetNamed(nameof(PersonalitySurgeryPassionCategory));
}
