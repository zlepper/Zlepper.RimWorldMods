using System.Xml.Schema;
using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.XsdOneOne;

[XmlType("annotation", Namespace = XmlSchema.Namespace)]
public class XsdAnnotation
{
    [XmlElement("documentation")]
    public string? Documentation;
}