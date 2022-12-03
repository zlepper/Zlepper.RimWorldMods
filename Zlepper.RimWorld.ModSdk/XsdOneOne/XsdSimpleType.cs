using System.Xml.Schema;
using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.XsdOneOne;

[XmlType("simpleType", Namespace = XmlSchema.Namespace)]
public class XsdSimpleType : XsdType
{
    [XmlElement("restriction")]
    public XsdRestriction? Restriction;
}