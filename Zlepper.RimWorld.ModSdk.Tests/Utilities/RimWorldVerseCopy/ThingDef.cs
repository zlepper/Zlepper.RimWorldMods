namespace Zlepper.RimWorld.ModSdk.Tests.Utilities.RimWorldVerseCopy;

public class ThingDef : Def
{
    
    public List<CompProperties> comps = new();
}

public abstract class ThingComp
{
    
}

public class CompProperties
{
    public Type compClass = typeof(ThingComp);
}

public class CompProperties_Foo : CompProperties
{
    public string SomeValue = null!;

    public Action DoSomething = null!;
}

public class CompProperties_FooFoo : CompProperties_Foo
{
    public int AnotherValue;
}