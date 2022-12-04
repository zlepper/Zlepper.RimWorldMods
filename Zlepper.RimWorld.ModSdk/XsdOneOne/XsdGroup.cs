using System;
using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.XsdOneOne;

[XmlType(Namespace = XmlSchema.Namespace)]
public abstract class XsdGroup : XsdSchemaItem
{
    [XmlElement("element", typeof(XsdElement))]
    [XmlElement("any", typeof(XsdAny))]
    public List<XsdSchemaItem> Elements = new();

    public override void Sort()
    {
        foreach (var xsdSchemaItem in Elements)
        {
            xsdSchemaItem.Sort();
        }
    }

    protected void SortElements()
    {
        Elements.Sort((a, b) => string.Compare(a.GetSortKey(), b.GetSortKey(), StringComparison.InvariantCultureIgnoreCase));
    }

    public override string GetSortKey()
    {
        return "";
    }
}