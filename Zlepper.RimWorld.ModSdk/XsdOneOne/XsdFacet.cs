using System.Xml.Schema;
using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.XsdOneOne;

[XmlType(Namespace = XmlSchema.Namespace)]
public abstract class XsdFacet : XsdNode
{
    [XmlAttribute("value")]
    public string Value;

    protected XsdFacet(string value)
    {
        Value = value;
    }

    protected XsdFacet()
    {
        Value = null!;
    }
}