using RimWorld.QuestGen;

namespace Zlepper.RimWorld.ModSdk.Tests.Utilities.RimWorldVerseCopy;

public class BackstoryDef
{
    public Dictionary<SkillDef, int> skillGains = new Dictionary<SkillDef, int>();
    
    public SlateRef<float?> selectionWeight;
}