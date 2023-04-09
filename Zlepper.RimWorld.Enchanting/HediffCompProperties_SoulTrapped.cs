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

        var availableSoulGems = GetAvailableSoulGems(map);
        if (availableSoulGems.Count == 0)
        {
            return;
        }
        
        var nearest = availableSoulGems.MinBy(t => t.PositionHeld.DistanceToSquared(Pawn.PositionHeld));

        var distance = nearest.PositionHeld.DistanceTo(Pawn.PositionHeld);

        if (distance > 20)
        {
            Log.Message("Soul trapped pawn died too far from soul gem");
            return;
        }

        var target = nearest.stackCount > 1 ? nearest.SplitOff(1) : nearest;

        var charged = GenSpawn.Spawn(EnchantingDefOf.soulGemCharged, nearest.PositionHeld, map);

        AnimateSoulTrap(map, charged);

        target.Destroy();

        PreventRevival();
    }

    private static List<Thing> GetAvailableSoulGems(Map map)
    {
        var availableSoulGems = map.listerThings.ThingsOfDef(EnchantingDefOf.soulGemEmpty)?.ToList() ?? new();


        var soulGemsOnPawns = map.mapPawns.AllPawnsSpawned
            .SelectMany(p => p.inventory.innerContainer.Where(t => t.def == EnchantingDefOf.soulGemEmpty))
            .ToList();
        
        availableSoulGems.AddRange(soulGemsOnPawns);
        return availableSoulGems;
    }

    private void AnimateSoulTrap(Map map, Thing charged)
    {
        var chainedFleckManager = map.GetComponent<MapComponent_ChainedFleckManager>();
        if (chainedFleckManager != null)
        {
            var chainedFlecks = EnchantingDefOf.SoulTrapped.Spawn(Pawn.Corpse, charged, map);
            chainedFleckManager.Add(chainedFlecks);
        }
    }


    private void PreventRevival()
    {
        Pawn.Corpse.GetComp<CompRottable>()?.RotImmediately();
    }
}