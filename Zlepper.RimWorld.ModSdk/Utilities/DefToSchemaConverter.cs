using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.Utilities;

public class DefToSchemaConverter
{
    private readonly DefContext _defContext;
    private readonly IReadOnlyDictionary<Type, List<string>> _currentlyDefinedDefs;

    public DefToSchemaConverter(DefContext defContext, IReadOnlyDictionary<Type, List<string>> currentlyDefinedDefs)
    {
        _defContext = defContext;
        _currentlyDefinedDefs = currentlyDefinedDefs;
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

    private static readonly XmlSerializerNamespaces rimWorldXmlSerializerNamespaces =
        new XmlSerializerNamespaces(new[] {new XmlQualifiedName("", rimWorldXmlNamespace)});

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
                CreateXmlSchemaForClass(defType, schema, 1);

                var elementName = _defContext.GetDefElementName(defType);
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

        if (ShouldSkip(defType))
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
            return InferSchemaTypeForCustomLoadClass(schema, typeName, defType);
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

    private XmlSchemaComplexType InferSchemaTypeForCustomLoadClass(XmlSchema schema, string typeName,
        Type defType)
    {
        var fields = GetDefFieldsForDefType(defType);
        if (fields.Count == 2)
        {
            var simpleFields = fields.Where(f =>
                _wellKnownFieldTypes.ContainsKey(GetTypeFromPossiblyNullable(f.FieldType).FullName)).ToList();
            if (simpleFields.Count == 1)
            {
                var defFields = fields.Where(f => _defContext.IsDef(f.FieldType)).ToList();
                if (defFields.Count == 1)
                {
                    var defField = defFields.Single();
                    var simpleField = simpleFields.Single();

                    var wellKnownFieldType =
                        _wellKnownFieldTypes[GetTypeFromPossiblyNullable(simpleField.FieldType).FullName];

                    if (_currentlyDefinedDefs.TryGetValue(defField.FieldType, out var defNames))
                    {
                        XmlSchemaGroupBase xmlSchemaParticle = new XmlSchemaAll();
                        foreach (var name in defNames.Distinct())
                        {
                            xmlSchemaParticle.Items.Add(new XmlSchemaElement()
                            {
                                Name = name,
                                Namespaces = rimWorldXmlSerializerNamespaces,
                                SchemaTypeName = wellKnownFieldType,
                                MinOccurs = 0,
                            });
                        }

                        if (IsNullable(simpleField.FieldType))
                        {
                            var defOptions = CreateDefOptionsType(defField.FieldType, schema);
                            var listAlternative = new XmlSchemaElement()
                            {
                                Name = "li",
                                Namespaces = rimWorldXmlSerializerNamespaces,
                                SchemaTypeName = new XmlQualifiedName(defOptions.Name, rimWorldXmlNamespace),
                                MinOccurs = 0,
                                MaxOccurs = decimal.MaxValue,
                            };

                            var newParticle = new XmlSchemaChoice()
                            {
                                Items = {listAlternative}
                            };

                            foreach (var xmlSchemaObject in xmlSchemaParticle.Items)
                            {
                                newParticle.Items.Add(xmlSchemaObject);
                            }
                            
                            xmlSchemaParticle = newParticle;
                            
                        }
                        
                        var customDictionaryType = new XmlSchemaComplexType()
                        {
                            Name = typeName,
                            Namespaces = rimWorldXmlSerializerNamespaces,
                            Particle = xmlSchemaParticle,
                        };

                        
                        schema.Items.Add(customDictionaryType);
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
                var xmlSchemaParticle = new XmlSchemaChoice()
                {
                    MinOccurs = 0,
                    MaxOccurs = decimal.MaxValue
                };

                foreach (var concreteDefType in _currentlyDefinedDefs.Keys.Where(t => t.IsSubclassOf(field.FieldType)))
                {
                    var defOptionsType = CreateDefOptionsType(concreteDefType, schema);

                    var element = new XmlSchemaElement
                    {
                        Name = _defContext.GetDefElementName(concreteDefType),
                        SchemaTypeName = new XmlQualifiedName(defOptionsType.Name, rimWorldXmlNamespace),
                    };
                    xmlSchemaParticle.Items.Add(element);
                }

                var customDefLinks = new XmlSchemaComplexType()
                {
                    Name = typeName,
                    Namespaces = rimWorldXmlSerializerNamespaces,
                    Particle = xmlSchemaParticle,
                };
                schema.Items.Add(customDefLinks);
                return customDefLinks;
            }
            else
            {
                throw new UnwindingStackOverflowException(
                    $"Type {defType} has a single field, but it is not a def. It is: {field.FieldType}");
            }
        }

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
                        MaxOccurs = decimal.MaxValue
                    }
                }
            },
        };
        schema.Items.Add(type);
        return type;
    }

    private static bool HasCustomLoadMethod(Type defType)
    {
        return defType.GetMethod("LoadDataFromXmlCustom",
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
        var isSteamRelated = type.Namespace?.StartsWith("Verse.Steam") ?? false;


        var isInstanceOfDef = type.GetField("def", BindingFlags.Instance | BindingFlags.Public)?.FieldType.Name ==
                              $"{type.Name}Def";

        return isEntity || isMap || isIdeo || isSteamRelated || isInstanceOfDef;
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

        if (ShouldSkip(field))
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
            case "defName" when field.DeclaringType?.Name == _defContext.RootDefTypeClassName &&
                                field.DeclaringType?.Namespace == _defContext.RootDefTypeClassNamespace:
            {
                xmlSchemaElement.SchemaTypeName = new XmlQualifiedName(defNameSchemaTypeName, rimWorldXmlNamespace);
                break;
            }
            case "label" when field.DeclaringType?.Name == _defContext.RootDefTypeClassName &&
                              field.DeclaringType?.Namespace == _defContext.RootDefTypeClassNamespace:
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
                foreach (var enumField in type.GetFields(BindingFlags.Public | BindingFlags.Static))
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

            if (genericTypeDefinitionFullName == typeof(Dictionary<,>).FullName)
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

            if (genericTypeDefinitionFullName == "RimWorld.QuestGen.SlateRef`1")
            {
                return new XmlSchemaElement
                {
                    SchemaTypeName = _wellKnownFieldTypes[typeof(string).FullName]
                };
            }

            if (genericTypeDefinitionFullName == typeof(Nullable<>).FullName)
            {
                var innerType = type.GetGenericArguments()[0];

                return InferXmlSchemaElement(field, innerType, schema, depth);
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
            var defOptionsType = CreateDefOptionsType(type, schema);

            return new XmlSchemaElement
            {
                SchemaTypeName = new XmlQualifiedName(defOptionsType.Name, rimWorldXmlNamespace)
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

    private XmlSchemaSimpleType CreateDefOptionsType(Type defType, XmlSchema schema)
    {
        var optionsTypeName = defType.FullName!.Replace("+", ".") + ".Enumeration";

        var existing = schema.Items.OfType<XmlSchemaSimpleType>().FirstOrDefault(type => type.Name == optionsTypeName);
        if (existing != null)
        {
            return existing;
        }

        var optionsType = new XmlSchemaSimpleType()
        {
            Name = optionsTypeName,
            Namespaces = rimWorldXmlSerializerNamespaces
        };

        var restriction = new XmlSchemaSimpleTypeRestriction()
        {
            BaseTypeName = _wellKnownFieldTypes[typeof(string).FullName],
        };

        if (_currentlyDefinedDefs.TryGetValue(defType, out var knownDefs))
        {
            foreach (var defName in knownDefs)
            {
                restriction.Facets.Add(new XmlSchemaEnumerationFacet()
                {
                    Value = defName
                });
            }
        }

        optionsType.Content = restriction;
        schema.Items.Add(optionsType);

        return optionsType;
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
        return defType.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
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
}

public class UnwindingStackOverflowException : Exception
{
    public UnwindingStackOverflowException(string message, Exception? innerException = null) : base(message,
        innerException)
    {
    }
}