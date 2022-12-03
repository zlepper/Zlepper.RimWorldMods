using System.Xml.Schema;
using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.XsdOneOne;

[XmlType(Namespace = XmlSchema.Namespace)]
public abstract class XsdNode
{
    [XmlElement("annotation")]
    public XsdAnnotation? Annotation;
}