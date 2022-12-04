using System;
using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.XsdOneOne;

[XmlType("extension", Namespace = XmlSchema.Namespace)]
public sealed class XsdExtension : XsdNode
{
    [XmlAttribute("base")] public string Base;
    
    [XmlElement("choice", typeof(XsdChoiceGroup))]
    [XmlElement("all", typeof(XsdAllGroup))]
    [XmlElement("sequence", typeof(XsdSequenceGroup))]
    public XsdGroup? Properties;

    
    [XmlElement("attribute", typeof(XsdAttribute))]
    public List<XsdAttribute> Attributes = new();

    
    public XsdExtension(string @base)
    {
        Base = @base;
    }

    private XsdExtension()
    {
        Base = null!;
    }

    public override void Sort()
    {
        Properties?.Sort();
        
        Attributes.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.InvariantCultureIgnoreCase));
        foreach (var xsdAttribute in Attributes)
        {
            xsdAttribute.Sort();
        }
    }
}