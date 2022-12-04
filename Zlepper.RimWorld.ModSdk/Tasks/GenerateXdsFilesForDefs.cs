using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Zlepper.RimWorld.ModSdk.Utilities;

namespace Zlepper.RimWorld.ModSdk.Tasks;

public class GenerateXdsFilesForDefs : Task
{
    [Required] public ITaskItem[] References { get; set; } = null!;

    [Required] public ITaskItem[] RimWorldDefFiles { get; set; } = null!;

    public string XmlSchemaFileName { get; set; } = "Defs/DefsSchema.xsd";

    private readonly DefContext _defContext = new();

    public override bool Execute()
    {
        try
        {
            using var context = GetReferencedAssemblies();

            var assemblies = context.GetAssemblies().ToList();

            var allTypes = GetAllTypes(assemblies);

            var currentlyDefinedDefs = GetCurrentDefinedDefs(allTypes);

            var converter = new DefToSchemaConverter(_defContext, currentlyDefinedDefs);

            var schema = converter.CreateSchema(allTypes);


            XmlUtilities.WriteSchemaToFile(XmlSchemaFileName, schema);
        }
        catch (Exception e)
        {
            Log.LogErrorFromException(e, true, true, null);
        }

        return !Log.HasLoggedErrors;
    }

    private IReadOnlyDictionary<Type, List<string>> GetCurrentDefinedDefs(List<Type> defClasses)
    {
        var files = RimWorldDefFiles
            .Select(item => item.GetMetadata("FullPath"))
            .Where(p => p != null)
            .Where(File.Exists);


        var defReader = new DefReader(defClasses, _defContext);

        return defReader.ReadAllDefFiles(files, Log);
    }

    private List<Type> GetAllTypes(IEnumerable<Assembly> assemblies)
    {
        return assemblies
            .SelectMany(a =>
            {
                try
                {
                    return a.GetTypes();
                }
                catch (ReflectionTypeLoadException e)
                {
                    Log.LogError("Failed to get some types in assembly " + a.FullName + ". " + e);
                    return e.Types.Where(t => t != null);
                }
                catch (Exception e)
                {
                    Log.LogError("Failed to get types in assembly " + a.FullName + ". " + e);
                    return Enumerable.Empty<Type>();
                }
            })
            .ToList();
    }

    private MetadataLoadContext GetReferencedAssemblies()
    {
        var coreAssembly = typeof(string).Assembly;
        var assemblyPaths = References
            .Select(reference =>
            {
                var fullPath = reference.GetMetadata("FullPath");
                if (File.Exists(fullPath))
                    return fullPath;

                var hintPath = reference.GetMetadata("HintPath");
                if (File.Exists(hintPath))
                    return hintPath;

                return null;
            })
            .OfType<string>()
            .Prepend(coreAssembly.Location)
            .ToList();

        var resolver = new PathAssemblyResolver(assemblyPaths);

        var context = new MetadataLoadContext(resolver, coreAssembly.GetName().Name);

        foreach (var assemblyPath in assemblyPaths)
        {
            var assemblyBytes = File.ReadAllBytes(assemblyPath);
            try
            {
                context.LoadFromByteArray(assemblyBytes);
            }
            catch (Exception e)
            {
                Log.LogWarning($"Failed to load assembly: {assemblyPath}. {e}");
            }
        }


        return context;
    }
}