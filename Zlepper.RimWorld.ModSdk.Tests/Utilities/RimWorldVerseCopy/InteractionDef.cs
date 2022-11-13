namespace Zlepper.RimWorld.ModSdk.Tests.Utilities.RimWorldVerseCopy;

public class InteractionDef : Def
{
    public float socialFightBaseChance;

    public InteractionDetails details = null!;
}

public class InteractionDetails
{
    public string stringDetails = null!;
    public double doubleDetails;
}