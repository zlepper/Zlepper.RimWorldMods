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

    public static void Main(string[] args)
    {
        throw new NotImplementedException();
    }
}