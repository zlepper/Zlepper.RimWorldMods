namespace Zlepper.RimWorld.PerfFixes.FastReachability;

public sealed class FasterReachabilityComponent : MapComponent
{
    public readonly FasterReachability Reachability;
    
    public FasterReachabilityComponent(Map map) : base(map)
    {
        Reachability = new FasterReachability(map);
    }

    public override void MapGenerated()
    {
        Reachability.BuildConnectedAreas();
    }
}