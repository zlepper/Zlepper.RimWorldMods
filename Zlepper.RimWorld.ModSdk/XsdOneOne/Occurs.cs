using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.XsdOneOne;

public enum Occurs : uint
{
    [XmlEnum("0")]
    Zero = 0,
    [XmlEnum("1")]
    One = 1,
    [XmlEnum("unbounded")]
    Unbounded = uint.MaxValue,
}