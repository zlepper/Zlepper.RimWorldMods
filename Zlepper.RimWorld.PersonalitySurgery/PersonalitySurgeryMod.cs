using HugsLib;
using HugsLib.Utils;
using RecipeDefDatabase = Verse.DefDatabase<Verse.RecipeDef>;
using ThingDefDatabase = Verse.DefDatabase<Verse.ThingDef>;

namespace Zlepper.RimWorld.PersonalitySurgery;

public class PersonalitySurgeryMod : ModBase
{
    internal const string ModIdentifierValue = "Zlepper.RimWorld.PersonalitySurgery";
    
    public override string ModIdentifier => ModIdentifierValue;

    public static ModLogger ModLogger => new(ModIdentifierValue);
}