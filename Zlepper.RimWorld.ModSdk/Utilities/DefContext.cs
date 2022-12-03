using System;
using System.Collections.Generic;
using System.Linq;

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
        return type.IsClass && !type.IsAbstract &&
               GetBaseTypes(type).Append(type).Any(IsRootDefClass);
    }

    public bool IsRootDefClass(Type type)
    {
        return type.Namespace == RootDefTypeClassNamespace && type.Name == RootDefTypeClassName;
    }

    public IEnumerable<Type> GetBaseTypes(Type type)
    {
        if (type.BaseType == null || type.BaseType.FullName == typeof(object).FullName)
        {
            return Enumerable.Empty<Type>();
        }

        return Enumerable.Repeat(type.BaseType, 1).Concat(GetBaseTypes(type.BaseType!));
    }

    public string GetDefElementName(Type defType)
    {
        return defType.Namespace == RootDefTypeClassNamespace || defType.Namespace == "RimWorld"
            ? defType.Name
            : defType.FullName!.Replace("+", ".");
    }
}