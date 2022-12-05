using System.Xml.Schema;
using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.XsdOneOne;

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