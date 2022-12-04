using System;
using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.XsdOneOne;

[XmlType("restriction", Namespace = XmlSchema.Namespace)]
public class XsdRestriction : XsdNode
{
    [XmlAttribute("base")]
    public string Base = "xs:string";
    
    [XmlElement("pattern", typeof(XsdPattern))]
    [XmlElement("enumeration", typeof(XsdEnumeration))]
    public List<XsdFacet> Facets = new();

    public override void Sort()
    {
        Facets.Sort((a, b) => string.Compare(a.Value, b.Value, StringComparison.InvariantCultureIgnoreCase));
    }
}