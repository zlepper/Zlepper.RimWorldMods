
using System.Reflection;
using HarmonyLib;

namespace Zlepper.RimWorld.TraitCrafting;

public class TraitCraftingMod : Mod
{
    public TraitCraftingMod(ModContentPack content) : base(content)
    {
        var harmony = new Harmony("dk.zlepper.rimworld.traitcrafting");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}