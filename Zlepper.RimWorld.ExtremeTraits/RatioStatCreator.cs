namespace Zlepper.RimWorld.ExtremeTraits;

public class RatioStatCreator : IStatCreator
{
    private readonly float _ratio;

    public RatioStatCreator(float ratio)
    {
        _ratio = ratio;
    }


    public float GetValue(int degree)
    {
        return (float) Math.Pow(_ratio, 0 - Math.Abs(degree));
    }
}