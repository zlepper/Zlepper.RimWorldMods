namespace Zlepper.RimWorld.Enchanting.WeaponEnchants;

public class FeatherWeightEnchant_Properties : EnchantCompProperties
{
    public float AwfulReductionPercent;
    public float PoorReductionPercent;
    public float NormalReductionPercent;
    public float GoodReductionPercent;
    public float ExcellentReductionPercent;
    public float MasterworkReductionPercent;
    public float LegendaryReductionPercent;

    public FeatherWeightEnchant_Properties()
    {
        CompClass = typeof(FeatherWeightEnchant);
    }
}

public class FeatherWeightEnchant : EnchantComp
{
    public override bool IsAvailableForThing(Thing thing)
    {
        return thing.def.BaseMass > 0;
    }

    public override string GetEffectDescription(Thing thing, QualityCategory quality)
    {
        var weightReduction = GetWeightReduction(thing, quality);

        if (weightReduction > 0)
        {
            return "Reduces mass by " + weightReduction.ToStringMass() + ".";
        }
        else
        {
            return "Increases mass by " + weightReduction.ToStringMass() + ".";
        }
        
    }

    private float GetWeightReduction(Thing thing, QualityCategory quality)
    {
        var weightReductionPercent = GetWeightReductionPercent(quality);
        return thing.def.BaseMass * (weightReductionPercent / 100);
    }
    
    private float GetWeightReductionPercent(QualityCategory quality)
    {
        var props = (FeatherWeightEnchant_Properties) Props;
        switch (quality)
        {
            case QualityCategory.Awful:
                return props.AwfulReductionPercent;
            case QualityCategory.Poor:
                return props.PoorReductionPercent;
            case QualityCategory.Normal:
                return props.NormalReductionPercent;
            case QualityCategory.Good:
                return props.GoodReductionPercent;
            case QualityCategory.Excellent:
                return props.ExcellentReductionPercent;
            case QualityCategory.Masterwork:
                return props.MasterworkReductionPercent;
            case QualityCategory.Legendary:
                return props.LegendaryReductionPercent;
            default:
                return props.NormalReductionPercent;
        }
    }

    public override void TransformValue(StatRequest req, StatDef stat, QualityCategory quality, ref float val)
    {
        if (stat != StatDefOf.Mass || req.Thing == null)
        {
            return;
        }
        
        val *= (100f - GetWeightReductionPercent(quality)) / 100f;
    }

    public override string? GetStatExplanation(StatRequest req, StatDef stat, QualityCategory quality)
    {
        if (stat != StatDefOf.Mass || req.Thing == null)
        {
            return base.GetStatExplanation(req, stat, quality);
        }
        
        var weightReductionPercent = GetWeightReductionPercent(quality);

        return EnchantDef.LabelCap + " ("+ quality.GetLabel() + ") Enchant: x" + (100f - weightReductionPercent).ToStringDecimalIfSmall() + "%";
    }
}