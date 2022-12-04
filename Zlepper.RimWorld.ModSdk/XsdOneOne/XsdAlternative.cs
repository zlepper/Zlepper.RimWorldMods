using System;
using System.Text.RegularExpressions;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.XsdOneOne;

[XmlType("alternative", Namespace = XmlSchema.Namespace)]
public class XsdAlternative : XsdNode
{
    [XmlAttribute("type")]
    public string Type;

    [XmlAttribute("test")]
    public string Test
    {
        get => _test.ToValue();
        set => _test = XsdAlternativeTest.FromValue(value);
    }

    [XmlIgnore]
    private XsdAlternativeTest _test;
    
    public XsdAlternative(string type, XsdAlternativeTest test)
    {
        Type = type;
        _test = test;
    }
    
    private XsdAlternative()
    {
        Type = null!;
        _test = null!;
    }

    public override void Sort()
    {
    }
}

public abstract class XsdAlternativeTest
{
    public abstract string ToValue();

    public static XsdAlternativeTest FromValue(string value)
    {
        return XsdAlternativeTestAttributeEquals.FromValue(value) ?? throw new ArgumentException($"Invalid value for xsd alternative test: `{value}`", nameof(value));
    }
}

public class XsdAlternativeTestAttributeEquals : XsdAlternativeTest
{
    public string AttributeName;
    public string ExpectedValue;
    
    public XsdAlternativeTestAttributeEquals(string attributeName, string expectedValue)
    {
        AttributeName = attributeName;
        ExpectedValue = expectedValue;
    }
    
    public override string ToValue() => $"@{AttributeName}='{ExpectedValue}'";
    
    public new static XsdAlternativeTestAttributeEquals? FromValue(string value)
    {
        var match = Regex.Match(value, "^@(?<attributeName>.+?)='(?<expectedValue>.+?)'$");


        if (!match.Success)
        {
            return null;
        }

        var attributeName = match.Groups["attributeName"].Value;
        var expectedValue = match.Groups["expectedValue"].Value;

        return new XsdAlternativeTestAttributeEquals(attributeName, expectedValue);
    }
}