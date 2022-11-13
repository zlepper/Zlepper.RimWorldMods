using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Zlepper.RimWorld.ModSdk.Utilities;

namespace Zlepper.RimWorld.ModSdk.Tasks;

public class GenerateXdsFilesForDefs : Task
{
    public ITaskItem[] References { get; set; } = null!;
    
    [Output] public string XmlSchemaFileName { get; set; } = "Defs/DefsSchema.xsd";
    
    private readonly DefToSchemaConverter _converter = new();

    public override bool Execute()
    {
        try
        {
            using var context = GetReferencedAssemblies();

            var assemblies = context.GetAssemblies().ToList();

            var defs = GetDefTypes(assemblies);

            Log.LogMessage(MessageImportance.High,
                $"Found {defs.Count} def classes across {assemblies.Count} assemblies.");


            var schema = _converter.CreateSchema(defs);

            XmlUtilities.WriteSchemaToFile(XmlSchemaFileName, schema);
        }
        catch (Exception e)
        {
            Log.LogErrorFromException(e, true, true, null);
        }

        return !Log.HasLoggedErrors;
    }

    private List<Type> GetDefTypes(IEnumerable<Assembly> assemblies)
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
            .Where(_converter.IsDef)
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