namespace Zlepper.RimWorld.Enchanting;

public class EnchantDef : Def
{
    private List<EnchantCompProperties>? properties = null;
    [Unsaved(false)]
    public EnchantComp[] Comps = Array.Empty<EnchantComp>();

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var configError in base.ConfigErrors())
        {
            yield return configError;
        }
        
        if (properties != null)
        {
            foreach (var prop in properties)
            {
                foreach (var configError in prop.ConfigErrors())
                {
                    yield return configError;
                }
            }
        }
    }

    public override void PostLoad()
    {
        if (properties == null)
        {
            return;
        }

        Comps = new EnchantComp[properties.Count];

        for (var i = 0; i < properties.Count; i++)
        {
            var enchantCompProperties = properties[i];
            var comp = (EnchantComp) Activator.CreateInstance(enchantCompProperties.CompClass);
            comp.Initialize(enchantCompProperties, this);
            Comps[i] = comp;
        }
    }

    public virtual bool IsAvailableForThing(Thing thing)
    {
        return Comps.Any(c => c.IsAvailableForThing(thing));
    }

    public virtual string GetEffectDescription(Thing thing, QualityCategory quality)
    {
        var descriptions = Comps.Select(c => c.GetEffectDescription(thing, quality)).Where(d => !d.NullOrEmpty());
        return string.Join("\n", descriptions);
    }

    public string? GetStatExplanation(StatRequest req, StatDef stat, QualityCategory quality)
    {
        var explanations = Comps.Select(c => c.GetStatExplanation(req, stat, quality)).Where(e => !e.NullOrEmpty());
        return string.Join("\n", explanations);
    }

    public void TransformValue(StatRequest req, StatDef stat, QualityCategory quality, ref float val)
    {
        var c = Comps;
        for (var i = 0; i < c.Length; i++)
        {
            c[i].TransformValue(req, stat, quality, ref val);
        }
    }
}