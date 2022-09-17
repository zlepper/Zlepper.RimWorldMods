using RimWorld;
using Verse;

namespace Zlepper.RimWorld.PersonalitySurgery;

[DefOf]
public static class PersonalitySurgeryModDefOf
{
    public static RecipeDef SurgeryExtractBioProperty = null!;
    public static RecipeDef SurgeryInstallBioProperty = null!;
    public static ThingDef SurgeryExtractBioTraitItem = null!;
    public static ThingCategoryDef PersonalitySurgeryTraitCategory = null!;
    public static ThingCategoryDef PersonalitySurgeryPassionCategory = null!;

    static PersonalitySurgeryModDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(PersonalitySurgeryModDefOf));
    }
}