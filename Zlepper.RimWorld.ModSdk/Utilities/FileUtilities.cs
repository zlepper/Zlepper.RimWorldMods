using System;
using System.IO;
using System.Linq;

namespace Zlepper.RimWorld.ModSdk.Utilities;

public static class FileUtilities
{
    
    public static void EnsureDirectory(string directoryName)
    {
        if (!Directory.Exists(directoryName))
        {
            Directory.CreateDirectory(directoryName);
        }
    }
    
    
    public static string GetRelativePath(string from, string to)
    {
        var fromParts = from.Split('/', '\\');
        var toParts = to.Split('/', '\\');


        var startPosition = 0;
        while (startPosition < fromParts.Length && startPosition < toParts.Length && fromParts[startPosition] == toParts[startPosition])
        {
            startPosition++;
        }

        var remainingFrom = fromParts.Skip(startPosition).ToList();
        var remainingTo = toParts.Skip(startPosition).ToList();

        var upCount = remainingFrom.Count - 1;

        var toName = remainingTo.LastOrDefault() ?? "";


        return string.Join("/", Enumerable.Repeat("..", upCount).Concat(remainingTo.Take(remainingTo.Count - 1)).Concat(new[] { toName }));
    }

}