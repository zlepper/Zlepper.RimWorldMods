using System.Xml.Schema;
using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.XsdOneOne;

[XmlType("element", Namespace = XmlSchema.Namespace)]
public class XsdElement : XsdSchemaItem 
{
    [XmlAttribute("name")]
    public string Name;

    [XmlAttribute("type")]
    public string? SchemaTypeName;
    
    [XmlElement("simpleType", typeof(XsdSimpleType))]
    [XmlElement("complexType", typeof(XsdComplexType))]
    public XsdType? SchemaType;

    public XsdElement(string name)
    {
        Name = name;
    }

    private XsdElement()
    {
        Name = null!;
    }
}