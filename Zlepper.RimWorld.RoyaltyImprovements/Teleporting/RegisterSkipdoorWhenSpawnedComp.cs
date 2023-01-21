using VanillaPsycastsExpanded.Skipmaster;

namespace Zlepper.RimWorld.RoyaltyImprovements.Teleporting;


public class RegisterSkipdoorWhenSpawnedComp : ThingComp
{
    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        if (parent != null)
        {
            var map = parent.Map;
            if (map != null)
            {
                var tracker = map.GetComponent<TeleporterDistanceTracker>();
                if (tracker == null)
                {
                    tracker = new TeleporterDistanceTracker(map);
                    map.components.Add(tracker);
                }

                tracker.RegisterTeleporter(parent);
            }
        }
    }

    public override void PostDeSpawn(Map map)
    {
        if (parent != null)
        {
            map.GetComponent<TeleporterDistanceTracker>()?.RemoveTeleporter(parent);
        }
    }
}