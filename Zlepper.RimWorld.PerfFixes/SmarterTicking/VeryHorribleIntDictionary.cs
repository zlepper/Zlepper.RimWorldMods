using System.Runtime.CompilerServices;

namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

public sealed class VeryHorribleIntDictionary<TValue>
    where TValue : notnull
{
    private TValue?[] _values;
    
    public VeryHorribleIntDictionary(int initialCapacity = 1000000)
    {
        _values = new TValue[initialCapacity];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(int key, out TValue value)
    {
        var v = _values;
        if (key < v.Length)
        {
            var match = v[key];
            if (match != null)
            {
                value = match;
                return true;
            }
        }

        value = default!;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(int key, TValue value)
    {
        if (key >= _values.Length)
        {
            var newValues = new TValue[key * 2];
            Array.Copy(_values, newValues, _values.Length);
            _values = newValues;
        }

        _values[key] = value;
    }
}