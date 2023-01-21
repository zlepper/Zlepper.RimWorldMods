namespace Zlepper.RimWorld.RoyaltyImprovements;

internal class ValueCache<TKey, TValue>
    where TKey : class
    where TValue : class

{
    private TKey? _lastKey;
    private TValue? _lastValue;
    private readonly Func<TKey, TValue> _getValue;

    public ValueCache(Func<TKey, TValue> getValue)
    {
        _getValue = getValue;
    }

    public TValue Get(TKey key)
    {
        if (ReferenceEquals(_lastKey, key))
        {
            return _lastValue!;
        }

        _lastKey = key;
        return _lastValue = _getValue(key);
    }
}