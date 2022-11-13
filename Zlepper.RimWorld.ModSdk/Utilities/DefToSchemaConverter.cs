using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

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

    private const string defNameSchemaTypeName = "defName";
    private const string defLabelSchemaTypeName = "defLabel";

    private readonly XmlSchemaSimpleType defLabelSchemaType = new()
    {
        Name = defLabelSchemaTypeName,
        Namespaces = new(new[] {new XmlQualifiedName("", rimWorldXmlNamespace)}),
        Content = new XmlSchemaSimpleTypeRestriction()
        {
            BaseTypeName = _wellKnownFieldTypes[typeof(string).FullName],
            Facets =
            {
                new XmlSchemaPatternFacet()
                {
                    Value = @"[^\[\]\{\}]*"
                },
            }
        }
    };

    private readonly XmlSchemaSimpleType defNameSchemaType = new()
    {
        Name = defNameSchemaTypeName,
        Namespaces = new(new[] {new XmlQualifiedName("", rimWorldXmlNamespace)}),
        Content = new XmlSchemaSimpleTypeRestriction()
        {
            BaseTypeName = _wellKnownFieldTypes[typeof(string).FullName],
            Facets =
            {
                new XmlSchemaPatternFacet()
                {
                    Value = @"[a-zA-Z0-9\-_]*"
                },
            }
        }
    };

    private const string rimWorldXmlNamespace = "rimworld";
    private static readonly XmlSerializerNamespaces rimWorldXmlSerializerNamespaces = new XmlSerializerNamespaces(new[] {new XmlQualifiedName("", rimWorldXmlNamespace)});

    public XmlSchema CreateSchema(IEnumerable<Type> defTypes)
    {
        defTypes = defTypes.OrderBy(t => t.FullName, StringComparer.InvariantCultureIgnoreCase).ToList();
        var schema = new XmlSchema();
        schema.Namespaces.Add("", rimWorldXmlNamespace);
        schema.TargetNamespace = rimWorldXmlNamespace;
        schema.ElementFormDefault = XmlSchemaForm.Qualified;

        var defsRoot = new XmlSchemaElement()
        {
            Name = "Defs",
            Namespaces = rimWorldXmlSerializerNamespaces,
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
        schema.Items.Add(defNameSchemaType);
        schema.Items.Add(defLabelSchemaType);


        foreach (var defType in defTypes)
        {
            try
            {
                var type = CreateXmlSchemaForClass(defType, schema, 1);

                var elementName = defType.Namespace == _rootDefTypeClassNamespace || defType.Namespace == "RimWorld"
                    ? defType.Name
                    : type.Name;
                defsRootElementList.Items.Add(new XmlSchemaElement()
                {
                    Name = elementName,
                    Namespaces = rimWorldXmlSerializerNamespaces,
                    SchemaTypeName = new XmlQualifiedName(defType.FullName, rimWorldXmlNamespace),
                });
            }
            catch (UnwindingStackOverflowException e)
            {
                throw new UnwindingStackOverflowException(
                    $"Detected possible stackoverflow when checking def {defType}",
                    e);
            }
        }

        return schema;
    }


    private XmlSchemaComplexType CreateXmlSchemaForClass(Type defType, XmlSchema schema, int depth)
    {
        if (depth > 100)
            throw new UnwindingStackOverflowException("Dug too deep");

        if (IsEntity(defType))
        {
            throw new UnwindingStackOverflowException(
                $"Type {defType} is an entity, which should not be hit normally. ");
        }

        var typeName = defType.FullName!.Replace("+", ".");
        var existing = schema.Items
            .OfType<XmlSchemaComplexType>()
            .FirstOrDefault(t => t.Name == typeName);

        if (existing != null)
        {
            return existing;
        }

        if (HasCustomLoadMethod(defType))
        {
            var type = new XmlSchemaComplexType()
            {
                Name = typeName,
                Namespaces = rimWorldXmlSerializerNamespaces,
                Particle = new XmlSchemaSequence()
                {
                    Items =
                    {
                        new XmlSchemaAny()
                        {
                            ProcessContents = XmlSchemaContentProcessing.Skip,
                            MinOccurs = 0,
                        }
                    }
                },
            };
            schema.Items.Add(type);
            return type;
        }
        else
        {

            var fields = new XmlSchemaAll();
            var type = new XmlSchemaComplexType
            {
                Name = typeName,
                Namespaces = rimWorldXmlSerializerNamespaces,
                Particle = fields
            };
            schema.Items.Add(type);

            foreach (var fieldInfo in GetDefFieldsForDefType(defType))
            {
                var fieldElement = GetFieldElement(fieldInfo, schema, depth);
                if (fieldElement == null)
                {
                    continue;
                }

                fields.Items.Add(fieldElement);
            }

            return type;
        }
    }

    private static bool HasCustomLoadMethod(Type defType)
    {
        return defType.GetMethod("LoadDataFromXmlCustom",
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) != null;
    }

    private static bool IsEntity(Type defType)
    {
        return GetBaseTypes(defType).Any(t => t.FullName == "Verse.Entity");
    }

    public const string XMLSchemaNamespace = "http://www.w3.org/2001/XMLSchema";

    private static readonly IReadOnlyDictionary<string, XmlQualifiedName> _wellKnownFieldTypes =
        new Dictionary<string, XmlQualifiedName>
        {
            {typeof(string).FullName, new XmlQualifiedName("string", XMLSchemaNamespace)},
            {typeof(Type).FullName, new XmlQualifiedName("string", XMLSchemaNamespace)},
            {typeof(byte).FullName, new XmlQualifiedName("byte", XMLSchemaNamespace)},
            {typeof(decimal).FullName, new XmlQualifiedName("decimal", XMLSchemaNamespace)},
            {typeof(float).FullName, new XmlQualifiedName("float", XMLSchemaNamespace)},
            {typeof(double).FullName, new XmlQualifiedName("double", XMLSchemaNamespace)},
            {typeof(int).FullName, new XmlQualifiedName("int", XMLSchemaNamespace)},
            {typeof(long).FullName, new XmlQualifiedName("long", XMLSchemaNamespace)},
            {typeof(short).FullName, new XmlQualifiedName("short", XMLSchemaNamespace)},
            {typeof(ushort).FullName, new XmlQualifiedName("unsignedShort", XMLSchemaNamespace)},
            {typeof(bool).FullName, new XmlQualifiedName("boolean", XMLSchemaNamespace)},
        };

    private XmlSchemaElement? GetFieldElement(FieldInfo field, XmlSchema schema, int depth)
    {
        var name = field.Name;

        if (field.FieldType.IsInterface)
        {
            return null;
        }

        if (IsEntity(field.FieldType))
        {
            return null;
        }

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


        var xmlSchemaElement = InferXmlSchemaElement(field, field.FieldType, schema, depth);
        if (xmlSchemaElement == null)
        {
            return null;
        }

        switch (field.Name)
        {
            case "defName" when field.DeclaringType?.Name == _rootDefTypeClassName &&
                                field.DeclaringType?.Namespace == _rootDefTypeClassNamespace:
            {
                xmlSchemaElement.SchemaTypeName = new XmlQualifiedName(defNameSchemaTypeName, rimWorldXmlNamespace);
                break;
            }
            case "label" when field.DeclaringType?.Name == _rootDefTypeClassName &&
                              field.DeclaringType?.Namespace == _rootDefTypeClassNamespace:
            {
                xmlSchemaElement.SchemaTypeName = new XmlQualifiedName(defLabelSchemaTypeName, rimWorldXmlNamespace);
                break;
            }
        }


        xmlSchemaElement.Annotation = annotation;
        xmlSchemaElement.Name = name;
        xmlSchemaElement.Namespaces = rimWorldXmlSerializerNamespaces;
        xmlSchemaElement.MinOccurs = 0;

        return xmlSchemaElement;
    }

    private XmlSchemaElement? InferXmlSchemaElement(FieldInfo field, Type type, XmlSchema schema, int depth)
    {
        if (_wellKnownFieldTypes.TryGetValue(type.FullName, out var wellKnownType))
        {
            return new XmlSchemaElement()
            {
                SchemaTypeName = wellKnownType
            };
        }

        if (type.IsEnum || type.BaseType?.FullName == "System.Enum")
        {
            var restriction = new XmlSchemaSimpleTypeRestriction()
            {
                BaseTypeName = _wellKnownFieldTypes[typeof(string).FullName],
            };

            if (type.IsEnum)
            {
                foreach (var enumName in type.GetEnumNames())
                {
                    restriction.Facets.Add(new XmlSchemaEnumerationFacet()
                    {
                        Value = enumName
                    });
                }
            }
            else
            {
                foreach (var enumField in type.GetFields(BindingFlags.Public|BindingFlags.Static))
                {
                    restriction.Facets.Add(new XmlSchemaEnumerationFacet()
                    {
                        Value = enumField.Name
                    });
                }
            }

            var xmlSchemaElement = new XmlSchemaElement()
            {
                SchemaType = new XmlSchemaSimpleType()
                {
                    Content = restriction
                }
            };

            var flagsAttribute = type.GetCustomAttributesData()
                .FirstOrDefault(attr => attr.AttributeType.FullName == typeof(FlagsAttribute).FullName);
            if (flagsAttribute != null)
            {
                xmlSchemaElement = WrapInList(xmlSchemaElement);
            }

            return xmlSchemaElement;
        }

        if (type.IsConstructedGenericType)
        {
            var genericTypeDefinitionFullName = type.GetGenericTypeDefinition().FullName;
            if (genericTypeDefinitionFullName == typeof(List<>).FullName)
            {
                var innerType = type.GetGenericArguments()[0];


                var innerSchema = InferXmlSchemaElement(field, innerType, schema, depth);
                if (innerSchema == null)
                {
                    return null;
                }
                return HasCustomLoadMethod(innerType) ? innerSchema : WrapInList(innerSchema);
            }
            else if (genericTypeDefinitionFullName == typeof(Dictionary<,>).FullName)
            {
                var keyType = type.GetGenericArguments()[0];
                var valueType = type.GetGenericArguments()[1];

                var keySchema = InferXmlSchemaElement(field, keyType, schema, depth);
                var valueSchema = InferXmlSchemaElement(field, valueType, schema, depth);
                if (keySchema == null || valueSchema == null)
                {
                    return null;
                }

                return WrapInDictionary(keySchema, valueSchema);
            }
            else if (genericTypeDefinitionFullName == "RimWorld.QuestGen.SlateRef`1")
            {
                return new XmlSchemaElement
                {
                    SchemaTypeName = _wellKnownFieldTypes[typeof(string).FullName]
                };
            }
            else if (genericTypeDefinitionFullName == typeof(Nullable<>).FullName)
            {
                var innerType = type.GetGenericArguments()[0];

                return InferXmlSchemaElement(field, innerType, schema, depth);
            }
            else if (genericTypeDefinitionFullName == typeof(Predicate<>).FullName ||
                     genericTypeDefinitionFullName == typeof(Func<,>).FullName)
            {
                return null;
            }
            else
            {
                throw new NotImplementedException(
                    $"Can't handle field {field.Name} of type {type} on class {field.DeclaringType}. Full field type: {field.FieldType}. Base base type: {type.BaseType}");
            }
        }

        if (IsDef(type))
        {
            return new XmlSchemaElement
            {
                SchemaTypeName = _wellKnownFieldTypes[typeof(string).FullName]
            };
        }

        if (type.FullName == "RimWorld.Faction")
        {
            return new XmlSchemaElement
            {
                SchemaTypeName = _wellKnownFieldTypes[typeof(string).FullName]
            };
        }

        try
        {
            var schemaType = CreateXmlSchemaForClass(type, schema, depth + 1);

            return new XmlSchemaElement()
            {
                SchemaTypeName = new XmlQualifiedName(schemaType.Name, rimWorldXmlNamespace)
            };
        }
        catch (UnwindingStackOverflowException e)
        {
            throw new UnwindingStackOverflowException(
                $"Detected possible stackoverflow: Can't handle field {field.Name} of type {type} on class {field.DeclaringType}. Full field type: {field.FieldType}. Base base type: {type.BaseType}",
                e);
        }
        catch (NotImplementedException e)
        {
            throw new NotImplementedException(
                $"Can't handle field {field.Name} of type {type} on class {field.DeclaringType}. Full field type: {field.FieldType}. Base base type: {type.BaseType}",
                e);
        }
    }

    private XmlSchemaElement WrapInDictionary(XmlSchemaElement keySchema, XmlSchemaElement valueSchema)
    {
        keySchema.Name = "key";
        valueSchema.Name = "value";
        var innerType = new XmlSchemaElement()
        {
            Name = "li",
            SchemaType = new XmlSchemaComplexType()
            {
                Particle = new XmlSchemaSequence()
                {
                    Items =
                    {
                        keySchema,
                        valueSchema,
                    }
                }
            }
        };

        return WrapInList(innerType);
    }

    private List<FieldInfo> GetDefFieldsForDefType(Type defType)
    {
        return defType.GetFields(BindingFlags.Instance | BindingFlags.Public).ToList();
    }

    private static XmlNode[] TextToNodeArray(string text)
    {
        var doc = new XmlDocument();
        return new XmlNode[]
        {
            doc.CreateTextNode(text)
        };
    }

    private XmlSchemaElement WrapInList(XmlSchemaElement mainElement)
    {
        mainElement.Name = "li";

        return new()
        {
            SchemaType = new XmlSchemaComplexType()
            {
                Particle = new XmlSchemaChoice()
                {
                    MinOccurs = 0,
                    MaxOccurs = decimal.MaxValue,
                    Items =
                    {
                        mainElement,
                    }
                }
            }
        };
    }


    public bool IsDef(Type type)
    {
        return type.IsClass && !type.IsAbstract &&
               GetBaseTypes(type).Any(t =>
                   t.Namespace == _rootDefTypeClassNamespace && t.Name == _rootDefTypeClassName);
    }

    private static IEnumerable<Type> GetBaseTypes(Type type)
    {
        if (type.BaseType == null || type.BaseType.FullName == typeof(object).FullName)
        {
            return Enumerable.Empty<Type>();
        }

        return Enumerable.Repeat(type.BaseType, 1).Concat(GetBaseTypes(type.BaseType!));
    }
}

public class UnwindingStackOverflowException : Exception
{
    public UnwindingStackOverflowException(string message, Exception? innerException = null) : base(message,
        innerException)
    {
    }
}