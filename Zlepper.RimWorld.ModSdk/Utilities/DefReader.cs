using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Build.Utilities;

namespace Zlepper.RimWorld.ModSdk.Utilities;

public class DefReader
{
    private readonly Dictionary<string, List<Type>> _defTypesByDefElementName;
    private readonly TimeMeasuringTool _timeMeasuringTool = new();

    public DefReader(IReadOnlyCollection<Type> allTypes, DefContext defContext)
    {
        var defClasses = allTypes.Where(defContext.IsDef).ToList();
        
        _defTypesByDefElementName = new Dictionary<string, List<Type>>();
        
        foreach (var defClass in defClasses)
        {
            var elementName = defContext.GetDefElementName(defClass);

            _defTypesByDefElementName[elementName] = defContext
                .GetBaseTypes(defClass)
                .TakeWhile(t => !defContext.IsRootDefClass(t))
                .Append(defClass)
                .ToList();
        }
    }

    public void Dump(TaskLoggingHelper logger)
    {
        _timeMeasuringTool.Dump(logger);
    }

    public IReadOnlyDictionary<Type, List<string>> ReadAllDefFiles(IEnumerable<string> files, TaskLoggingHelper log)
    {
        var result = new Dictionary<Type, List<string>>();
        
        foreach (var file in files)
        {
            try
            {
                var xml = File.ReadAllText(file);

                var items = ParseDefContent(xml);

                foreach (var pair in items)
                {
                    if (result.TryGetValue(pair.Key, out var existing))
                    {
                        existing.AddRange(pair.Value);
                    }
                    else
                    {
                        result[pair.Key] = pair.Value;
                    }
                }
            }
            catch (Exception e)
            {
                log.LogWarning("Exception while reading def file: " + file + ". " + e);
            }
        }
        
        
        return result;
    }

    public IReadOnlyDictionary<Type, List<string>> ParseDefContent(string defXmlFile)
    {
        var document = XDocument.Parse(defXmlFile);

        var element = document.Root;
        
        
        if (element?.Name.LocalName != "Defs")
        {
            return ImmutableDictionary<Type, List<string>>.Empty;
        }

        var result = new Dictionary<Type, List<string>>();

        
        foreach (var defElement in element.Descendants())
        {
            if (defElement.NodeType == XmlNodeType.Element)
            {
                var defName = defElement.Element("defName")?.Value;
                
                if(string.IsNullOrEmpty(defName))
                    continue;

                var defElementName = defElement.Name.LocalName;
                if (_defTypesByDefElementName.TryGetValue(defElementName, out var defTypes))
                {
                    foreach (var defType in defTypes)
                    {
                        if (result.TryGetValue(defType, out var defNames))
                        {
                            defNames.Add(defName!);
                        }
                        else
                        {
                            result[defType] = new List<string> {defName!};
                        }
                    }
                }
            }
        }


        return result;

    }
}