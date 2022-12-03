namespace Zlepper.RimWorld.ModSdk.Utilities;

public static class RegexPatterns
{
    public const string IntRange = @"(-?\d+)(?:~(-?\d+))?";
    public const string FloatRange = @"(-?\d+(?:\.\d+)?)(?:~(-?\d+(?:\.\d+)?))?";
    public const string Vector2 = @"\(?(-?\d+)(?:,(-?\d+)){0,1}\)?";
    public const string IntVec2 = @"\(?(-?\d+),(-?\d+)\)?";
    public const string IntVec3 = @"\(?(-?\d+),(-?\d+),(-?\d+)\)?";
    
}