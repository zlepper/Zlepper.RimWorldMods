using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.XsdOneOne;

[XmlRoot("schema", Namespace = XmlSchema.Namespace)]
public class XsdSchema
{
    [XmlElement("element")]
    public List<XsdElement> Elements = new();
    
    [XmlElement("simpleType", typeof(XsdSimpleType))]
    [XmlElement("complexType", typeof(XsdComplexType))]
    public List<XsdType> Types = new();
    
    [XmlAttribute("elementFormDefault")]
    public XmlSchemaForm ElementFormDefault = XmlSchemaForm.Qualified;

    [XmlAttribute("targetNamespace")]
    public string TargetNamespace;

    public XsdSchema(string targetNamespace)
    {
        TargetNamespace = targetNamespace;
    }

    private XsdSchema()
    {
        TargetNamespace = null!;
    }
}