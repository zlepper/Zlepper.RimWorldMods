namespace Zlepper.RimWorld.Enchanting;

public class HediffCompProperties_SoulTrapped : HediffCompProperties
{
    public HediffCompProperties_SoulTrapped()
    {
        compClass = typeof(HediffComp_SoulTrapped);
    }
}

public class HediffComp_SoulTrapped : HediffComp
{
    public override void Notify_PawnDied()
    {
        Pawn.health.RemoveHediff(parent);

        var map = Pawn.MapHeld;
        if (map == null)
        {
            return;
        }

        var availableSoulGems = map.listerThings.ThingsOfDef(EnchantingDefOf.soulGemEmpty) ?? new();
        if (availableSoulGems.Count == 0)
        {
            return;
        }

        var soulGemsOnPawns = map.mapPawns.AllPawnsSpawned
            .SelectMany(p => p.inventory.GetDirectlyHeldThings().Where(t => t.def == EnchantingDefOf.soulGemEmpty));

        availableSoulGems.AddRange(soulGemsOnPawns);

        var nearest = availableSoulGems.MinBy(t => t.PositionHeld.DistanceToSquared(Pawn.PositionHeld));

        var distance = nearest.PositionHeld.DistanceTo(Pawn.PositionHeld);

        if (distance > 20)
        {
            Log.Message("Soul trapped pawn died too far from soul gem");
            return;
        }

        Thing target;
        if (nearest.stackCount > 1)
        {
            target = nearest.SplitOff(1);
        }
        else
        {
            target = nearest;
        }

        var charged = GenSpawn.Spawn(EnchantingDefOf.soulGemCharged, nearest.PositionHeld, map,
            WipeMode.VanishOrMoveAside);

        var chainedFleckManager = map.GetComponent<MapComponent_ChainedFleckManager>();
        if (chainedFleckManager != null)
        {
            var chainedFlecks = EnchantingDefOf.SoulTrapped.Spawn(Pawn.Corpse, charged);
            chainedFleckManager.Add(chainedFlecks);
        }

        target.Destroy();

        PreventRevival();
    }


    private static void ShowChargeFleck(Thing charged, Map map)
    {
        var fleckData =
            FleckMaker.GetDataStatic(charged.PositionHeld.ToVector3Shifted(), map, FleckDefOf.EntropyPulse, 2) with
            {
                rotationRate = Rand.Range(-30, 30),
                rotation = 90 * Rand.RangeInclusive(0, 3),
            };
        map.flecks.CreateFleck(fleckData);
    }

    private void PreventRevival()
    {
        Pawn.Corpse.GetComp<CompRottable>()?.RotImmediately();
    }
}