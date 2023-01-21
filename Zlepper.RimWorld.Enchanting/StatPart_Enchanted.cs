namespace Zlepper.RimWorld.Enchanting;

public class StatPart_Enchanted : StatPart
{
    public override void TransformValue(StatRequest req, ref float val)
    {
        if (req.Thing is ThingWithComps thingWithComps)
        {
            var enchantment = thingWithComps.GetComp<ThingComp_Enchantable>();
            if (enchantment != null)
            {
                enchantment.TransformValue(req, parentStat, ref val);
            }
        }
    }

    public override string? ExplanationPart(StatRequest req)
    {
        if (req.Thing is ThingWithComps thingWithComps)
        {
            var enchantment = thingWithComps.GetComp<ThingComp_Enchantable>();
            if (enchantment != null)
            {
                return enchantment.GetStatExplanation(req, parentStat);
            }
        }

        return null;
    }
    
    
}