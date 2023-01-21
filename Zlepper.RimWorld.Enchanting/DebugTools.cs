namespace Zlepper.RimWorld.Enchanting;

public static class DebugTools
{
    [DebugAction("Enchanting", allowedGameStates = AllowedGameStates.PlayingOnMap)]
    public static List<DebugActionNode> EnchantItem()
    {
        return DefDatabase<EnchantDef>.AllDefs
            .Select(enchant => new DebugActionNode(enchant.LabelCap)
            {
                childGetter = () => Enum.GetValues(typeof(QualityCategory))
                    .Cast<QualityCategory>()
                    .Select(quality =>
                    {
                        return new DebugActionNode(quality.GetLabel(), DebugActionType.ToolMap, () =>
                        {
                            if (Find.CurrentMap == null)
                            {
                                return;
                            }

                            var thing = UI.MouseCell()
                                .GetFirstThingWithComp<ThingComp_Enchantable>(Find.CurrentMap);
                                
                            if (thing == null)
                            {
                                Messages.Message("Select an item to enchant", MessageTypeDefOf.RejectInput);
                                return;
                            }

                            var enchantComp = thing.TryGetComp<ThingComp_Enchantable>();

                            if (enchantComp == null || !enchant.IsAvailableForThing(thing))
                            {
                                Messages.Message("This enchantment is not available for this item",
                                    MessageTypeDefOf.RejectInput);
                                return;
                            }

                            var enchantment = new AppliedEnchant()
                            {
                                EnchantDef = enchant,
                                Quality = quality
                            };
                            enchantComp.AddEnchant(enchantment);
                        });
                    })
                    .ToList()
            })
            .ToList();
    }
}