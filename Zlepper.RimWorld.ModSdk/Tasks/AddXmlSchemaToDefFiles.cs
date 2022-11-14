using System;
using System.IO;
using System.Xml.Linq;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;
using Zlepper.RimWorld.ModSdk.Utilities;

namespace Zlepper.RimWorld.ModSdk.Tasks;

public class AddXmlSchemaToDefFiles : Task
{
    [Required] public ITaskItem[] DefFiles { get; set; } = null!;

    [Required]
    public string SchemaLocation { get; set; } = null!;
    
    
    public override bool Execute()
    {
        var schemaLocationFullPath = Path.GetFullPath(SchemaLocation);
        
        foreach (var taskItem in DefFiles)
        {
            var fullPath = taskItem.GetMetadata("FullPath");
            var schemaPath = FileUtilities.GetRelativePath(fullPath, schemaLocationFullPath);
            try
            {

                var modified = false;
                var document = XDocument.Load(fullPath, LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri | LoadOptions.SetLineInfo);
                if (document.Root != null)
                {
                    var namespaceAttribute = document.Root.Attribute("xmlns");
                    if (namespaceAttribute == null || namespaceAttribute.Value != "rimworld")
                    {
                        modified = true;
                        document.Root.SetAttributeValue("xmlns", "rimworld");
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

                    if (modified)
                    {
                        document.Save(fullPath);
                    }
                }
            }
            catch (Exception e)
            {
                Log.LogWarning($"Failed to add schema to def file '{fullPath}': {e}");
            }
        }

        return !Log.HasLoggedErrors;
    }

}