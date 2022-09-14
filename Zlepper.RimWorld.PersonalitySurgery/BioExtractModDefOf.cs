using RimWorld;
using Verse;

namespace Zlepper.RimWorld.PersonalitySurgery;

[DefOf]
public static class PersonalitySurgeryModDefOf
{
    public static RecipeDef SurgeryExtractBioProperty = null!;
    public static RecipeDef SurgeryInstallBioProperty = null!;
    public static ThingDef SurgeryExtractBioTraitItem = null!;

    static PersonalitySurgeryModDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(PersonalitySurgeryModDefOf));
    }
}