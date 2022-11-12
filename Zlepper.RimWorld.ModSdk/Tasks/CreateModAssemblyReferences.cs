using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Zlepper.RimWorld.ModSdk.Utilities;

namespace Zlepper.RimWorld.ModSdk.Tasks;

public class CreateModAssemblyReferences : Task
{
    [Required] public ITaskItem[] ModDependencies { get; set; } = null!;

    [Required] public string CurrentRimWorldVersion { get; set; } = null!;
    [Required] public string SteamModContentFolder { get; set; } = null!;


    [Output] public ITaskItem[] MatchedReferences { get; set; } = null!;

    public override bool Execute()
    {
        var modLocator = new RimWorldModLocator(SteamModContentFolder, Log);


        var rimWorldVersion = Version.Parse(CurrentRimWorldVersion);

        MatchedReferences = ModDependencies
            .SelectMany(modDependency =>
            {
                var steamModPackageId = modDependency.GetMetadata("Identity");
                var mainDllName = modDependency.GetMetadata("MainDllName");
                var modMetadata = modLocator.FindMod(steamModPackageId);

                if (modMetadata == null)
                {
                    Log.LogError(
                        $"Could not find mod {steamModPackageId}. Do you actually subscribe to the mod on steam? If yes, is the {nameof(SteamModContentFolder)} configured correctly? Right now it's pointing to '{SteamModContentFolder}'.");
                    return Enumerable.Empty<ITaskItem>();
                }

                var assemblyPaths = modMetadata.GetModAssemblyPath(rimWorldVersion, Log).ToList();
                if (assemblyPaths.Count == 0)
                {
                    Log.LogError($"Could not find assembly for mod {steamModPackageId}");
                    return Enumerable.Empty<ITaskItem>();
                }

                if (!string.IsNullOrWhiteSpace(mainDllName))
                {
                    if (!mainDllName.EndsWith(".dll"))
                    {
                        mainDllName += ".dll";
                    }

                    var focusedOptions = assemblyPaths
                        .Where(p => Path.GetFileName(p).Equals(mainDllName, StringComparison.InvariantCultureIgnoreCase))
                        .ToList();
                    if (focusedOptions.Count == 0)
                    {
                        var availableDllNames = assemblyPaths.Select(Path.GetFileName);
                        Log.LogError($"Could not find dll with name '{mainDllName}' for mod '{steamModPackageId}', please check that you have the correct dll name. Available dlls: {string.Join(", ", availableDllNames)}");
                    }

                    assemblyPaths = focusedOptions;
                }

                return assemblyPaths.Select(assemblyPath =>
                {
                    var taskItemName = steamModPackageId;
                    if (assemblyPaths.Count > 1)
                    {
                        taskItemName += "_" + Path.GetFileNameWithoutExtension(assemblyPath);
                    }
                    
                    return new TaskItem(taskItemName,
                        new Dictionary<string, string>
                        {
                            {"HintPath", assemblyPath},
                            {"Private", "false"}
                        });
                });
            })
            .ToArray();

        return !Log.HasLoggedErrors;
    }
}