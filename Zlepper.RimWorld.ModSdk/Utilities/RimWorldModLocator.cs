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