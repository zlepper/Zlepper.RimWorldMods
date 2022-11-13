using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Zlepper.RimWorld.ModSdk.Tasks;

public class GenerateXdsFilesForDefs : Task
{
    public ITaskItem[] References { get; set; } = null!;


    public override bool Execute()
    {
        var assemblies = GetReferencedAssemblies();

        var defs = GetDefTypes(assemblies);

        Log.LogMessage(MessageImportance.High, $"Found {defs.Count} def classes across {assemblies.Count} assemblies.: {string.Join("\n", defs.Select(t =>t.Name))}");
        
        return !Log.HasLoggedErrors;
    }
    
    private List<Type> GetDefTypes(IEnumerable<Assembly> assemblies)
    {
        return assemblies.SelectMany(a =>
        {
            try
            {
                return a.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where(t => t != null);
            }
            catch (Exception e)
            {
                Log.LogError("Failed to get types in assembly " + a.FullName + ". " + e);
                return Enumerable.Empty<Type>();
            }
        }).Where(IsDef).ToList();
    }

    private List<Assembly> GetReferencedAssemblies()
    {
        var assemblies = new List<Assembly>();


        foreach (var reference in References)
        {
            try
            {
                var fullPath = reference.GetMetadata("FullPath");
                var hintPath = reference.GetMetadata("HintPath");

                string assemblyPath;
                if (File.Exists(fullPath))
                    assemblyPath = fullPath;
                else if (File.Exists(hintPath))
                    assemblyPath = hintPath;
                else
                    continue;


                var assemblyBytes = File.ReadAllBytes(assemblyPath);
                Assembly assembly;
                try
                {
                    assembly = Assembly.ReflectionOnlyLoad(assemblyBytes);
                }
                catch (Exception e)
                {
                    Log.LogWarning($"Failed to load assembly: {assemblyPath}. {e}");
                    continue;
                }

                assemblies.Add(assembly);
            }
            catch (Exception e)
            {
                Log.LogError("Something went wrong when generating xsd files for defs. " + e);
            }
        }

        return assemblies;
    }

    private bool IsDef(Type type)
    {
        try
        {
            return type.IsClass && !type.IsAbstract &&
                   GetBaseTypes(type).Any(t => t is {Name: "Def", Namespace: "Verse"});
        }
        catch (Exception e)
        {
            Log.LogError($"Failed to check if type {type.Name} is def subclass. " + e);
            return false;
        }
    }

    private static IEnumerable<Type> GetBaseTypes(Type type)
    {
        if (type.BaseType == null || type.BaseType == typeof(object))
        {
            return Enumerable.Empty<Type>();
        }

        return Enumerable.Repeat(type.BaseType, 1).Concat(GetBaseTypes(type.BaseType!));
    }
}