using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Utilities;
using Zlepper.RimWorld.ModSdk.Utilities;

namespace Zlepper.RimWorld.ModSdk.Models;

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
                    return FindAssemblyForMod(folders, modDirectory);
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
            return FindAssemblyForMod(defaultLoadFolders, modDirectory);
        }

        return Enumerable.Empty<string>();
    }
    
    private static IEnumerable<string> FindAssemblyForMod(List<string> foldersToTry, string modRootDir)
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