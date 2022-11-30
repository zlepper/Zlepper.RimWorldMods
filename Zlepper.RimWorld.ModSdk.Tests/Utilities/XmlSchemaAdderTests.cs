using System.Xml.Linq;
using Zlepper.RimWorld.ModSdk.Utilities;

namespace Zlepper.RimWorld.ModSdk.Tests.Utilities;

[TestFixture]
public class XmlSchemaAdderTests
{
    [Test]
    public async Task AddToDefRoot()
    {
        
        var defContent = await File.ReadAllTextAsync("Utilities/TestFiles/DefWithoutNs.xml");

        var doc = XDocument.Parse(defContent);
        XmlSchemaAdder.AddRimWorldNamespace(doc, "DefsSchema.xsd");


        var result = doc.ToString();
        await VerifyXml(result).UseDirectory("../Snapshots");
    }
}