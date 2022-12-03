using System.ComponentModel;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.XsdOneOne;

[XmlType(Namespace = XmlSchema.Namespace)]
public abstract class XsdSchemaItem : XsdNode
{
    [DefaultValue(Occurs.One)]
    [XmlAttribute("minOccurs")]
    public Occurs MinOccurs = Occurs.One;

    [DefaultValue(Occurs.One)]
    [XmlAttribute("maxOccurs")]
    public Occurs MaxOccurs = Occurs.One;
}