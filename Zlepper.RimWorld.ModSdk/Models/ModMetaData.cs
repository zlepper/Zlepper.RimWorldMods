using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.Models;

public class ModMetaData
{
    [XmlElement("name")] public string Name { get; set; } = null!;
    [XmlElement("author")] public string? Author { get; set; }
    [XmlElement("authors")] public RimWorldXmlList<string> Authors { get; set; } = new();
    public bool ShouldSerializeAuthors() => Authors.ListItems.Count > 0;

    [XmlElement("url")] public string? Url { get; set; }
    [XmlElement("packageId")] public string PackageId { get; set; } = null!;

    [XmlElement("description")] public string Description { get; set; } = "";

    [XmlElement("supportedVersions")] public RimWorldXmlList<string> SupportedVersions { get; set; } = new();

    [XmlElement("modDependencies")] public RimWorldXmlList<ModDependencyItem> ModDependencies { get; set; } = new();
    public bool ShouldSerializeModDependencies() => ModDependencies.ListItems.Count > 0;

    [XmlElement("loadAfter")] public RimWorldXmlList<string> LoadAfter { get; set; } = new();
    public bool ShouldSerializeLoadAfter() => LoadAfter.ListItems.Count > 0;

    [XmlElement("loadBefore")] public RimWorldXmlList<string> LoadBefore { get; set; } = new();
    public bool ShouldSerializeLoadBefore() => LoadBefore.ListItems.Count > 0;

    public void AddModDependency(ModDependencyItem dep)
    {
        ModDependencies.Add(dep);
        LoadAfter.Add(dep.PackageId);
    }
}