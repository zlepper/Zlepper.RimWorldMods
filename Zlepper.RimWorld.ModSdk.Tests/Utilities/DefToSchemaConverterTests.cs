using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using Zlepper.RimWorld.ModSdk.Utilities;
using VerifyNUnit;

namespace Zlepper.RimWorld.ModSdk.Tests.Utilities;

[TestFixture]
public class DefToSchemaConverterTests
{
    private DefToSchemaConverter _converter = new DefToSchemaConverter(typeof(Def));

    private string GetSchemaFor(params Type[] types)
    {
        var schema = _converter.CreateSchema(new List<Type> {typeof(WorkTypeDef), typeof(Def)});
        return GetSchemaAsString(schema);
    }

    [Test]
    public Task GeneratesSchemaForSimpleDef()
    {
        var result = GetSchemaFor(typeof(Def), typeof(WorkTypeDef));

        return VerifyXml(result).UseDirectory("../Snapshots");
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
        nsmgr.AddNamespace("xs", DefToSchemaConverter.XMLSchemaNamespace);
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
}

public class Def
{
    
    [Description("The name of this Def. It is used as an identifier by the game code.")]
    public string defName = "UnnamedDef";
    
    [Description("A human-readable label used to identify this in game.")]
    public string label = null!;
    public string testField = null!;
    
    [Unsaved(false)]
    public string fileName = null!;
}

public class WorkTypeDef : Def
{
    public int intField;
    
    public WorkTags workTags;
}

[AttributeUsage(AttributeTargets.Field)]
public class DescriptionAttribute : Attribute
{
    public string description;

    public DescriptionAttribute(string description)
    {
        this.description = description;
    }
}
[AttributeUsage(AttributeTargets.Field)]
public class UnsavedAttribute : Attribute
{
    
    public bool allowLoading;

    public UnsavedAttribute(bool allowLoading = false) => this.allowLoading = allowLoading;
}

[Flags]
public enum WorkTags
{
    None = 0,
    ManualDumb = 2,
    ManualSkilled = 4,
}