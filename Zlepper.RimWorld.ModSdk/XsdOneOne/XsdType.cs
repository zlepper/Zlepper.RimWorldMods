using System.Xml.Schema;
using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.XsdOneOne;

[XmlType(Namespace = XmlSchema.Namespace)]
public abstract class XsdType : XsdNode
{
    [XmlAttribute("name")]
    public string? Name;
}