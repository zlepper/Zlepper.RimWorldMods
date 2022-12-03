using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Zlepper.RimWorld.ModSdk.XsdOneOne;

namespace Zlepper.RimWorld.ModSdk.Utilities;

public class CustomStringTypes
{
    private readonly XsdSchema _xsdSchema;

    public CustomStringTypes(XsdSchema xsdSchema)
    {
        _xsdSchema = xsdSchema;
    }

    private static XsdSimpleType _rot4 = new()
    {
        Name = "Verse.Rot4",
        Restriction = new XsdRestriction()
        {
            Facets =
            {
                new XsdEnumeration("0"),
                new XsdEnumeration("1"),
                new XsdEnumeration("2"),
                new XsdEnumeration("3"),
                new XsdEnumeration("North"),
                new XsdEnumeration("East"),
                new XsdEnumeration("South"),
                new XsdEnumeration("West"),
            }
        },
    };

    private static readonly IReadOnlyDictionary<string, string> _regexTypes = new Dictionary<string, string>
    {
        {"Verse.IntRange", RegexPatterns.IntRange},
        {"Verse.FloatRange", RegexPatterns.FloatRange},
        {"Verse.CurvePoint", RegexPatterns.Vector2},
        {"Verse.IntVec2", RegexPatterns.IntVec2},
        {"Verse.IntVec3", RegexPatterns.IntVec3},
    };
    
    public XsdSimpleType GetCustomStringType(string typeName, Type type)
    {
        var fullTypeName = type.FullName;

        XsdSimpleType? result = null;
        if (_regexTypes.TryGetValue(fullTypeName, out var pattern))
        {
            result = new XsdSimpleType()
            {
                Name = typeName,
                Restriction = new XsdRestriction()
                {
                    Facets =
                    {
                        new XsdPattern(pattern)
                    }
                },
            };
        }
        else
        {
            switch (fullTypeName)
            {
                case "Verse.Rot4":
                    result = _rot4;
                    break;
                case "RimWorld.QualityRange":
                    result = GenerateQualityRange(typeName, type);
                    break;
            }
        }

        if (result == null)
        {
            return new XsdSimpleType()
            {
                Name = "xs:string"
            };
        }

        _xsdSchema.Types.Add(result);
        return result;
    }

    private static XsdSimpleType GenerateQualityRange(string typeName, Type type)
    {
        var restriction = new XsdRestriction();

        var enumFields = type.GetField("min").FieldType
            .GetFields(BindingFlags.Static | BindingFlags.Public)
            .OrderBy(f => (byte) f.GetRawConstantValue())
            .ToList();

        for (var i = 0; i < enumFields.Count; i++)
        {
            for (var j = i; j < enumFields.Count; j++)
            {
                var from = enumFields[i].Name;
                var to = enumFields[j].Name;
                restriction.Facets.Add(new XsdEnumeration($"{from}~{to}"));
            }
        }

        return new XsdSimpleType()
        {
            Name = typeName,
            Restriction = restriction,
        };
    }
}