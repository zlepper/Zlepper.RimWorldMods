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
}