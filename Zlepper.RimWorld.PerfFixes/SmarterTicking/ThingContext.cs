namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

public class ThingContext
{
    private static ThingContext?[] _things = new ThingContext[10_000_000];


    public readonly int ThingIDNumber;



    private ThingContext(int thingIDNumber)
    {
        ThingIDNumber = thingIDNumber;
    }

    public static ThingContext Get(Thing i)
    {
        var id = i.thingIDNumber;
        if (id >= _things.Length)
        {
            var old = _things;
            _things = new ThingContext[id * 2];
            Array.Copy(old, _things, old.Length);
        }
        
        var context = _things[id];
        if (context == null)
        {
            _things[id] = context = new(id);
        }

        return context;
    }
    
    
    
    public bool? IsOverseerSubject;
}