namespace Zlepper.RimWorld.RoyaltyImprovements;

[DefOf]
public static class JobDefOf
{
    public static JobDef MeditateMore = null!;
    public static JobDef ReignMore = null!;

    static JobDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(JobDefOf));
    }
}