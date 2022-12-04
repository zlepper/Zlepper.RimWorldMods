using System;
using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.XsdOneOne;

[XmlType("element", Namespace = XmlSchema.Namespace)]
public class XsdElement : XsdSchemaItem
{
    [XmlAttribute("name")] public string Name;

    [XmlAttribute("type")] public string? SchemaTypeName;

    [XmlElement("simpleType", typeof(XsdSimpleType))] [XmlElement("complexType", typeof(XsdComplexType))]
    public XsdType? SchemaType;

    [XmlElement("alternative")] public List<XsdAlternative> Alternatives = new();

    public XsdElement(string name)
    {
        Name = name;
    }

    private XsdElement()
    {
        Name = null!;
    }

    public override void Sort()
    {
        Alternatives.Sort((a, b) => string.Compare(a.Type, b.Type, StringComparison.InvariantCultureIgnoreCase));
        foreach (var alternative in Alternatives)
        {
            alternative.Sort();
        }
        
        SchemaType?.Sort();
    }

    public override string GetSortKey()
    {
        return Name;
    }
}

[XmlType("attribute", Namespace = XmlSchema.Namespace)]
public class XsdAttribute : XsdNode
{
    [XmlAttribute("name")] public string Name;

    [XmlAttribute("type")] public string? SchemaTypeName;

    [XmlElement("simpleType", typeof(XsdSimpleType))]
    public XsdSimpleType? SchemaType;

    public XsdAttribute(string name)
    {
        Name = name;
    }

    private XsdAttribute()
    {
        Name = null!;
    }
    
    
    public override void Sort()
    {
        SchemaType?.Sort();
    }
}