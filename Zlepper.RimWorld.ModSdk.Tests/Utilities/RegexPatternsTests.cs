using System.Text.RegularExpressions;
using Zlepper.RimWorld.ModSdk.Utilities;

namespace Zlepper.RimWorld.ModSdk.Tests.Utilities;

[TestFixture]
public class RegexPatternsTests
{
    [TestCase("1~5")]
    [TestCase("-2~8")]
    [TestCase("-200~-50")]
    [TestCase("1")]
    [TestCase("-2")]
    public void IntRange_Valid(string s)
    {
        var isMatch = Regex.IsMatch(s, RegexPatterns.IntRange);
        Assert.That(isMatch);
    }  
    
    [TestCase("1~5")]
    [TestCase("-2~8")]
    [TestCase("-200~-50")]
    [TestCase("1")]
    [TestCase("-2")]
    [TestCase("1.5~5.5")]
    [TestCase("-2.5~8.5")]
    [TestCase("-200.5~-50.5")]
    [TestCase("1.5")]
    [TestCase("-2.5")]
    public void FloatRange_Valid(string s)
    {
        var isMatch = Regex.IsMatch(s, RegexPatterns.FloatRange);
        Assert.That(isMatch);
    } 
    
    [TestCase("(1,2)")]
    [TestCase("(1)")]
    [TestCase("1")]
    [TestCase("(-1,2)")]
    [TestCase("(-1,-2)")]
    [TestCase("(-2)")]
    [TestCase("-1")]
    public void Vector2_Valid(string s)
    {
        var isMatch = Regex.IsMatch(s, RegexPatterns.Vector2);
        Assert.That(isMatch);
    }
    
    [TestCase("(1,2)")]
    [TestCase("(-1,2)")]
    [TestCase("(-1,-2)")]
    public void IntVec2_Valid(string s)
    {
        var isMatch = Regex.IsMatch(s, RegexPatterns.IntVec2);
        Assert.That(isMatch);
    }
    
    [TestCase("(1,2,3)")]
    [TestCase("(-1,2,3)")]
    [TestCase("(-1,-2,3)")]
    [TestCase("(1,2,-3)")]
    [TestCase("(-1,2,-3)")]
    [TestCase("(-1,-2,-3)")]
    public void IntVec3_Valid(string s)
    {
        var isMatch = Regex.IsMatch(s, RegexPatterns.IntVec3);
        Assert.That(isMatch);
    }
}