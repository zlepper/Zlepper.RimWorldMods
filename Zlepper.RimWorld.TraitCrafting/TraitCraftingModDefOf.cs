using RimWorld;
using Verse;

namespace Zlepper.RimWorld.TraitCrafting;

[DefOf]
public static class TraitCraftingModDefOf
{
    public static ThingDef TraitCombiner = null!;
    public static RecipeDef TraitCraftingBaseRecipe = null!;
    
    static TraitCraftingModDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(TraitCraftingModDefOf));
    }
}