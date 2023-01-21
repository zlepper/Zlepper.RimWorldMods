using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Schema;
using Zlepper.RimWorld.ModSdk.XsdOneOne;

namespace Zlepper.RimWorld.ModSdk.Utilities;

public class DefToSchemaConverter
{
    private readonly DefContext _defContext;
    private readonly IReadOnlyDictionary<Type, List<string>> _currentlyDefinedDefs;
    private readonly XsdSchema _schema;
    private readonly CustomStringTypes _customStringTypes;
    private readonly SchemaTypeSkipper _skipper;
    private readonly TimeMeasuringTool _timeMeasuringTool;

    public DefToSchemaConverter(DefContext defContext, IReadOnlyDictionary<Type, List<string>> currentlyDefinedDefs,
        TimeMeasuringTool? timeMeasuringTool = null)
    {
        _schema = new(rimWorldXmlNamespace);
        _defContext = defContext;
        _currentlyDefinedDefs = currentlyDefinedDefs;
        _timeMeasuringTool = timeMeasuringTool ?? new();
        _customStringTypes = new CustomStringTypes(_schema);
        _skipper = new SchemaTypeSkipper(defContext, _timeMeasuringTool);
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

    private IReadOnlyList<Type> _allTypes = null!;

    public XsdSchema CreateSchema(IEnumerable<Type> allTypes)
    {
        _allTypes = allTypes.ToList();

        var defTypes = _allTypes
            .Where(_defContext.IsDef)
            .ToList();

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
                    $"Detected possible stackoverflow when checking def {defType}\n{e.Message}",
                    e);
            }
        }

        return _schema;
    }


    private XsdType CreateXmlSchemaForClass(Type defType, int depth)
    {
        if (depth > 100)
            throw new UnwindingStackOverflowException("Dug too deep");

        if (_skipper.ShouldSkip(defType))
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

        if (!_defContext.IsDef(defType))
        {
            var subTypes = GetAllSubTypesOf(defType);

            if (subTypes.Count > 0)
            {
                var attributeType = GetClassPropertyAttributeType(subTypes, defType, depth + 1);

                var attr = new XsdAttribute(ClassAttributeName)
                {
                    SchemaTypeName = attributeType.Name,
                };

                if (type.ComplexContent?.Extension is { } ext)
                {
                    ext.Attributes.Add(attr);
                }
                else
                {
                    type.Attributes.Add(attr);
                }

                foreach (var subType in subTypes)
                {
                    foreach (var fieldInfo in GetDefFieldsForDefType(subType))
                    {
                        var fieldElement = GetFieldElement(fieldInfo, depth);
                        if (fieldElement == null)
                        {
                            continue;
                        }

                        fields.Elements.Add(fieldElement);
                    }
                }
            }
        }

        return type;
    }

    public const string ClassAttributeName = "Class";

    private XsdSimpleType GetClassPropertyAttributeType(IEnumerable<Type> possibleClasses, Type parentClass, int depth)
    {
        var typeName = GetTypeNameForXml(parentClass) + ".SubTypes";
        var existing = _schema.Types
            .OfType<XsdSimpleType>()
            .FirstOrDefault(t => t.Name == typeName);

        if (existing != null)
        {
            return existing;
        }

        var type = new XsdSimpleType()
        {
            Name = typeName,
            Restriction = new XsdRestriction()
        };

        _schema.Types.Add(type);

        type.Restriction.Facets =
            possibleClasses.Select(c =>
            {
                var possibleType = CreateXmlSchemaForClass(c, depth + 1);

                return new XsdEnumeration(possibleType.Name!);
            }).ToList<XsdFacet>();


        return type;
    }

    private string GetTypeNameForXml(Type defType)
    {
        return _defContext.GetDefElementName(defType);
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

                var concreteDefTypes = GetAllSubTypesOf(field.FieldType);

                foreach (var concreteDefType in concreteDefTypes)
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

    private readonly Dictionary<Type, bool> _hasCustomLoadMethodCache = new();

    private bool HasCustomLoadMethod(Type defType)
    {
        if (_hasCustomLoadMethodCache.TryGetValue(defType, out var c))
        {
            return c;
        }

        return _hasCustomLoadMethodCache[defType] = defType.GetMethod("LoadDataFromXmlCustom",
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) != null;
    }

    private readonly Dictionary<Type, bool> _hasCustomFromStringMethod = new();

    private bool HasCustomFromStringMethod(Type defType)
    {
        if (_hasCustomFromStringMethod.TryGetValue(defType, out var c))
        {
            return c;
        }

        return _hasCustomFromStringMethod[defType] = defType.GetMethod("FromString",
            BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic) != null;
    }


    private XsdElement? GetFieldElement(FieldInfo field, int depth)
    {
        var name = field.Name;

        if (field.FieldType.IsInterface)
        {
            return null;
        }

        if (_skipper.ShouldSkip(field))
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
        if (_skipper.ShouldSkip(field))
        {
            return null;
        }

        if (WellKnownFieldTypes.TryGetValue(type.FullName, out var wellKnownType))
        {
            return new XsdElement(field.Name)
            {
                SchemaTypeName = wellKnownType
            };
        }

        if (_defContext.IsDef(type))
        {
            var defOptionsType = CreateDefOptionsType(type);

            return new XsdElement(field.Name)
            {
                SchemaTypeName = defOptionsType.Name
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

            if (genericTypeDefinitionFullName.StartsWith("System.Predicate") ||
                genericTypeDefinitionFullName.StartsWith("System.Func") ||
                genericTypeDefinitionFullName.StartsWith("System.Action") ||
                genericTypeDefinitionFullName.StartsWith("System.Delegate"))
            {
                return null;
            }

            throw new NotImplementedException(
                $"Can't handle field {field.Name} of type {type} on class {field.DeclaringType}. Full field type: {field.FieldType}. Base base type: {type.BaseType}");
        }

        try
        {
            var schemaType = CreateXmlSchemaForClass(type, depth + 1);

            var el = new XsdElement(field.Name)
            {
                SchemaTypeName = schemaType.Name,
            };

            return el;
        }
        catch (UnwindingStackOverflowException e)
        {
            throw new UnwindingStackOverflowException(
                $"Detected possible stackoverflow: Can't handle field {field.Name} of type {type} on class {field.DeclaringType}. Full field type: {field.FieldType}. Base base type: {type.BaseType}\n{e.Message}",
                e);
        }
        catch (NotImplementedException e)
        {
            throw new NotImplementedException(
                $"Can't handle field {field.Name} of type {type} on class {field.DeclaringType}. Full field type: {field.FieldType}. Base base type: {type.BaseType}",
                e);
        }
    }

    private Dictionary<Type, HashSet<Type>>? _subTypeCache;

    private HashSet<Type> GetAllSubTypesOf(Type type)
    {
        if (_subTypeCache == null)
        {
            _subTypeCache = new(_allTypes.Count);

            foreach (var t in _allTypes)
            {
                if (_skipper.ShouldSkip(t))
                {
                    continue;
                }

                var next = t;
                while (next.BaseType != null && next.BaseType.FullName != typeof(object).FullName)
                {
                    if (_subTypeCache.TryGetValue(next.BaseType, out var e))
                    {
                        e.Add(t);
                    }
                    else
                    {
                        _subTypeCache[next.BaseType] = new HashSet<Type> {t};
                    }

                    next = next.BaseType;
                }
            }
        }


        if (_subTypeCache.TryGetValue(type, out var cached))
        {
            return cached;
        }

        return new HashSet<Type>();
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
            .Where(f => !_skipper.ShouldSkip(f))
            .ToList();
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