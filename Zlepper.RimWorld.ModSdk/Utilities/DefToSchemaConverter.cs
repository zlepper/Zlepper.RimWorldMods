using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Zlepper.RimWorld.ModSdk.XsdOneOne;

namespace Zlepper.RimWorld.ModSdk.Utilities;

public class DefToSchemaConverter
{
    private readonly DefContext _defContext;
    private readonly IReadOnlyDictionary<Type, List<string>> _currentlyDefinedDefs;
    private readonly XsdSchema _schema;

    public DefToSchemaConverter(DefContext defContext, IReadOnlyDictionary<Type, List<string>> currentlyDefinedDefs)
    {
        _schema = new(rimWorldXmlNamespace);
        _defContext = defContext;
        _currentlyDefinedDefs = currentlyDefinedDefs;
        _customStringTypes = new CustomStringTypes(_schema);
    }


    public static readonly IReadOnlyDictionary<string, string> WellKnownFieldTypes =
        new Dictionary<string, string>
        {
            {typeof(string).FullName, "xs:string"},
            {typeof(Type).FullName, "xs:string"},
            {typeof(byte).FullName, "xs:byte"},
            {typeof(decimal).FullName, "xs:decimal"},
            {typeof(float).FullName, "xs:float"},
            {typeof(double).FullName, "xs:double"},
            {typeof(int).FullName, "xs:int"},
            {typeof(long).FullName, "xs:long"},
            {typeof(short).FullName, "xs:short"},
            {typeof(ushort).FullName, "xs:unsignedShort"},
            {typeof(bool).FullName, "xs:boolean"},
        };


    public const string defNameSchemaTypeName = "defName";
    public const string defLabelSchemaTypeName = "defLabel";

    public static readonly XmlSerializerNamespaces rimWorldXmlSerializerNamespaces =
        new XmlSerializerNamespaces(new[] {new XmlQualifiedName("", rimWorldXmlNamespace)});

    public static readonly XsdSimpleType defLabelSchemaType = new()
    {
        Name = defLabelSchemaTypeName,
        Restriction = new XsdRestriction
        {
            Facets =
            {
                new XsdPattern(@"[^\[\]\{\}]*")
            }
        },
    };

    public readonly XsdSimpleType defNameSchemaType = new()
    {
        Name = defNameSchemaTypeName,
        Restriction = new XsdRestriction()
        {
            Facets = {new XsdPattern(@"[a-zA-Z0-9\-_]*")}
        },
    };

    public const string rimWorldXmlNamespace = "rimworld";

    public XsdSchema CreateSchema(IEnumerable<Type> defTypes)
    {
        defTypes = defTypes.OrderBy(t => t.FullName, StringComparer.InvariantCultureIgnoreCase).ToList();

        var defsRoot = new XsdElement("Defs");
        var defsRootElementList = new XsdChoiceGroup()
        {
            MaxOccurs = Occurs.Unbounded,
            MinOccurs = Occurs.Zero,
        };
        defsRoot.SchemaType = new XsdComplexType()
        {
            Properties = defsRootElementList,
        };
        _schema.Elements.Add(defsRoot);
        _schema.Types.Add(defNameSchemaType);
        _schema.Types.Add(defLabelSchemaType);


        foreach (var defType in defTypes)
        {
            try
            {
                var classType = CreateXmlSchemaForClass(defType, 1);

                var elementName = _defContext.GetDefElementName(defType);
                defsRootElementList.Elements.Add(new XsdElement(elementName)
                {
                    SchemaTypeName = classType.Name,
                });
            }
            catch (UnwindingStackOverflowException e)
            {
                throw new UnwindingStackOverflowException(
                    $"Detected possible stackoverflow when checking def {defType}",
                    e);
            }
        }

        return _schema;
    }


    private XsdType CreateXmlSchemaForClass(Type defType, int depth)
    {
        if (depth > 100)
            throw new UnwindingStackOverflowException("Dug too deep");

        if (ShouldSkip(defType))
        {
            throw new UnwindingStackOverflowException(
                $"Type {defType} is an entity, which should not be hit normally. ");
        }

        var typeName = GetTypeNameForXml(defType);
        var existing = _schema.Types
            .FirstOrDefault(t => t.Name == typeName);

        if (existing != null)
        {
            return existing;
        }

        if (HasCustomLoadMethod(defType))
        {
            return InferSchemaTypeForCustomLoadClass(typeName, defType);
        }

        if (HasCustomFromStringMethod(defType))
        {
            return _customStringTypes.GetCustomStringType(typeName, defType);
        }

        var fields = new XsdAllGroup();
        var type = new XsdComplexType()
        {
            Name = typeName,
        };
        _schema.Types.Add(type);

        foreach (var fieldInfo in GetDefFieldsForDefType(defType))
        {
            var fieldElement = GetFieldElement(fieldInfo, depth);
            if (fieldElement == null)
            {
                continue;
            }

            fields.Elements.Add(fieldElement);
        }

        var baseType = defType.BaseType;
        if (baseType != null && baseType.FullName != typeof(object).FullName &&
            baseType.FullName != typeof(ValueType).FullName)
        {
            var baseTypeSchema = CreateXmlSchemaForClass(baseType, depth + 1);
            
            type.ComplexContent = new XsdComplexContent()
            {
                Extension = new XsdExtension(baseTypeSchema.Name ?? GetTypeNameForXml(baseType))
                {
                    Properties = fields
                }
            };
        }
        else
        {
            type.Properties = fields;
        }

        return type;
    }

    private static string GetTypeNameForXml(Type defType)
    {
        return defType.FullName!.Replace("+", ".");
    }

    private Type GetTypeFromPossiblyNullable(Type type)
    {
        if (IsNullable(type))
        {
            return type.GetGenericArguments()[0];
        }

        return type;
    }

    private static bool IsNullable(Type type)
    {
        return type.IsConstructedGenericType && type.GetGenericTypeDefinition().FullName == typeof(Nullable<>).FullName;
    }

    private XsdComplexType InferSchemaTypeForCustomLoadClass(string typeName,
        Type defType)
    {
        var fields = GetDefFieldsForDefType(defType);
        if (fields.Count == 2)
        {
            var simpleFields = fields.Where(f =>
                WellKnownFieldTypes.ContainsKey(GetTypeFromPossiblyNullable(f.FieldType).FullName)).ToList();
            if (simpleFields.Count == 1)
            {
                var defFields = fields.Where(f => _defContext.IsDef(f.FieldType)).ToList();
                if (defFields.Count == 1)
                {
                    var defField = defFields.Single();
                    var simpleField = simpleFields.Single();

                    var wellKnownFieldType =
                        WellKnownFieldTypes[GetTypeFromPossiblyNullable(simpleField.FieldType).FullName];

                    if (_currentlyDefinedDefs.TryGetValue(defField.FieldType, out var defNames))
                    {
                        XsdGroup xmlSchemaParticle = new XsdAllGroup();
                        foreach (var name in defNames.Distinct())
                        {
                            xmlSchemaParticle.Elements.Add(new XsdElement(name)
                            {
                                SchemaTypeName = wellKnownFieldType,
                                MinOccurs = Occurs.Zero,
                            });
                        }

                        if (IsNullable(simpleField.FieldType))
                        {
                            var defOptions = CreateDefOptionsType(defField.FieldType);
                            var listAlternative = new XsdElement("li")
                            {
                                Name = "li",
                                SchemaTypeName = defOptions.Name,
                                MinOccurs = Occurs.Zero,
                                MaxOccurs = Occurs.Unbounded,
                            };

                            var newParticle = new XsdChoiceGroup()
                            {
                                Elements = new(xmlSchemaParticle.Elements)
                                {
                                    listAlternative
                                },
                            };

                            xmlSchemaParticle = newParticle;
                        }

                        var customDictionaryType = new XsdComplexType()
                        {
                            Name = typeName,
                            Properties = xmlSchemaParticle,
                        };


                        _schema.Types.Add(customDictionaryType);
                        return customDictionaryType;
                    }
                }
            }
        }

        if (fields.Count == 1)
        {
            var field = fields.Single();
            if (_defContext.IsDef(field.FieldType))
            {
                var xmlSchemaParticle = new XsdChoiceGroup()
                {
                    MinOccurs = Occurs.Zero,
                    MaxOccurs = Occurs.Unbounded
                };

                foreach (var concreteDefType in _currentlyDefinedDefs.Keys.Where(t => t.IsSubclassOf(field.FieldType)))
                {
                    var defOptionsType = CreateDefOptionsType(concreteDefType);

                    var element = new XsdElement(_defContext.GetDefElementName(concreteDefType))
                    {
                        SchemaTypeName = defOptionsType.Name,
                    };
                    xmlSchemaParticle.Elements.Add(element);
                }

                var customDefLinks = new XsdComplexType()
                {
                    Name = typeName,
                    Properties = xmlSchemaParticle,
                };
                _schema.Types.Add(customDefLinks);
                return customDefLinks;
            }
        }

        var type = new XsdComplexType()
        {
            Name = typeName,
            Properties = new XsdSequenceGroup()
            {
                Elements =
                {
                    new XsdAny()
                    {
                        ProcessContents = XmlSchemaContentProcessing.Skip,
                        MinOccurs = Occurs.Zero,
                        MaxOccurs = Occurs.Unbounded
                    }
                }
            },
        };
        _schema.Types.Add(type);
        return type;
    }

    private static bool HasCustomLoadMethod(Type defType)
    {
        return defType.GetMethod("LoadDataFromXmlCustom",
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) != null;
    }

    private static bool HasCustomFromStringMethod(Type defType)
    {
        return defType.GetMethod("FromString",
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) != null;
    }

    private bool ShouldSkip(Type type)
    {
        if (type.IsConstructedGenericType)
        {
            return type.GetGenericArguments().Any(ShouldSkip);
        }

        var isEntity = _defContext.GetBaseTypes(type).Any(t => t.FullName == "Verse.Entity");
        var isMap = type.FullName == "Verse.Map";
        var isIdeo = type.FullName == "RimWorld.Ideo";
        var isFaction  =type.FullName == "RimWorld.Faction";
        var isSteamRelated = type.Namespace?.StartsWith("Verse.Steam") ?? false;

        return isEntity || isMap || isIdeo || isSteamRelated || isFaction;
    }

    private bool ShouldSkip(FieldInfo field)
    {
        if (ShouldSkip(field.FieldType))
        {
            return true;
        }

        var isInternalField = field.IsPrivate && field.Name.EndsWith("Int");

        var invalidName = field.Name.StartsWith("<");

        return isInternalField || field.IsSpecialName || field.IsLiteral || invalidName;
    }

    public const string XMLSchemaNamespace = "http://www.w3.org/2001/XMLSchema";

    private readonly CustomStringTypes _customStringTypes;

    private XsdElement? GetFieldElement(FieldInfo field, int depth)
    {
        var name = field.Name;

        if (field.FieldType.IsInterface)
        {
            return null;
        }

        if (ShouldSkip(field))
        {
            return null;
        }

        XsdAnnotation? annotation = null;
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
                    case "DescriptionAttribute":
                    {
                        if (attribute.ConstructorArguments.Count > 0)
                        {
                            var description = attribute.ConstructorArguments[0].Value as string;
                            if (!string.IsNullOrWhiteSpace(description))
                            {
                                annotation = new XsdAnnotation()
                                {
                                    Documentation = description
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


        var xmlSchemaElement = InferXmlSchemaElement(field, field.FieldType, depth);
        if (xmlSchemaElement == null)
        {
            return null;
        }

        switch (field.Name)
        {
            case "defName" when _defContext.IsRootDefClass(field.DeclaringType!):
            {
                xmlSchemaElement.SchemaTypeName = defNameSchemaTypeName;
                break;
            }
            case "label" when _defContext.IsRootDefClass(field.DeclaringType!):
            {
                xmlSchemaElement.SchemaTypeName = defLabelSchemaTypeName;
                break;
            }
        }


        xmlSchemaElement.Annotation = annotation;
        xmlSchemaElement.Name = name;
        xmlSchemaElement.MinOccurs = 0;

        return xmlSchemaElement;
    }

    private XsdElement? InferXmlSchemaElement(FieldInfo field, Type type, int depth)
    {
        if (WellKnownFieldTypes.TryGetValue(type.FullName, out var wellKnownType))
        {
            return new XsdElement(field.Name)
            {
                SchemaTypeName = wellKnownType
            };
        }

        if (type.IsEnum || type.BaseType?.FullName == "System.Enum")
        {
            var restriction = new XsdRestriction();

            if (type.IsEnum)
            {
                foreach (var enumName in type.GetEnumNames())
                {
                    restriction.Facets.Add(new XsdEnumeration(enumName));
                }
            }
            else
            {
                foreach (var enumField in type.GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    restriction.Facets.Add(new XsdEnumeration(enumField.Name));
                }
            }

            var xmlSchemaElement = new XsdElement(field.Name)
            {
                SchemaType = new XsdSimpleType()
                {
                    Restriction = restriction
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


                var innerSchema = InferXmlSchemaElement(field, innerType, depth);
                if (innerSchema == null)
                {
                    return null;
                }

                return HasCustomLoadMethod(innerType) ? innerSchema : WrapInList(innerSchema);
            }

            if (genericTypeDefinitionFullName == typeof(Dictionary<,>).FullName)
            {
                var keyType = type.GetGenericArguments()[0];
                var valueType = type.GetGenericArguments()[1];

                var keySchema = InferXmlSchemaElement(field, keyType, depth);
                var valueSchema = InferXmlSchemaElement(field, valueType, depth);
                if (keySchema == null || valueSchema == null)
                {
                    return null;
                }

                return WrapInDictionary(keySchema, valueSchema);
            }

            if (genericTypeDefinitionFullName == "RimWorld.QuestGen.SlateRef`1")
            {
                return new XsdElement(field.Name)
                {
                    SchemaTypeName = "xs:string"
                };
            }

            if (genericTypeDefinitionFullName == typeof(Nullable<>).FullName)
            {
                var innerType = type.GetGenericArguments()[0];

                return InferXmlSchemaElement(field, innerType, depth);
            }

            if (genericTypeDefinitionFullName == typeof(Predicate<>).FullName ||
                genericTypeDefinitionFullName == typeof(Func<,>).FullName)
            {
                return null;
            }

            throw new NotImplementedException(
                $"Can't handle field {field.Name} of type {type} on class {field.DeclaringType}. Full field type: {field.FieldType}. Base base type: {type.BaseType}");
        }

        if (_defContext.IsDef(type))
        {
            var defOptionsType = CreateDefOptionsType(type);

            return new XsdElement(field.Name)
            {
                SchemaTypeName = defOptionsType.Name
            };
        }

        try
        {
            var schemaType = CreateXmlSchemaForClass(type, depth + 1);

            return new XsdElement("")
            {
                SchemaTypeName = schemaType.Name,
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

    private XsdSimpleType CreateDefOptionsType(Type defType)
    {
        var optionsTypeName = GetTypeNameForXml(defType) + ".Enumeration";

        var existing = _schema.Types.OfType<XsdSimpleType>().FirstOrDefault(type => type.Name == optionsTypeName);
        if (existing != null)
        {
            return existing;
        }

        var optionsType = new XsdSimpleType()
        {
            Name = optionsTypeName,
        };

        var restriction = new XsdRestriction();

        if (_currentlyDefinedDefs.TryGetValue(defType, out var knownDefs))
        {
            foreach (var defName in knownDefs)
            {
                restriction.Facets.Add(new XsdEnumeration(defName));
            }
        }

        optionsType.Restriction = restriction;
        _schema.Types.Add(optionsType);

        return optionsType;
    }

    private XsdElement WrapInDictionary(XsdElement keySchema, XsdElement valueSchema)
    {
        keySchema.Name = "key";
        valueSchema.Name = "value";
        var innerType = new XsdElement("li")
        {
            SchemaType = new XsdComplexType()
            {
                Properties = new XsdSequenceGroup()
                {
                    Elements =
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
        return defType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                                 BindingFlags.DeclaredOnly)
            .Where(f => !ShouldSkip(f))
            .ToList();
    }

    private static XmlNode[] TextToNodeArray(string text)
    {
        var doc = new XmlDocument();
        return new XmlNode[]
        {
            doc.CreateTextNode(text)
        };
    }

    private XsdElement WrapInList(XsdElement mainElement)
    {
        mainElement.Name = "li";

        return new("")
        {
            SchemaType = new XsdComplexType()
            {
                Properties = new XsdChoiceGroup()
                {
                    MinOccurs = Occurs.Zero,
                    MaxOccurs = Occurs.Unbounded,
                    Elements = {mainElement},
                }
            }
        };
    }
}

public class UnwindingStackOverflowException : Exception
{
    public UnwindingStackOverflowException(string message, Exception? innerException = null) : base(message,
        innerException)
    {
    }
}