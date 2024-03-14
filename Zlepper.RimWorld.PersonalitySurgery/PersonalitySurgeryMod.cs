
using System.Reflection;
using HarmonyLib;

namespace Zlepper.RimWorld.PersonalitySurgery;

public class PersonalitySurgeryMod : Mod
{
    public const string ModIdentifierValue = "Zlepper.RimWorld.PersonalitySurgery";

    public PersonalitySurgeryMod(ModContentPack content) : base(content)
    {
        var harmony = new Harmony("dk.zlepper.rimworld.PersonalitySurgery");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
    
    
}