using System.Xml.Schema;
using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.XsdOneOne;

[XmlType("extension", Namespace = XmlSchema.Namespace)]
public class XsdExtension : XsdNode
{
    [XmlAttribute("base")] public string Base;
    
    [XmlElement("choice", typeof(XsdChoiceGroup))]
    [XmlElement("all", typeof(XsdAllGroup))]
    [XmlElement("sequence", typeof(XsdSequenceGroup))]
    public XsdGroup? Properties;

    public XsdExtension(string @base)
    {
        Base = @base;
    }

    private XsdExtension()
    {
        Base = null!;
    }
}