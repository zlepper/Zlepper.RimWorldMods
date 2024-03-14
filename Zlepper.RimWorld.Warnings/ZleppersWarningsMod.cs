
using System.Reflection;
using HarmonyLib;

namespace Zlepper.RimWorld.Warnings;

public class ZleppersWarningsMod : Mod
{
    public ZleppersWarningsMod(ModContentPack content) : base(content)
    {
        var harmony = new Harmony("dk.zlepper.rimworld.warnings");
        harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
}