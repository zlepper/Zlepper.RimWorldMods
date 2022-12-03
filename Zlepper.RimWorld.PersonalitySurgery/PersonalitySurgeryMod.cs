using HugsLib;
using HugsLib.Utils;

namespace Zlepper.RimWorld.PersonalitySurgery;

[EarlyInit]
public class PersonalitySurgeryMod : ModBase
{
    public const string ModIdentifierValue = "Zlepper.RimWorld.PersonalitySurgery";

    public override string ModIdentifier => ModIdentifierValue;

    public static ModLogger ModLogger => new(ModIdentifierValue);
}