using System.Runtime.CompilerServices;

namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

public sealed class PawnLazyTicker<TValue>
    where TValue : notnull
{
    private readonly VeryHorribleIntDictionary<TValue> _cache = new();
    private readonly TickQueue _tickQueue;

    public PawnLazyTicker(int maxTicksBetweenChecks)
    {
        _tickQueue = new TickQueue(maxTicksBetweenChecks);
    }

    public void Tick()
    {
        foreach (var key in _tickQueue.Next())
        {
            _cache.Remove(key);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(Pawn pawn, out TValue value)
    {
        return _cache.TryGetValue(pawn.thingIDNumber, out value);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(Pawn pawn, TValue value)
    {
        var id = pawn.thingIDNumber;
        _tickQueue.Add(id);
        _cache.Set(id, value);
    }

    public void Reset()
    {
        _cache.Clear();
        _tickQueue.Clear();
    }
}