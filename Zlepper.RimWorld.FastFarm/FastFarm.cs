using HugsLib;
using Verse;

namespace Zlepper.Rimworld.FastFarm;

[StaticConstructorOnStartup]
public class FastFarm : ModBase
{
    public override string ModIdentifier => "Zlepper.RimWorld.FastFarm";

    public override void Initialize()
    {
        Logger.Message("Hello world!");
    }
}