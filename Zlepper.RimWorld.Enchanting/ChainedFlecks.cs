namespace Zlepper.RimWorld.Enchanting;

public class ChainedFlecksDef : Def
{
    public List<ChildFlecksDef> ChildFlecks = null!;

    public override IEnumerable<string> ConfigErrors()
    {
        foreach (var configError in base.ConfigErrors())
        {
            yield return configError;
        }

        if (ChildFlecks == null! || ChildFlecks.Count == 0)
        {
            yield return $"No child flecks defined for ChainedFlecksDef {defName}";
            yield break;
        }
        
        foreach (var childFlecksDef in ChildFlecks)
        {
            foreach (var configError in childFlecksDef.ConfigErrors())
            {
                yield return configError;
            }
        }
    }
    
    public ChainedFlecks Spawn(TargetInfo source)
    {
        return new ChainedFlecks(this, source);
    }

    public ChainedFlecks Spawn(TargetInfo source, TargetInfo target)
    {
        return new ChainedFlecks(this, source, target);
    }
}

public class ChildFlecksDef
{
    public FleckDef FleckDef = null!;
    public FloatRange Scale = new FloatRange(1f, 1f);
    public ChainFleckSpawnType ChainFleckSpawnType = ChainFleckSpawnType.OnSource;

    public IEnumerable<string> ConfigErrors()
    {
        if (FleckDef == null!)
        {
            yield return "No FleckDef defined for ChildFlecksDef";
        }
    }
}

public enum ChainFleckSpawnType
{
    OnSource,
    OnTarget,
    BetweenSourceAndTarget
}

public class ChainedFlecks
{
    private readonly ChainedFlecksDef _parent;
    private int _fleckIndex = -1;
    private int _fleckTicksRemaining = 0;
    private TargetInfo _source;
    private TargetInfo _target;

    public ChainedFlecks(ChainedFlecksDef parent, TargetInfo source, TargetInfo target)
    {
        _parent = parent;
        _source = source;
        _target = target;
    }

    public ChainedFlecks(ChainedFlecksDef parent, TargetInfo source)
    {
        _parent = parent;
        _source = source;
        _target = source;
    }

    /// <summary>
    /// Returns true if there are more flecks to spawn or we are still not done displaying in general
    /// </summary>
    public bool Tick()
    {
        if (_fleckTicksRemaining == 0)
        {
            _fleckIndex++;
            if(_fleckIndex >= _parent.ChildFlecks.Count)
                return false;

            var subFleck = _parent.ChildFlecks[_fleckIndex];

            if (subFleck.ChainFleckSpawnType == ChainFleckSpawnType.BetweenSourceAndTarget)
            {
                FleckMaker.ConnectingLine(_source.CenterVector3, _target.CenterVector3, subFleck.FleckDef, _source.Map);
            }
            else
            {
                var loc = subFleck.ChainFleckSpawnType == ChainFleckSpawnType.OnSource ? _source : _target;

                var creationData = FleckMaker.GetDataStatic(loc.CenterVector3, loc.Map, subFleck.FleckDef,
                    subFleck.Scale.RandomInRange);
                
                _source.Map.flecks.CreateFleck(creationData);
            }

            _fleckTicksRemaining = subFleck.FleckDef.Lifespan.SecondsToTicks();
        }
        else
        {
            _fleckTicksRemaining--;
        }

        // Still going
        return true;
    }
}