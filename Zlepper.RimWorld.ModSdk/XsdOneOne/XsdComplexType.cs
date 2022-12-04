using System;
using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.XsdOneOne;

[XmlType("complexType", Namespace = XmlSchema.Namespace)]
public class XsdComplexType : XsdType
{
    [XmlElement("choice", typeof(XsdChoiceGroup))]
    [XmlElement("all", typeof(XsdAllGroup))]
    [XmlElement("sequence", typeof(XsdSequenceGroup))]
    public XsdGroup? Properties;

    [XmlElement("complexContent")] public XsdComplexContent? ComplexContent;

    [XmlElement("attribute", typeof(XsdAttribute))]
    public List<XsdAttribute> Attributes = new();

    public override void Sort()
    {
        ComplexContent?.Sort();
        
        Properties?.Sort();
        Attributes.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.InvariantCultureIgnoreCase));
        foreach (var xsdAttribute in Attributes)
        {
            xsdAttribute.Sort();
        }
    }
}