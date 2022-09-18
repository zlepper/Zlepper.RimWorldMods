using Verse;

namespace Zlepper.RimWorld.PersonalitySurgery;

public class CompThingLabel : ThingComp
{
    
    public override void DrawGUIOverlay()
    {
        if (props is not CompPropertiesLabel p)
        {
            return;
        }
        
        GenMapUI.DrawThingLabel(parent, p.Label);
    }
}

public class CompPropertiesLabel : CompProperties
{
    public string Label;
    
    
    public CompPropertiesLabel(string label)
    {
        Label = label;
        compClass = typeof(CompThingLabel);
    }
}