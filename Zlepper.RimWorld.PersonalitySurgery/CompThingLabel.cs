namespace Zlepper.RimWorld.PersonalitySurgery;

public class CompThingLabel : ThingComp
{
    public override void DrawGUIOverlay()
    {
        if (Find.CameraDriver.CurrentZoom != CameraZoomRange.Closest)
            return;

        GenMapUI.DrawThingLabel(parent, parent.LabelCap);
    }
}

public class CompPropertiesLabel : CompProperties
{
    public CompPropertiesLabel()
    {
        compClass = typeof(CompThingLabel);
    }
}