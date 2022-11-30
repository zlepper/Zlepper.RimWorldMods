using System;
using System.IO;
using System.Text;
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

                var document = XDocument.Load(fullPath, LoadOptions.PreserveWhitespace | LoadOptions.SetBaseUri | LoadOptions.SetLineInfo);
                var modified = XmlSchemaAdder.AddRimWorldNamespace(document, schemaPath);

                if (modified)
                {
                    var result = document.ToString(SaveOptions.OmitDuplicateNamespaces);
                    File.WriteAllText(fullPath, result, Encoding.UTF8);
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