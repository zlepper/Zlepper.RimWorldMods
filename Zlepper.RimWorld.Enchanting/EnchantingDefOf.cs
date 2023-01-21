namespace Zlepper.RimWorld.Enchanting;

[DefOf]
public static class EnchantingDefOf
{
    public static StatCategoryDef Enchanting = null!;
    
    static EnchantingDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(EnchantingDefOf));
    }
}