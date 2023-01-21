namespace Zlepper.RimWorld.Enchanting;

public class AppliedEnchant : IExposable
{
    public EnchantDef EnchantDef = null!;
    public QualityCategory Quality;

    public void ExposeData()
    {
        Scribe_Defs.Look(ref EnchantDef, nameof(EnchantDef));
        Scribe_Values.Look(ref Quality, nameof(Quality));
    }

    public string GetReportText(ThingWithComps thing)
    {
        return EnchantDef.LabelCap + ": " + Quality.GetLabel() + "\n" + EnchantDef.GetEffectDescription(thing, Quality).Indented();
    }

    public string? GetStatExplanation(StatRequest req, StatDef stat)
    {
        return EnchantDef.GetStatExplanation(req, stat, Quality);
    }

    public void TransformValue(StatRequest req, StatDef stat, ref float val)
    {
        EnchantDef.TransformValue(req, stat, Quality, ref val);
    }
}