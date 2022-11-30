using System.Xml.Linq;

namespace Zlepper.RimWorld.ModSdk.Utilities;

public static class XmlSchemaAdder
{
    public static bool AddRimWorldNamespace(XDocument document, string schemaPath)
    {
        var modified = false;
        if (document.Root == null || document.Root.Name.LocalName != "Defs") return modified;
        
        XNamespace rimWorldNamespace = "rimworld";
        if (document.Root.Name.Namespace != rimWorldNamespace)
        {
            modified = true;
            document.Root.Name = rimWorldNamespace + document.Root.Name.LocalName;
            
            foreach (var xElement in document.Descendants())
            {
                xElement.Name = rimWorldNamespace + xElement.Name.LocalName;
            }
        }
        

        XNamespace xsiNamespace = "http://www.w3.org/2001/XMLSchema-instance";
        var xsiNamespaceAttribute = document.Root.Attribute(xsiNamespace + "xsi");
        if (xsiNamespaceAttribute == null || xsiNamespaceAttribute.Value != xsiNamespace)
        {
            modified = true;
            document.Root.SetAttributeValue(XNamespace.Xmlns + "xsi", xsiNamespace);
        }
        
        var schemaLocationAttribute = document.Root.Attribute(xsiNamespace + "schemaLocation");
        var value = $"rimworld {schemaPath}";
        if (schemaLocationAttribute == null || schemaLocationAttribute.Value != value)
        {
            modified = true;
            document.Root.SetAttributeValue(xsiNamespace + "schemaLocation", value);
        }

        return modified;
    }
}