using NUnit.Framework;
using Zlepper.RimWorld.ModSdk.Utilities;

namespace Zlepper.RimWorld.ModSdk.Tests.Utilities;

[TestFixture]
public class FileUtilitiesTests
{
    [TestCase("C:/dir/mydefs.xml", "C:/dir/schema.xsd", "schema.xsd")]
    [TestCase("C:/dir/sub/mydefs.xml", "C:/dir/schema.xsd", "../schema.xsd")]
    public void CalculatesRelativePath(string from, string to, string expected)
    {
        var actual = FileUtilities.GetRelativePath(from, to);
        Assert.That(actual, Is.EqualTo(expected));
    }
}