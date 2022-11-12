using System.Collections.Generic;
using System.Xml.Serialization;

namespace Zlepper.RimWorld.ModSdk.Models;

public class RimWorldXmlList<T>
{
    [XmlElement("li")] public List<T> ListItems { get; set; } = new();

    public void Add(T item)
    {
        ListItems.Add(item);
    }

    public void AddMissing(IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            if (!ListItems.Contains(item))
            {
                ListItems.Add(item);
            }
        }
    }
}

public static class RimWorldXmlListExtensions
{
    public static RimWorldXmlList<T> ToRimWorldXmlList<T>(this IEnumerable<T> enumerable)
    {
        var list = new RimWorldXmlList<T>();
        foreach (var item in enumerable)
        {
            list.Add(item);
        }

        return list;
    }
}