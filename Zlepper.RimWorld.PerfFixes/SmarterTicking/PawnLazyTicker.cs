
using System.Runtime.CompilerServices;

namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

public sealed class PawnLazyTicker<TValue>
{
    private readonly VeryHorribleIntDictionary<CachedValue<TValue>> _cache = new();

    private readonly int _minTicksBetweenChecks;
    private readonly int _maxTicketsBetweenChecks;
    private readonly Random r = new();

    public PawnLazyTicker(int minTicksBetweenChecks, int maxTicketsBetweenChecks)
    {
        _minTicksBetweenChecks = minTicksBetweenChecks;
        _maxTicketsBetweenChecks = maxTicketsBetweenChecks;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TValue TickAndCalculate(Pawn pawn, Func<TValue> calculate)
    {
        var id = pawn.thingIDNumber;
        if (!_cache.TryGetValue(id, out var cachedValue))
        {
            cachedValue = new CachedValue<TValue>(1, default!);
            _cache.Set(id, cachedValue);
        }

        if (--cachedValue.TicksLeft == 0)
        {
            var result = calculate();

            cachedValue.Value = result;
            
            cachedValue.TicksLeft = r.Next(_minTicksBetweenChecks, _maxTicketsBetweenChecks);
        }

        return cachedValue.Value;
    }
}