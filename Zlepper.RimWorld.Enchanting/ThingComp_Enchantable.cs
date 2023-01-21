using System.Text;

namespace Zlepper.RimWorld.Enchanting;

public sealed class ThingComp_Enchantable : ThingComp
{
    public List<AppliedEnchant> AppliedEnchants = new();

    public override string? CompInspectStringExtra()
    {
        if (AppliedEnchants.Count == 0)
        {
            return null;
        }

        return string.Join("\n", AppliedEnchants.Select(e => e.EnchantDef.LabelCap + ": " + e.Quality.GetLabel()));
    }

    public override string? GetDescriptionPart()
    {
        return CompInspectStringExtra();
    }

    public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
    {
        if (AppliedEnchants.Count == 0)
        {
            yield return new StatDrawEntry(EnchantingDefOf.Enchanting, "Applied enchants",
                "None", "No enchants has been applied to this", 0);
        }
        else
        {
            var links = AppliedEnchants.Select(e => new Dialog_InfoCard.Hyperlink(e.EnchantDef));
            var valueString = string.Join(", ", AppliedEnchants.Select(e => e.EnchantDef.LabelCap));
            var reportText = string.Join("\n", AppliedEnchants.Select(e => e.GetReportText(parent)));
            yield return new StatDrawEntry(EnchantingDefOf.Enchanting, "Applied enchants",
                valueString, $"Enchants that has been applied to this item:\n\n{reportText}", 0, hyperlinks: links);
        }

        var applicableEnchants = DefDatabase<EnchantDef>.AllDefs
            .Where(enchantDef => enchantDef.IsAvailableForThing(parent))
            .ToList();

        if (applicableEnchants.Count != 0)
        {
            var links = applicableEnchants.Select(e => new Dialog_InfoCard.Hyperlink(e));
            var valueString = string.Join(", ", applicableEnchants.Select(e => e.LabelCap));
            yield return new StatDrawEntry(EnchantingDefOf.Enchanting, "Applicable enchants",
                valueString, "Enchants that can be applied to this item", 1, hyperlinks: links);
        }
    }

    public override void PostExposeData()
    {
        Scribe_Collections.Look(ref AppliedEnchants, nameof(AppliedEnchants), LookMode.Deep);
    }

    public void AddEnchant(AppliedEnchant enchantment)
    {
        var existing = AppliedEnchants.FirstOrDefault(e => e.EnchantDef == enchantment.EnchantDef);
        if (existing == null)
        {
            AppliedEnchants.Add(enchantment);
        }
        else
        {
            existing.Quality = enchantment.Quality;
        }
    }

    public void TransformValue(StatRequest req, StatDef stat, ref float val)
    {
        for (var index = 0; index < AppliedEnchants.Count; index++)
        {
            var appliedEnchant = AppliedEnchants[index];
            appliedEnchant.TransformValue(req, stat, ref val);
        }
    }

    public string? GetStatExplanation(StatRequest req, StatDef stat)
    {
        var explanationParts = new StringBuilder();
        foreach (var appliedEnchant in AppliedEnchants)
        {
            var explanation = appliedEnchant.GetStatExplanation(req, stat);
            if (explanation != null)
            {
                explanationParts.AppendLine(explanation);
            }
        }

        if (explanationParts.Length == 0)
        {
            return null;
        }

        return explanationParts.ToString().TrimEndNewlines();
    }
}