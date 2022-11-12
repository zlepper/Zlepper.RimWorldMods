using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Zlepper.RimWorld.ModSdk.Models;

namespace Zlepper.RimWorld.ModSdk.Utilities;

public class RimWorldModLocator
{
    private readonly List<SteamMod> _mods = new();

    public RimWorldModLocator(string steamModContentFolder, TaskLoggingHelper log)
    {
        foreach (var modDirectory in Directory.EnumerateDirectories(steamModContentFolder))
        {
            
            ModMetaData modMetaData;
            var aboutXmlFilePath = Path.Combine(modDirectory, "About", "About.xml");
            if (File.Exists(aboutXmlFilePath))
            {
                using var stream = File.Open(aboutXmlFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                try
                {
                    modMetaData = XmlUtilities.DeserializeFromXml<ModMetaData>(stream);
                }
                catch (Exception e)
                {
                    log.LogMessage(MessageImportance.Low, $"Failed to parse 'About.xml' file at path {aboutXmlFilePath}: {e}");
                    continue;
                }
            }
            else
            {
                continue;
            }

            var packageId = modMetaData.PackageId;

            var steamFileId = Path.GetFileName(modDirectory)!;

            var authors = modMetaData.Authors.ListItems.Count > 0
                ? modMetaData.Authors.ListItems
                : new List<string> {modMetaData.Author!};

            var name = modMetaData.Name;
            var url = modMetaData.Url;

            var mod = new SteamMod(steamFileId, authors, packageId, name, url, modDirectory);
            _mods.Add(mod);
        }
    }

    public SteamMod? FindMod(string steamModPackageId)
    {
        return _mods.FirstOrDefault(m => m.PackageId.Equals(steamModPackageId, StringComparison.InvariantCultureIgnoreCase));
    }

}

public record SteamMod(string FileId, List<string> Authors, string PackageId, string Name, string? Url,
    string modDirectory)
{
    public IEnumerable<string> GetModAssemblyPath(Version version, TaskLoggingHelper log)
    {
        var defaultLoadFolders = new List<string>
        {
            "/",
            "Common",
            version.ToString()
        };

        var loadFoldersXmlFilePath = Path.Combine(modDirectory, "LoadFolders.xml");
        if (File.Exists(loadFoldersXmlFilePath))
        {
            using var stream = File.Open(loadFoldersXmlFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            try
            {
                var versionFolders = XmlUtilities.DeserializeFromXml<VersionedXmlList<string>>(stream, "loadFolders");

                if (versionFolders.TryGet(version, out var folders))
                {
                    return FindAssemblyForMod(folders, modDirectory, PackageId, log);
                }

                log.LogWarning($"Could not find version {version} in path: {loadFoldersXmlFilePath}");
            }
            catch (Exception e)
            {
                log.LogWarning($"Failed to parse 'LoadFolders.xml' file at path {loadFoldersXmlFilePath}: {e}");
            }
        }
        else
        {
            return FindAssemblyForMod(defaultLoadFolders, modDirectory, PackageId, log);
        }

        return Enumerable.Empty<string>();
    }
    
    private IEnumerable<string> FindAssemblyForMod(List<string> foldersToTry, string modRootDir, string modPackageId, TaskLoggingHelper log)
    {
        for (var i = foldersToTry.Count - 1; i >= 0; i--)
        {
            var folder = foldersToTry[i];
            var assemblyPath = Path.Combine(modRootDir, folder, "Assemblies");

            if (!Directory.Exists(assemblyPath))
            {
                continue;
            }

            var dllFiles = Directory.EnumerateFiles(assemblyPath, "*.dll").ToList();

            if (dllFiles.Count > 0)
            {
                return dllFiles;
            }
        }

        return Enumerable.Empty<string>();
    }
}