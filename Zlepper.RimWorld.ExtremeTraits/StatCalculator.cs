using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace Zlepper.RimWorld.ExtremeTraits;

public static class StatCalculator
{
    public static IStatCreator InferStatValues(IReadOnlyList<float> values)
    {
        if (values.Count > 1)
        {
            if (Math.Abs(values[0]) > Math.Abs(values[1]))
            {
                var ratio = values[0] / values[1];
                return new RatioStatCreator(ratio);
            }
        }
        
        if(values.Count == 1)
        {
            return new LinearStatCreator(values[0], 0);
        }
        
        var distance = values.Zip(values.Skip(1), (a, b) => b - a).Average();
        return new LinearStatCreator(distance, values[0] - distance);
    }
}

public interface IStatCreator
{
    public float GetValue(int degree);
}

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