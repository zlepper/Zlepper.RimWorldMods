using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.Models;

public class ModDependencyItem
{
    [XmlElement("packageId")] public string PackageId { get; set; } = null!;

    [XmlElement("displayName")] public string DisplayName { get; set; } = null!;

    [XmlElement("steamWorkshopUrl")] public string SteamWorkshopUrl { get; set; } = null!;

    [XmlElement("downloadUrl")] public string? DownloadUrl { get; set; }
}