using System.Xml.Schema;
using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.XsdOneOne;

[XmlType("enumeration", Namespace = XmlSchema.Namespace)]
public class XsdEnumeration : XsdFacet
{
    public XsdEnumeration(string value) : base(value)
    {
    }

    private XsdEnumeration()
    {
    }
}