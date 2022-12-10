using HugsLib;
using HugsLib.Utils;

namespace Zlepper.RimWorld.TraitCrafting;

[EarlyInit]
public class TraitCraftingMod : ModBase
{
    public override string ModIdentifier => "Zlepper.RimWorld.TraitCrafting";

    public override void DefsLoaded()
    {
        var defs = DefDatabase<RecipeDef>.AllDefs.Where(d => d.ingredients.Any(i => i.filter.AllowedThingDefs.Any(innerD => innerD == null))).ToList();
        if (defs.Count == 0)
        {
            Logger.Message("All defs have all ingredients defined");
        }
        else
        {
            foreach (var recipeDef in defs)
            {
                Logger.Error($"The def {recipeDef.defName} has a null value in the filter");
            }
        }

        base.DefsLoaded();
    }
}