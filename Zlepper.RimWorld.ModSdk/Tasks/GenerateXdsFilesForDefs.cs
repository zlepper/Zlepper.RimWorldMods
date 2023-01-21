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
    [Required] public ITaskItem[] RimWorldCompilationResult { get; set; } = null!;

    [Required] public ITaskItem[] RimWorldDefFiles { get; set; } = null!;

    public string XmlSchemaFileName { get; set; } = "Defs/DefsSchema.xsd";

    private readonly DefContext _defContext = new();

    public override bool Execute()
    {
        try
        {
            NextMeasure();
            using var context = GetReferencedAssemblies();
            NextMeasure("Get referenced assemblies");
            
            var assemblies = context.GetAssemblies().ToList();
            NextMeasure("Get assemblies");

            var allTypes = GetAllTypes(assemblies);
            NextMeasure("Get all types");

            var currentlyDefinedDefs = GetCurrentDefinedDefs(allTypes);
            NextMeasure("Get currently defined defs");

            var schemaMeasures = new TimeMeasuringTool();
            var converter = new DefToSchemaConverter(_defContext, currentlyDefinedDefs, schemaMeasures);

            var schema = converter.CreateSchema(allTypes);
            NextMeasure("Create schema");
            schemaMeasures.Dump(Log);

            XmlUtilities.WriteSchemaToFile(XmlSchemaFileName, schema);
            NextMeasure("Write schema to file");
        }
        catch (Exception e)
        {
            Log.LogErrorFromException(e, true, true, null);
        }

        return !Log.HasLoggedErrors;
    }

    private readonly Stopwatch _stopwatch = new();
    
    private void NextMeasure(string finishedText = "")
    {
        if (_stopwatch.IsRunning)
        {
            Log.LogMessage( "Finished {0} in {1}ms", finishedText, _stopwatch.ElapsedMilliseconds);
        }
        _stopwatch.Restart();
    }

    private IReadOnlyDictionary<Type, List<string>> GetCurrentDefinedDefs(List<Type> defClasses)
    {
        var files = RimWorldDefFiles
            .Select(item => item.GetMetadata("FullPath"))
            .Where(p => p != null)
            .Where(File.Exists);


        var defReader = new DefReader(defClasses, _defContext);

        var d = defReader.ReadAllDefFiles(files, Log);

        defReader.Dump(Log);
        return d;
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
            .Concat(RimWorldCompilationResult)
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
            .Where(s => s.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase))
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