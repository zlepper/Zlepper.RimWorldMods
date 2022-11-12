using System;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Zlepper.RimWorld.ModSdk.Models;
using Zlepper.RimWorld.ModSdk.Utilities;

namespace Zlepper.RimWorld.ModSdk.Tasks;

public class GenerateAboutXml : Task
{
    [Required] public string ModName { get; set; } = null!;

    [Required] public string ModPackageId { get; set; } = null!;

    [Required] public string ModAuthors { get; set; } = null!;

    [Required] public string SteamModContentFolder { get; set; } = null!;

    [Required] public string Description { get; set; } = null!;

    [Required] public string CurrentRimWorldVersion { get; set; } = null!;

    [Required] public string ModOutputFolder { get; set; } = null!;
    
    public string? ModUrl { get; set; }

    [Required] public ITaskItem[] ModDependencies { get; set; } = null!;

    [Required] public ITaskItem[] LoadBeforeMods { get; set; } = null!;
    [Required] public ITaskItem[] LoadAfterMods { get; set; } = null!;
    [Required] public ITaskItem[] IncompatibleWithMods { get; set; } = null!;
    
    [Required] public ITaskItem[] ProjectReferences { get; set; } = null!;

    [Output] public string AboutXmlFileName { get; set; } = "About/About.xml";

    public override bool Execute()
    {
        var about = new ModMetaData()
        {
            Name = ModName,
            PackageId = ModPackageId,
            Description = DescriptionTrimmer.TrimDescription(Description),
            Url = ModUrl,
        };
        
        AddModAuthors(about);

        AddSupportedVersions(about);

        AddDependencies(about);

        XmlUtilities.SerializeToXml(AboutXmlFileName, about);

        return !Log.HasLoggedErrors;
    }

    private void AddModAuthors(ModMetaData about)
    {
        var modAuthors = ModAuthors.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
        if (modAuthors.Length > 1)
        {
            about.Authors = modAuthors.ToRimWorldXmlList();
        }
        else
        {
            about.Author = modAuthors[0];
        }
    }

    private void AddSupportedVersions(ModMetaData about)
    {
        about.SupportedVersions.Add(CurrentRimWorldVersion);

        if (Directory.Exists(ModOutputFolder))
        {
            var versions = Directory.EnumerateDirectories(ModOutputFolder)
                .Select(Path.GetFileName)
                .Where(n => Version.TryParse(n, out _))
                .ToList();
            about.SupportedVersions.AddMissing(versions);
        }

        about.SupportedVersions.ListItems.Sort();
    }

    private void AddDependencies(ModMetaData about)
    {
        var modLocator = new RimWorldModLocator(SteamModContentFolder, Log);
        
        foreach (var modDependency in ModDependencies)
        {
            var steamModPackageId = modDependency.GetMetadata("Identity");
            var modMetadata = modLocator.FindMod(steamModPackageId);

            if (modMetadata == null)
            {
                Log.LogError($"Could not find mod {steamModPackageId}. Do you actually subscribe to the mod on steam? If yes, is the {nameof(SteamModContentFolder)} configured correctly? Right now it's pointing to '{SteamModContentFolder}'.");
                continue;
            }

            var dep = new ModDependencyItem()
            {
                PackageId = modMetadata.PackageId,
                DisplayName = modMetadata.Name,
                SteamWorkshopUrl = $"steam://url/CommunityFilePage/{modMetadata.FileId}",
                DownloadUrl = modMetadata.Url,
            };
            about.AddModDependency(dep);
        }
        
        foreach (var projectReference in ProjectReferences)
        {
            var fullPath = projectReference.GetMetadata("Fullpath");

            var projectFolder = Path.GetDirectoryName(fullPath)!;
            var aboutXmlPath = Path.Combine(projectFolder, "About", "About.xml");
            if (!File.Exists(aboutXmlPath))
            {
                continue;
            }

            var publishedFileIdPath = Path.Combine(projectFolder, "About", "PublishedFileId.txt");
            var publishedFileId = "";
            if (File.Exists(publishedFileIdPath))
            {
                publishedFileId = File.ReadAllText(publishedFileIdPath).Trim();
            }

            var projectMetaData = XmlUtilities.DeserializeFromFile<ModMetaData>(aboutXmlPath);

            var dep = new ModDependencyItem()
            {
                PackageId = projectMetaData.PackageId,
                DisplayName = projectMetaData.Name,
                DownloadUrl = projectMetaData.Url,
            };

            if (!string.IsNullOrWhiteSpace(publishedFileId))
            {
                dep.SteamWorkshopUrl = $"steam://url/CommunityFilePage/{publishedFileId}";
            }
            
            about.AddModDependency(dep);
        }

        foreach (var loadBeforeMod in LoadBeforeMods)
        {
            var packageId = loadBeforeMod.GetMetadata("Identity");
            
            about.LoadBefore.Add(packageId);
        }
        
        foreach (var loadAfterMod in LoadAfterMods)
        {
            var packageId = loadAfterMod.GetMetadata("Identity");
            
            about.LoadAfter.Add(packageId);
        }
        
        foreach (var incompatibleWithMod in IncompatibleWithMods)
        {
            var packageId = incompatibleWithMod.GetMetadata("Identity");
            
            about.IncompatibleWith.Add(packageId);
        }
    }

    private ModMetaData? ReadAboutXmlFile(string filePath)
    {
        try
        {
            return XmlUtilities.DeserializeFromFile<ModMetaData>(filePath);
        }
        catch (Exception e)
        {
            Log.LogError($"Failed to read About.xml file at '{filePath}': {e}");
            return null;
        }
    }
}