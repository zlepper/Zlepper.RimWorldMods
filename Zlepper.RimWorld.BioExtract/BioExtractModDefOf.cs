using RimWorld;
using Verse;

namespace Zlepper.Rimworld.BioExtract;

[DefOf]
public static class BioExtractModDefOf
{
    public static RecipeDef SurgeryExtractBioProperty = null!;
    public static ThingDef SurgeryExtractBioTraitItem = null!;

    static BioExtractModDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(BioExtractModDefOf));
    }
}