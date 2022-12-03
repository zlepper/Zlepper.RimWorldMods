using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Schema;

namespace Zlepper.RimWorld.ModSdk.Utilities;

public class CustomStringTypes
{
    private readonly DefContext _defContext;
    private readonly IReadOnlyDictionary<Type, List<string>> _currentlyDefinedDefs;

    public CustomStringTypes(DefContext defContext, IReadOnlyDictionary<Type, List<string>> currentlyDefinedDefs)
    {
        _defContext = defContext;
        _currentlyDefinedDefs = currentlyDefinedDefs;
    }

    private static XmlSchemaSimpleType _rot4 = new XmlSchemaSimpleType()
    {
        Name = "Verse.Rot4",
        Namespaces = DefToSchemaConverter.rimWorldXmlSerializerNamespaces,
        Content = new XmlSchemaSimpleTypeRestriction()
        {
            BaseTypeName = DefToSchemaConverter.WellKnownFieldTypes[typeof(string).FullName],
            Facets =
            {
                new XmlSchemaEnumerationFacet()
                {
                    Value = "0",
                },
                new XmlSchemaEnumerationFacet()
                {
                    Value = "1",
                },
                new XmlSchemaEnumerationFacet()
                {
                    Value = "2",
                },
                new XmlSchemaEnumerationFacet()
                {
                    Value = "3",
                },
                new XmlSchemaEnumerationFacet()
                {
                    Value = "North",
                },
                new XmlSchemaEnumerationFacet()
                {
                    Value = "East",
                },
                new XmlSchemaEnumerationFacet()
                {
                    Value = "South",
                },
                new XmlSchemaEnumerationFacet()
                {
                    Value = "West",
                },
            }
        }
    };

    private static IReadOnlyDictionary<string, string> _regexTypes = new Dictionary<string, string>
    {
        {"Verse.IntRange", RegexPatterns.IntRange},
        {"Verse.FloatRange", RegexPatterns.FloatRange},
        {"Verse.CurvePoint", RegexPatterns.Vector2},
        {"Verse.IntVec2", RegexPatterns.IntVec2},
        {"Verse.IntVec3", RegexPatterns.IntVec3},
    };
    
    public XmlSchemaSimpleType GetCustomStringType(XmlSchema schema, string typeName, Type type)
    {
        var fullTypeName = type.FullName;

        XmlSchemaSimpleType? result = null;
        if (_regexTypes.TryGetValue(fullTypeName, out var pattern))
        {
            result = new XmlSchemaSimpleType()
            {
                Name = typeName,
                Namespaces = DefToSchemaConverter.rimWorldXmlSerializerNamespaces,
                Content = new XmlSchemaSimpleTypeRestriction()
                {
                    BaseTypeName = DefToSchemaConverter.WellKnownFieldTypes[typeof(string).FullName],
                    Facets =
                    {
                        new XmlSchemaPatternFacet()
                        {
                            Value = pattern,
                        },
                    }
                }
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
            return XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.String);
        }

        schema.Items.Add(result);
        return result;
    }

    private static XmlSchemaSimpleType GenerateQualityRange(string typeName, Type type)
    {
        var restriction = new XmlSchemaSimpleTypeRestriction()
        {
            BaseTypeName = DefToSchemaConverter.WellKnownFieldTypes[typeof(string).FullName],
        };

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
                restriction.Facets.Add(new XmlSchemaEnumerationFacet()
                {
                    Value = $"{from}~{to}",
                });
            }
        }

        return new XmlSchemaSimpleType()
        {
            Name = typeName,
            Namespaces = DefToSchemaConverter.rimWorldXmlSerializerNamespaces,
            Content = restriction
        };
    }
}