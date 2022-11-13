namespace Zlepper.RimWorld.ModSdk.Tests.Utilities.RimWorldVerseCopy;


public class BodyTypeDef : Def
{
    public List<WoundAnchor> woundAnchors = null!;

    public class WoundAnchor
    {
        public string tag = null!;
    }
}