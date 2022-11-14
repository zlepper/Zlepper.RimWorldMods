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
        var folders = GetModFoldersToTry(version, log);

        return FindAssemblyForMod(folders);
    }

    public IEnumerable<string> FindModDefFiles(Version version, TaskLoggingHelper log)
    {
        var folders = GetModFoldersToTry(version, log);
        
        return FindModDefFiles(folders);
    }


    private IEnumerable<string> FindAssemblyForMod(IReadOnlyList<string> foldersToTry)
    {
        for (var i = foldersToTry.Count - 1; i >= 0; i--)
        {
            var folder = foldersToTry[i];
            var assemblyPath = Path.Combine(modDirectory, folder, "Assemblies");

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
    
    
    private IEnumerable<string> FindModDefFiles(IReadOnlyList<string> folders)
    {
        for (var i = folders.Count - 1; i >= 0; i--)
        {
            var folder = folders[i];
            var defsPath = Path.Combine(modDirectory, folder, "Defs");

            if (!Directory.Exists(defsPath))
            {
                continue;
            }
            
            var defFiles = Directory.EnumerateFiles(defsPath, "*.xml", SearchOption.AllDirectories).ToList();

            if (defFiles.Count > 0)
            {
                return defFiles;
            }
        }
        
        return Enumerable.Empty<string>();
    }
    
    
    private IReadOnlyList<string>? _cachedModFolders;

    private IReadOnlyList<string> GetModFoldersToTry(Version version, TaskLoggingHelper log)
    {
        if (_cachedModFolders != null)
        {
            return _cachedModFolders;
        }
        
        return _cachedModFolders = doFind();
        
        IReadOnlyList<string> doFind()
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
                    var versionFolders =
                        XmlUtilities.DeserializeFromXml<VersionedXmlList<string>>(stream, "loadFolders");

                    if (versionFolders.TryGet(version, out var folders))
                    {
                        return folders;
                    }

                    log.LogWarning($"Could not find version {version} in path: {loadFoldersXmlFilePath}");
                }
                catch (Exception e)
                {
                    log.LogWarning($"Failed to parse 'LoadFolders.xml' file at path {loadFoldersXmlFilePath}: {e}");
                }
            }

            return defaultLoadFolders;
        }
    }

}