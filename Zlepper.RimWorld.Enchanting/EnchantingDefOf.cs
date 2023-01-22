namespace Zlepper.RimWorld.Enchanting;

[DefOf]
public static class EnchantingDefOf
{
    public static StatCategoryDef Enchanting = null!;
    public static ThingDef soulGemEmpty = null!;
    public static ThingDef soulGemCharged = null!;
    public static ChainedFlecksDef SoulTrapped = null!;
    
    static EnchantingDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(EnchantingDefOf));
    }
}