using System.Xml.Schema;
using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.XsdOneOne;

[XmlType("pattern", Namespace = XmlSchema.Namespace)]
public class XsdPattern : XsdFacet
{
    public XsdPattern(string value) : base(value)
    {
    }

    private XsdPattern()
    {
        
    }

    public override void Sort()
    {
        
    }
}