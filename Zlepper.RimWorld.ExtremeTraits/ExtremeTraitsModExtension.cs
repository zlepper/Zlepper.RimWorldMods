using System.Collections.Generic;
using Verse;

namespace Zlepper.RimWorld.ExtremeTraits;

public class ExtremeTraitsModExtension : DefModExtension
{
    public int MaxDegree = 10;
    public int MinDegree = -10;

    public override IEnumerable<string> ConfigErrors()
    {
        if (MaxDegree < 1)
        {
            yield return "MaxDegree must be greater than 0";
        }

        if (MinDegree > -1)
        {
            yield return "MinDegree must be less than 0";
        }
    }
}