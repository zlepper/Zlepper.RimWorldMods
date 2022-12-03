using System.Collections;
using System.Reflection;

namespace Zlepper.RimWorld.PersonalitySurgery;

public static class DefGeneratorHelpers
{
    public static void HyperlinkAll(params Def[] defs)
    {
        for (var i = 0; i < defs.Length; i++)
        {
            var defI = defs[i];
            defI.descriptionHyperlinks ??= new List<DefHyperlink>();
            for (var j = 0; j < defs.Length; j++)
            {
                if (i == j)
                {
                    continue;
                }

                defI.descriptionHyperlinks.Add(new DefHyperlink(defs[j]));
            }
        }
    }


    /// <summary>
    /// Creates a shallow copy of the specified element for further modification
    /// </summary>
    public static T CreateCopy<T>(Def from)
        where T : Def, new()
    {
        if (from == null) throw new ArgumentNullException(nameof(from));

        var fromType = from.GetType();
        if (!fromType.IsAssignableFrom(typeof(T)))
        {
            throw new ArgumentException($"type of {typeof(T)} cannot be generated from {fromType}");
        }

        var newInstance = new T();

        foreach (var field in fromType.GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            if (field.Name == nameof(Def.shortHash))
            {
                continue;
            }

            if (field.IsLiteral || field.IsInitOnly)
            {
                continue;
            }

            var value = field.GetValue(from);
            if (value is IList l)
            {
                var copy = (IList) Activator.CreateInstance(l.GetType());
                foreach (var item in l)
                {
                    copy.Add(item);
                }

                field.SetValue(newInstance, copy);
            }
            else
            {
                field.SetValue(newInstance, value);
            }
        }

        if (newInstance is BuildableDef newBuildable && from is BuildableDef fromBuildable)
        {
            newBuildable.statBases = fromBuildable.statBases.Select(s => new StatModifier()
            {
                stat = s.stat,
                value = s.value
            }).ToList();
        }

        return newInstance;
    }


    public static void RemoveFromDatabase<T>(T def)
        where T : Def
    {
        const string removeMethodName = "Remove";
        var removeMethod =
            typeof(DefDatabase<T>).GetMethod(removeMethodName, BindingFlags.Static | BindingFlags.NonPublic);
        if (removeMethod == null)
        {
            throw new MissingMethodException(typeof(DefDatabase<T>).FullName, removeMethodName);
        }

        removeMethod.Invoke(null, new object[] {def});
    }
}