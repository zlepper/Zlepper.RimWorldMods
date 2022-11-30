using System.Runtime.CompilerServices;

namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

public sealed class ThingTickReducer
{
    private readonly VeryHorribleIntDictionary<int> _cache = new();

    private readonly int _minTicksBetweenChecks;
    private readonly int _maxTicketsBetweenChecks;
    private readonly Random r = new();
    
    public ThingTickReducer(int minTicksBetweenChecks, int maxTicketsBetweenChecks)
    {
        _minTicksBetweenChecks = minTicksBetweenChecks;
        _maxTicketsBetweenChecks = maxTicketsBetweenChecks;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool AllowTick(Thing t)
    {
        var id = t.thingIDNumber;
        _cache.TryGetValue(id, out var ticks);

        ticks--;
        var doTick = ticks < 0;
        if (doTick)
        {
            ticks = r.Next(_minTicksBetweenChecks, _maxTicketsBetweenChecks);
        }
        _cache.Set(id, ticks);

        return doTick;
    }
}