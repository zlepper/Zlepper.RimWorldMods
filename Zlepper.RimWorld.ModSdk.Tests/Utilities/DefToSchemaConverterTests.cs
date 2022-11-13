using System;
using System.Collections.Generic;
using Zlepper.RimWorld.ModSdk.Utilities;

namespace Zlepper.RimWorld.ModSdk.Tests.Utilities;

[TestFixture]
public class DefToSchemaConverterTests
{
    private DefToSchemaConverter _converter = new DefToSchemaConverter(typeof(MyDefRoot));
    
    [Test]
    public void GeneratesSchemaForSimpleDef()
    {
        var result = _converter.CreateSchema(new List<Type>{typeof(SimpleChildDef)});


        //language=xsd
        var expected = """
<?xml version="1.0" encoding="utf-16"?>
<xs:schema xmlns:xs="http://www.w3.org/2001/XMLSchema">

    <xs:element name="Defs">
        <xs:complexType>
            <xs:all>
                <xs:element name="MyDefRoot" type="Zlepper.RimWorld.ModSdk.Tests.Utilities.MyDefRoot"
                            minOccurs='0' maxOccurs='unbounded'/>
                <xs:element name="SimpleChildDef" type="Zlepper.RimWorld.ModSdk.Tests.Utilities.SimpleChildDef"
                            minOccurs='0' maxOccurs='unbounded'/>
            </xs:all>
        </xs:complexType>
    </xs:element>

    <xs:complexType name="Zlepper.RimWorld.ModSdk.Tests.Utilities.MyDefRoot">
        <xs:all>
            <xs:element name='testField' type='xs:string'/>
        </xs:all>
    </xs:complexType>

    <xs:complexType name="Zlepper.RimWorld.ModSdk.Tests.Utilities.SimpleChildDef">
        <xs:complexContent>
            <xs:extension base="Zlepper.RimWorld.ModSdk.Tests.Utilities.MyDefRoot">
                <xs:all>
                    <xs:element name='intField' type='xs:integer'/>
                </xs:all>
            </xs:extension>
        </xs:complexContent>
    </xs:complexType>

</xs:schema>
""";
        
        Assert.That(result, Is.EqualTo(expected));
    }
}

public class MyDefRoot
{
    public string testField = null!;
}

public class SimpleChildDef : MyDefRoot
{
    public int intField;
}