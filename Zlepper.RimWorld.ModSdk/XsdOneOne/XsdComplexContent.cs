using System.Xml.Schema;
using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.XsdOneOne;

[XmlType("complexContent", Namespace = XmlSchema.Namespace)]
public class XsdComplexContent : XsdNode
{
    [XmlElement("extension")] public XsdExtension? Extension;
}