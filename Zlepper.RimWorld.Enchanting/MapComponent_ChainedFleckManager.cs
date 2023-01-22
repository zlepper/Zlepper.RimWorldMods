namespace Zlepper.RimWorld.Enchanting;

public class MapComponent_ChainedFleckManager : MapComponent
{
    private List<ChainedFlecks> _chainedFlecks = new();

    public void Add(ChainedFlecks chainedFlecks)
    {
        _chainedFlecks.Add(chainedFlecks);
    }

    public MapComponent_ChainedFleckManager(Map map) : base(map)
    {
    }

    public override void MapComponentTick()
    {
        for (var i = _chainedFlecks.Count - 1; i >= 0; i--)
        {
            var group = _chainedFlecks[i];
            if (!group.Tick())
            {
                _chainedFlecks.RemoveAt(i);
            }
        }
    }
}