using System.Runtime.CompilerServices;
using DiffEngine;

namespace Zlepper.RimWorld.ModSdk.Tests;

public class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        DiffRunner.Disabled = true;
    }
}