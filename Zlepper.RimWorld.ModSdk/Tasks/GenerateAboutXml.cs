using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Zlepper.RimWorld.ModSdk.Tasks;

public class GenerateAboutXml : Task
{
    [Required] public string ModName { get; set; } = null!;

    [Required] public string ModPackageId { get; set; } = null!;

    [Required] public string ModAuthor { get; set; } = null!;
    
    public override bool Execute()
    {
        var about = new ModMetaData()
        {
            Name = ModName,
            PackageId = ModPackageId,
            Author = ModAuthor
        };
        
        var ns = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
        var serializer = new XmlSerializer(typeof(ModMetaData), "");

        EnsureDirectory(Path.GetDirectoryName(AboutXmlFileName)!);
        
        using (var stream = File.Open(AboutXmlFileName, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            using var xmlWriter = XmlWriter.Create(stream, new XmlWriterSettings()
            {
                Encoding = Encoding.UTF8,
                Indent = true,
            });
            serializer.Serialize(xmlWriter, about, ns);
        }

        
        
        return !Log.HasLoggedErrors;
    }

    private static void EnsureDirectory(string directoryName)
    {
        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }
    }

    [Output]
    public string AboutXmlFileName { get; set; } = "About/AboutTest.xml";
}

public class ModMetaData
{
    [XmlElement("name")]
    public string Name { get; set; } = null!;
    [XmlElement("author")]
    public string Author { get; set; } = null!;
    public string? Url { get; set; }
    [XmlElement("packageId")]
    public string PackageId { get; set; } = null!;
}