namespace Zlepper.RimWorld.ExtremeTraits;

public class LinearStatCreator : IStatCreator
{
    private readonly float _valuePerDegree;
    private readonly float _baseValue;

    public LinearStatCreator(float valuePerDegree, float baseValue)
    {
        _valuePerDegree = valuePerDegree;
        _baseValue = baseValue;
    }

    public float GetValue(int degree)
    {
        return _baseValue + _valuePerDegree * Math.Abs(degree);
    }
}