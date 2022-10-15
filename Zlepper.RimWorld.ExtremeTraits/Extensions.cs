using System.Collections;
using System.Reflection;
using System.Text;
using HugsLib.Utils;

namespace Zlepper.RimWorld.ExtremeTraits;

internal static class Extensions
{
    internal static IEnumerable<T> NotNull<T>(this IEnumerable<T?> source)
    {
        foreach (var item in source)
        {
            if (item != null)
            {
                yield return item;
            }
        }
    }

    internal static void DumpObject<T>(this ModLogger logger, string message, T o)
    {
        if (o == null)
        {
            logger.Trace(message + " null");
            return;
        }

        var s = new StringBuilder();
        s.AppendLine(o.GetType().ToString());
        
        foreach (var field in o.GetType().GetFields(BindingFlags.Instance|BindingFlags.Public))
        {
            var value = field.GetValue(o);
            s.AppendLine($"{field.Name}: {value}");

            if (value is ICollection col)
            {
                s.AppendLine("Count: " + col.Count);
                foreach (var inner in col)
                {
                    foreach (var innerField in inner.GetType().GetFields(BindingFlags.Instance|BindingFlags.Public))
                    {
                        var innerValue = innerField.GetValue(inner);
                        s.AppendLine($"  {innerField.Name}: {innerValue}");
                    }
                }
            }
        }
        
        
        logger.Trace(message + " " + s);
    }
}