using System.Reflection;
using HarmonyLib;

namespace Zlepper.RimWorld.BioTechImprovements;

public class BioTechImprovementsMod : Mod
{
    public BioTechImprovementsMod(ModContentPack content) : base(content)
    {
        var harmony = new Harmony("dk.zlepper.rimworld.biotech-improvements");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}