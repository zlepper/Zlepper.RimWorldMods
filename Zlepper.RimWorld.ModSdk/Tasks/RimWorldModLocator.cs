using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Zlepper.RimWorld.ModSdk.Models;
using Zlepper.RimWorld.ModSdk.Utilities;

namespace Zlepper.RimWorld.ModSdk.Tasks;

public class RimWorldModLocator
{
    private readonly List<SteamMod> _mods = new();

    public RimWorldModLocator(string steamModContentFolder, string rimWorldVersion, TaskLoggingHelper log)
    {
        foreach (var modDirectory in Directory.EnumerateDirectories(steamModContentFolder))
        {
            var loadFoldersXmlFilePath = Path.Combine(modDirectory, "LoadFolders.xml");
            if (File.Exists(loadFoldersXmlFilePath))
            {
                using var stream = File.Open(loadFoldersXmlFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                try
                {
                    var folders = XmlUtilities.DeserializeFromXml<RimWorldXmlList<string>>(stream, "loadFolders");
                }
                catch (Exception e)
                {
                    log.LogMessage(MessageImportance.Low, $"Failed to parse 'LoadFolders.xml' file at path {loadFoldersXmlFilePath}: {e}");
                }
            }

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

            var steamFileId = Path.GetFileName(modDirectory)!;

            var authors = modMetaData.Authors.ListItems.Count > 0
                ? modMetaData.Authors.ListItems
                : new List<string> {modMetaData.Author!};

            var packageId = modMetaData.PackageId;
            var name = modMetaData.Name;
            var url = modMetaData.Url;

            var mod = new SteamMod(steamFileId, authors, packageId, name, url);
            _mods.Add(mod);
        }
    }

    public SteamMod? FindMod(string steamModPackageId)
    {
        return _mods.FirstOrDefault(m => m.PackageId == steamModPackageId);
    }
}

public record SteamMod(string FileId, List<string> Authors, string PackageId, string Name, string? Url);