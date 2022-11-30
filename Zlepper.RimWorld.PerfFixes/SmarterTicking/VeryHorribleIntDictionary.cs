using System.Runtime.CompilerServices;

namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

public sealed class VeryHorribleIntDictionary<TValue>
    where TValue : notnull
{
    private ValueEntry<TValue>[] _values;
    
    public VeryHorribleIntDictionary(int initialCapacity = 1000000)
    {
        _values = new ValueEntry<TValue>[initialCapacity];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(int key, out TValue value)
    {
        var v = _values;
        if (key < v.Length)
        {
            var match = v[key];
            value = match.Value;
            return match.HasValue;
        }

        value = default!;
        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(int key, TValue value)
    {
        if (key >= _values.Length)
        {
            var newValues = new ValueEntry<TValue>[key * 2];
            Array.Copy(_values, newValues, _values.Length);
            _values = newValues;
        }

        _values[key] = new ValueEntry<TValue>(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Remove(int key)
    {
        var v = _values;
        if (key < v.Length)
        {
            v[key] = default;
        }
    }

    private readonly struct ValueEntry<T>
    {
        public readonly bool HasValue;
        public readonly T Value;
        
        public ValueEntry(T value)
        {
            HasValue = true;
            Value = value;
        }
    }

    public void Clear()
    {
        Array.Clear(_values, 0, _values.Length);
    }
}