using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Zlepper.RimWorld.ModSdk.Utilities;

namespace Zlepper.RimWorld.ModSdk.Models;

public class VersionedXmlList<T> : IXmlSerializable
where T: class
{
    private readonly Dictionary<Version, List<T>> _values = new();

    public void Add(Version key, T value)
    {
        if (_values.TryGetValue(key, out var existing))
        {
            existing.Add(value);
        }
        else
        {
            _values[key] = new List<T> {value};
        }
    }

    public bool TryGet(Version key, out List<T> options)
    {
        return _values.TryGetValue(key, out options);
    }

    XmlSchema? IXmlSerializable.GetSchema()
    {
        return null;
    }

    void IXmlSerializable.ReadXml(XmlReader reader)
    {
        var doc = new XmlDocument();
        doc.Load(reader);

        var root = doc.FirstChild;
        
        
        for (var index = 0; index < root.ChildNodes.Count; index++)
        {
            var versionNode = root.ChildNodes[index];
            if (versionNode.Name.StartsWith("v") && Version.TryParse(versionNode.Name.Substring(1), out var version))
            {
                for (var i = 0; i < versionNode.ChildNodes.Count; i++)
                {
                    var liNode = versionNode.ChildNodes[i];
                    if (liNode.Name != "li")
                    {
                        throw new Exception("Found non <li> node");
                    }

                    if (typeof(T) == typeof(string))
                    {
                        var s = liNode.InnerText as T;
                        Add(version, s!);
                    }
                    else
                    {
                        var child = XmlUtilities.ConvertNodeContent<T>(liNode);
                        Add(version, child!);
                    }
                }
            }
            else
            {
                throw new Exception($"key '{versionNode.Name}' is not a valid version");
            }
        }

    }
    void IXmlSerializable.WriteXml(XmlWriter writer)
    {
        foreach (var pair in _values)
        {
            writer.WriteStartElement($"v{pair.Key}");
            
            foreach (var item in pair.Value)
            {
                writer.WriteStartElement("li");
                
                writer.WriteValue(item!);
                
                writer.WriteEndElement();
            }
            
            writer.WriteEndElement();
        }
    }

    protected bool Equals(VersionedXmlList<T> other)
    {
        if (_values.Count != other._values.Count)
        {
            return false;
        }
        
        foreach (var thisPair in _values)
        {
            if (other._values.TryGetValue(thisPair.Key, out var otherValue))
            {
                if (!thisPair.Value.SequenceEqual(otherValue))
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        
        return true;
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((VersionedXmlList<T>) obj);
    }

    public override int GetHashCode()
    {
        return _values.GetHashCode();
    }

    public override string ToString()
    {
        return string.Join("; ", _values.Select(p => $"{p.Key} = {string.Join(", ", p.Value)}"));
    }
}