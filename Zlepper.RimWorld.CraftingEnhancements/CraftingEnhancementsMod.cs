using System.Reflection;
using HarmonyLib;

namespace Zlepper.RimWorld.CraftingEnhancements;

public class CraftingEnhancementsMod : Mod
{
    public CraftingEnhancementsMod(ModContentPack content) : base(content)
    {
        var harmony = new Harmony("dk.zlepper.rimworld.crafting-enhancements");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}