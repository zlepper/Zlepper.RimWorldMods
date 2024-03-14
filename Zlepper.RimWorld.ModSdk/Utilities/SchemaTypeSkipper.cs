using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Zlepper.RimWorld.ModSdk.Utilities;

public class SchemaTypeSkipper
{
    private readonly DefContext _defContext;
    private readonly TimeMeasuringTool _timeMeasuringTool;
    
    private static readonly string[] AllowedGenericTypes = new[]
    {
        typeof(List<>).FullName,
        typeof(Dictionary<,>).FullName,
        typeof(Nullable<>).FullName,
        "RimWorld.QuestGen.SlateRef`1"
    };

    public SchemaTypeSkipper(DefContext defContext, TimeMeasuringTool timeMeasuringTool)
    {
        _defContext = defContext;
        _timeMeasuringTool = timeMeasuringTool;
    }
    
    private readonly Dictionary<object, bool> _skipCache = new();

    public bool ShouldSkip(Type type)
    {
        if (_skipCache.TryGetValue(type, out var cachedResult))
        {
            return cachedResult;
        }

        return _skipCache[type] = InternalShouldSkip(type);
    }
    
    private bool InternalShouldSkip(Type type)
    {
        try
        {
            if (type.IsGenericParameter)
            {
                return true;
            }

            if (type.IsConstructedGenericType)
            {
                var genericTypeDefinitionFullName = type.GetGenericTypeDefinition().FullName;

                var isAllowedGenericType = AllowedGenericTypes.Contains(genericTypeDefinitionFullName);

                return !isAllowedGenericType || type.GetGenericArguments().Any(inner =>
                    ShouldSkip(inner) || inner.IsConstructedGenericType &&
                    inner.GetGenericTypeDefinition().FullName != typeof(Nullable<>).FullName);
            }

            var isEntity = _defContext.GetBaseTypes(type).Any(t => t.FullName == "Verse.Entity");
            var isMap = type.FullName == "Verse.Map";
            var isIdeo = type.FullName == "RimWorld.Ideo";
            var isFaction = type.FullName == "RimWorld.Faction";
            var isSteamRelated = type.Namespace?.StartsWith("Verse.Steam") ?? false;
            var isDisposable = type.GetInterfaces().Any(i => i.FullName == typeof(IDisposable).FullName);
            var isClosedFunction = IsFunctionName(type.FullName!);
            var isComplexSketch = type.FullName == "RimWorld.ComplexSketch";
            var isWorldObject = _defContext.GetBaseTypes(type).Any(t => t.FullName == "RimWorld.Planet.WorldObject");
            var isWorldComponent =
                _defContext.GetBaseTypes(type).Any(t => t.FullName == "RimWorld.Planet.WorldComponent");
            var isMapComponent = _defContext.GetBaseTypes(type).Any(t => t.FullName == "Verse.MapComponent");
            var isObject = type.FullName == typeof(object).FullName;
            var isUnityType = type.FullName!.StartsWith("UnityEngine.");
            var isGenStepParams = type.FullName == "Verse.GenStepParams";


            return isEntity || isMap || isIdeo || isSteamRelated || isFaction || isDisposable || isClosedFunction ||
                   isComplexSketch || isWorldObject || isObject || isUnityType || isGenStepParams || isWorldComponent ||
                   isMapComponent;
        }
        catch (FileNotFoundException)
        {
            return true;
        }
    }

    private static bool IsFunctionName(string genericTypeDefinitionFullName)
    {
        return genericTypeDefinitionFullName.StartsWith("System.Predicate") ||
               genericTypeDefinitionFullName.StartsWith("System.Func") ||
               genericTypeDefinitionFullName.StartsWith("System.Action") ||
               genericTypeDefinitionFullName.StartsWith("System.Delegate");
    }
    
    public bool ShouldSkip(FieldInfo field)
    {
        if (_skipCache.TryGetValue(field, out var cachedResult))
        {
            return cachedResult;
        }
        
        return _skipCache[field] = InternalShouldSkip(field);
    }

    private bool InternalShouldSkip(FieldInfo field)
    {
        
        if (ShouldSkip(field.FieldType))
        {
            return true;
        }

        var isInternalField = field.IsPrivate && field.Name.EndsWith("Int");
        var isTmp = field.IsPrivate && field.Name.StartsWith("tmp", StringComparison.InvariantCultureIgnoreCase);

        var invalidName = field.Name.StartsWith("<");

        var attributes = field.GetCustomAttributesData();
        if (attributes.Count > 0)
        {
            foreach (var attribute in attributes)
            {
                var attributeTypeName = attribute.AttributeType.Name;
                var attributeTypeNamespace = attribute.AttributeType.Namespace;

                if (attributeTypeNamespace != _defContext.RootDefTypeClassNamespace)
                {
                    continue;
                }

                switch (attributeTypeName)
                {
                    case "UnsavedAttribute":
                        return false;
                }
            }
        }

        return isInternalField || field.IsSpecialName || field.IsLiteral || invalidName || isTmp;
    }

}