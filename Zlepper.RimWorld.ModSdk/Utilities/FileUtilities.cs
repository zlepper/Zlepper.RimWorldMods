using System.IO;

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

}