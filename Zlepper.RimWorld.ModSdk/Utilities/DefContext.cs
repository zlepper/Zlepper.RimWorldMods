using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace Zlepper.RimWorld.ModSdk.Utilities;

public class DefContext
{
    public readonly string RootDefTypeClassName = "Def";
    public readonly string RootDefTypeClassNamespace = "Verse";
    
    
    public DefContext(Type rootDefType)
    {
        RootDefTypeClassName = rootDefType.Name;
        RootDefTypeClassNamespace = rootDefType.Namespace!;
    }

    public DefContext()
    {
    }
    
    
    public bool IsDef(Type type)
    {
        try
        {
            if (!type.IsClass || type.IsAbstract) return false;

            if (IsRootDefClass(type))
            {
                return true;
            }

            foreach (var t in GetBaseTypes(type))
            {
                if (IsRootDefClass(t))
                {
                    return true;
                }
            }

            return false;
        }
        catch(FileNotFoundException)
        {
            return false;
        }
    }

    public bool IsRootDefClass(Type type)
    {
        return type.Namespace == RootDefTypeClassNamespace && type.Name == RootDefTypeClassName;
    }

    private static readonly string _objectFullname = typeof(object).FullName;

    public IEnumerable<Type> GetBaseTypes(Type type)
    {
        try
        {
            var baseTypes = GetBaseTypesCore(type).ToList();
            return baseTypes;
        }
        catch (FileNotFoundException)
        {
            return Array.Empty<Type>();
        }
    }
    
    private static IEnumerable<Type> GetBaseTypesCore(Type type)
    {
        var next = type;
        while (next.BaseType != null && next.BaseType.FullName != _objectFullname)
        {
            yield return next.BaseType;
            next = next.BaseType;
        }
    }

    public string GetDefElementName(Type defType)
    {
        return defType.Namespace == RootDefTypeClassNamespace || defType.Namespace == "RimWorld"
            ? defType.Name
            : defType.FullName!.Replace("+", ".");
    }
}