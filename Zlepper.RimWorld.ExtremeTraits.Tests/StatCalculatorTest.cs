using System.Collections.Generic;
using NUnit.Framework;
using RimWorld;

namespace Zlepper.RimWorld.ExtremeTraits.Tests;

[TestFixture]
public class StatCalculatorTest
{
    
    [TestCaseSource(nameof(InfersOffsetsData))]
    public void InfersNextValue(float[] values, float[] expected)
    {
        var provider = StatCalculator.InferStatValues(values);
        
        for(var degree = 0; degree < expected.Length; degree++)
        {
            var degreeWithOffset = degree + values.Length + 1;
            Assert.That(provider.GetValue(degreeWithOffset), Is.EqualTo(expected[degree]));
        }
    }

    private static readonly float[][][] InfersOffsetsData = new[]
    {
        new[]
        {
            new[] {0.08f, 0.16f},
            new[] {0.24f, 0.32f}
        },
        new[]
        {
            new[] {1f, 2f},
            new[] {3f, 4f}
        },
        new[]
        {
            new[] {2f, 3f},
            new[] {4f, 5f}
        },
        new[]
        {
            new[] {1f},
            new[] {2f, 3f, 4f}
        },
        new[]
        {
            new[] {-0.08f, -0.16f},
            new[] {-0.24f, -0.32f}
        },
        new[]
        {
            new[] {-1f, -2f},
            new[] {-3f, -4f}
        },
        new[]
        {
            new[] {-1f},
            new[] {-2f, -3f, -4f}
        },
        new []
        {
            new []{0.5f, 0.25f},
            new []{0.125f, 0.125f/2},
        }
    };
}