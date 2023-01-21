namespace Zlepper.RimWorld.Enchanting;

public class EnchantComp
{
    protected EnchantCompProperties Props = null!;
    protected EnchantDef EnchantDef = null!;
    
    public virtual void Initialize(EnchantCompProperties props, EnchantDef enchantDef)
    {
        Props = props;
        EnchantDef = enchantDef;
    }

    public virtual bool IsAvailableForThing(Thing thing)
    {
        return false;
    }

    public virtual string GetEffectDescription(Thing thing, QualityCategory quality)
    {
        return "";
    }
    
    public virtual void TransformValue(StatRequest req, StatDef stat, QualityCategory quality, ref float val)
    {
    }
    
    public virtual string? ExplanationPart(StatRequest req)
    {
        return null;
    }

    public virtual string? GetStatExplanation(StatRequest req, StatDef stat, QualityCategory quality)
    {
        return null;
    }
}