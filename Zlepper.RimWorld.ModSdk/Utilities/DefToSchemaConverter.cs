using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace Zlepper.RimWorld.ModSdk.Utilities;

public class DefToSchemaConverter
{
    private readonly string _rootDefTypeClassName = "Def";
    private readonly string _rootDefTypeClassNamespace = "Verse";

    public DefToSchemaConverter(Type rootDefType)
    {
        _rootDefTypeClassName = rootDefType.Name;
        _rootDefTypeClassNamespace = rootDefType.Namespace!;
    }

    public DefToSchemaConverter()
    {
    }


    public XmlSchema CreateSchema(List<Type> defTypes)
    {
        var schema = new XmlSchema();

        var defsRoot = new XmlSchemaElement()
        {
            Name = "Defs"
        };
        var defsRootElementList = new XmlSchemaChoice()
        {
            MaxOccurs = decimal.MaxValue,
            MinOccurs = 0,
        };
        defsRoot.SchemaType = new XmlSchemaComplexType()
        {
            Particle = defsRootElementList
        };
        schema.Items.Add(defsRoot);

        foreach (var defType in defTypes)
        {
            var fields = new XmlSchemaAll();
            var type = new XmlSchemaComplexType
            {
                Name = defType.FullName,
                Particle = fields
            };

            var elementName = defType.Namespace == _rootDefTypeClassNamespace ? defType.Name : type.Name;
            defsRootElementList.Items.Add(new XmlSchemaElement()
            {
                Name = elementName,
                SchemaTypeName = new XmlQualifiedName(type.Name),
            });
            schema.Items.Add(type);

            foreach (var fieldInfo in GetDefFieldsForDefType(defType))
            {
                var fieldElement = GetFieldElement(fieldInfo);
                if (fieldElement == null)
                {
                    continue;
                }

                fields.Items.Add(fieldElement);
            }
        }

        return schema;
    }

    public const string XMLSchemaNamespace = "http://www.w3.org/2001/XMLSchema";

    private static readonly IReadOnlyDictionary<Type, XmlQualifiedName> _wellKnownFieldTypes =
        new Dictionary<Type, XmlQualifiedName>
        {
            {typeof(string), new XmlQualifiedName("string", XMLSchemaNamespace)},
            {typeof(byte), new XmlQualifiedName("byte", XMLSchemaNamespace)},
            {typeof(decimal), new XmlQualifiedName("decimal", XMLSchemaNamespace)},
            {typeof(int), new XmlQualifiedName("int", XMLSchemaNamespace)},
            {typeof(long), new XmlQualifiedName("long", XMLSchemaNamespace)},
            {typeof(short), new XmlQualifiedName("short", XMLSchemaNamespace)},
            {typeof(ushort), new XmlQualifiedName("unsignedShort", XMLSchemaNamespace)},
        };

    private XmlSchemaObject? GetFieldElement(FieldInfo field)
    {
        var name = field.Name;


        XmlSchemaAnnotation? annotation = null;
        var attributes = field.GetCustomAttributesData();
        if (attributes.Count > 0)
        {
            foreach (var attribute in attributes)
            {
                var attributeTypeName = attribute.AttributeType.Name;
                var attributeTypeNamespace = attribute.AttributeType.Namespace;

                if (attributeTypeNamespace != _rootDefTypeClassNamespace)
                {
                    continue;
                }

                switch (attributeTypeName)
                {
                    case "DescriptionAttribute":
                    {
                        if (attribute.ConstructorArguments.Count > 0)
                        {
                            var description = attribute.ConstructorArguments[0].Value as string;
                            if (!string.IsNullOrWhiteSpace(description))
                            {
                                annotation = new XmlSchemaAnnotation()
                                {
                                    Items =
                                    {
                                        new XmlSchemaDocumentation()
                                        {
                                            Markup = TextToNodeArray(description!),
                                        }
                                    }
                                };
                            }
                        }

                        break;
                    }
                    case "UnsavedAttribute":
                        // Unsaved attributes does not seem to be values that should be specified
                        return null;
                }
            }
        }


        XmlSchemaElement xmlSchemaElement;
        var fieldType = field.FieldType;
        if (_wellKnownFieldTypes.TryGetValue(fieldType, out var wellKnownType))
        {
            xmlSchemaElement = new XmlSchemaElement()
            {
                SchemaTypeName = wellKnownType
            };
        }
        else if (fieldType.IsEnum)
        {
            var restriction = new XmlSchemaSimpleTypeRestriction()
            {
                BaseTypeName = _wellKnownFieldTypes[typeof(string)],
            };

            foreach (var enumName in fieldType.GetEnumNames())
            {
                restriction.Facets.Add(new XmlSchemaEnumerationFacet()
                {
                    Value = enumName
                });
            }

            xmlSchemaElement = new XmlSchemaElement()
            {
                SchemaType = new XmlSchemaSimpleType()
                {
                    Content = restriction
                }
            };
            
            if (fieldType.GetCustomAttribute<FlagsAttribute>() != null)
            {
                xmlSchemaElement.Name = "li";
                xmlSchemaElement = new XmlSchemaElement()
                {
                    SchemaType = new XmlSchemaComplexType()
                    {
                        Particle = new XmlSchemaChoice()
                        {
                            MinOccurs = 0,
                            MaxOccurs = decimal.MaxValue,
                            Items =
                            {
                                xmlSchemaElement,
                            }
                        }
                    }
                };
            }
        }
        else if (fieldType.IsConstructedGenericType && fieldType.GetGenericTypeDefinition() == typeof(List<>))
        {
            var innerType = fieldType.GetGenericArguments()[0];

            throw new NotImplementedException();
        }
        else
        {
            throw new NotImplementedException();
        }

        switch (field.Name)
        {
            case "defName" when field.DeclaringType?.Name == _rootDefTypeClassName &&
                                field.DeclaringType?.Namespace == _rootDefTypeClassNamespace:
            {
                var originalTypeName = xmlSchemaElement.SchemaTypeName;
                xmlSchemaElement.SchemaTypeName = null;
                xmlSchemaElement.SchemaType = new XmlSchemaSimpleType()
                {
                    Content = new XmlSchemaSimpleTypeRestriction()
                    {
                        BaseTypeName = originalTypeName,
                        Facets =
                        {
                            new XmlSchemaPatternFacet()
                            {
                                Value = @"^[a-zA-Z0-9\-_]*$"
                            },
                        }
                    }
                };
                break;
            }
            case "label" when field.DeclaringType?.Name == _rootDefTypeClassName &&
                              field.DeclaringType?.Namespace == _rootDefTypeClassNamespace:
            {
                var originalTypeName = xmlSchemaElement.SchemaTypeName;
                xmlSchemaElement.SchemaTypeName = null;
                xmlSchemaElement.SchemaType = new XmlSchemaSimpleType()
                {
                    Content = new XmlSchemaSimpleTypeRestriction()
                    {
                        BaseTypeName = originalTypeName,
                        Facets =
                        {
                            new XmlSchemaPatternFacet()
                            {
                                Value = @"^[^\[\]\{\}]*$"
                            },
                        }
                    }
                };
                break;
            }
        }


        xmlSchemaElement.Annotation = annotation;
        xmlSchemaElement.Name = name;

        return xmlSchemaElement;
    }

    private List<FieldInfo> GetDefFieldsForDefType(Type defType)
    {
        return defType.GetFields(BindingFlags.Instance | BindingFlags.Public).ToList();
    }

    private static XmlNode[] TextToNodeArray(string text)
    {
        var doc = new XmlDocument();
        return new XmlNode[1]
        {
            doc.CreateTextNode(text)
        };
    }
}