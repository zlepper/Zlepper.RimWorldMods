using System.Xml.Schema;
using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.XsdOneOne;

[XmlType("any", Namespace = XmlSchema.Namespace)]
public class XsdAny : XsdSchemaItem
{
    [XmlAttribute("processContents")]
    public XmlSchemaContentProcessing ProcessContents = XmlSchemaContentProcessing.Skip;

    public override void Sort()
    {
        
    }

    public override string GetSortKey()
    {
        return "";
    }
}