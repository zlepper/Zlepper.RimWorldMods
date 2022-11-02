namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

internal sealed class CachedValue<T>
{
    public int TicksLeft;
    public T Value;

    public CachedValue(int ticksLeft, T value)
    {
        TicksLeft = ticksLeft;
        Value = value;
    }
}