using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Zlepper.RimWorld.ModSdk.Utilities;

public class DescriptionTrimmer
{
    
    public static string TrimDescription(string original)
    {
        var sb = new StringBuilder(original.Length);

        var lines = original.Split('\n')
            .Select(l => l.TrimEnd('\n', '\r'))
            .ToList();

        while (lines.Count > 0)
        {
            var firstLine = lines[0];
            if (string.IsNullOrWhiteSpace(firstLine))
            {
                lines.RemoveAt(0);
            }
            else
            {
                break;
            }
        }
        
        while (lines.Count > 0)
        {
            var lastLineIndex = lines.Count - 1;
            var lastLine = lines[lastLineIndex];
            if (string.IsNullOrWhiteSpace(lastLine))
            {
                lines.RemoveAt(lastLineIndex);
            }
            else
            {
                break;
            }
        }


        if (lines.Count == 0)
        {
            return string.Empty;
        }

        var indentLevel = int.MaxValue;
        
        
        
        
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var match = Regex.Match(line, @"^\s*");
            indentLevel = Math.Min(indentLevel, match.Length);
        }
        
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                sb.AppendLine();
                continue;
            }

            sb.AppendLine(line.Substring(indentLevel));
        }


        return sb.ToString().TrimEnd('\n', '\r');
    }

}