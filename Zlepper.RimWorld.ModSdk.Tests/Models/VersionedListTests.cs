using System;
using Zlepper.RimWorld.ModSdk.Models;
using Zlepper.RimWorld.ModSdk.Utilities;

namespace Zlepper.RimWorld.ModSdk.Tests.Models;

[TestFixture]
public class VersionedXmlListTests
{
    [Test]
    public void SerializesAsIntended()
    {
        var folders = new VersionedXmlList<string>();
        
        folders.Add(new Version(1, 1), "/");
        folders.Add(new Version(1, 1), "1.1");
        folders.Add(new Version(1, 2), "/");
        folders.Add(new Version(1, 2), "1.2");

        var xml = XmlUtilities.SerializeToXml(folders, "loadFolders");
        
        Assert.That(xml, Is.EqualTo("<loadFolders>\n  <v1.1>\n    <li>/</li>\n    <li>1.1</li>\n  </v1.1>\n  <v1.2>\n    <li>/</li>\n    <li>1.2</li>\n  </v1.2>\n</loadFolders>"));
    }
    
    [Test]
    public void DeserializesAsIntended()
    {
        var xml =
            "<loadFolders>\n  <v1.1>\n    <li>/</li>\n    <li>1.1</li>\n  </v1.1>\n  <v1.2>\n    <li>/</li>\n    <li>1.2</li>\n  </v1.2>\n</loadFolders>";
        
        
        
        
        
        var folders = new VersionedXmlList<string>();
        
        folders.Add(new Version(1, 1), "/");
        folders.Add(new Version(1, 1), "1.1");
        folders.Add(new Version(1, 2), "/");
        folders.Add(new Version(1, 2), "1.2");

        var result = XmlUtilities.DeserializeFromXml<VersionedXmlList<string>>(xml, "loadFolders");
        
        Assert.That(result, Is.EqualTo(folders));
    }
}