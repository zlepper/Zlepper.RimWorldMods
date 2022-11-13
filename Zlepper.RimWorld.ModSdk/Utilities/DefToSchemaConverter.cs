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


    public string CreateSchema(List<Type> defTypes)
    {
        var schema = new XmlSchema();

        var defsRoot = new XmlSchemaElement()
        {
            Name = "Defs"
        };
        var defsRootElementList = new XmlSchemaAll();
        defsRoot.SchemaType = new XmlSchemaComplexType()
        {
            Particle = defsRootElementList
        };
        schema.Items.Add(defsRoot);
        
        foreach (var defType in defTypes)
        {
            var type = new XmlSchemaComplexType();
            type.Name = defType.FullName;

            var elementName = defType.Namespace == _rootDefTypeClassNamespace ? defType.Name : type.Name;
            defsRootElementList.Items.Add(new XmlSchemaElement()
            {
                Name = elementName,
                MinOccurs = 0,
                MaxOccursString = "1",
                SchemaTypeName = type.QualifiedName,
            });
            schema.Items.Add(type);
        }

        return GetSchemaAsString(schema);
    }

    private string GetSchemaAsString(XmlSchema schema)
    {
        var schemaSet = new XmlSchemaSet();
        schemaSet.Add(schema);
        schemaSet.Compile();

        XmlSchema compiledSchema = null!;

        foreach (XmlSchema schema1 in schemaSet.Schemas())
        {
            compiledSchema = schema1;
        }

        var nsmgr = new XmlNamespaceManager(new NameTable());
        nsmgr.AddNamespace("xs", "http://www.w3.org/2001/XMLSchema");
        using var memStream = new StringWriter();
        using var xmlWriter = XmlWriter.Create(memStream, new XmlWriterSettings()
        {
            Encoding = Encoding.UTF8,
            Indent = true,
            NewLineChars = "\n"
        });
        compiledSchema.Write(xmlWriter, nsmgr);

        return memStream.ToString();
    }

    private readonly Dictionary<Type, List<FieldInfo>> _defTypeFieldCache = new();

    private List<FieldInfo> GetDefFieldsForDefType(Type defType)
    {
        if (_defTypeFieldCache.TryGetValue(defType, out var cachedResult))
        {
            return cachedResult;
        }

        var ownField = defType.GetFields(BindingFlags.Instance | BindingFlags.Public).ToList();
        
        _defTypeFieldCache[defType] = ownField;
        return ownField;
    }

}