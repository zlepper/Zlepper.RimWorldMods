namespace Zlepper.RimWorld.ModSdk.Tests.Utilities.RimWorldVerseCopy;

[AttributeUsage(AttributeTargets.Field)]
public class DescriptionAttribute : Attribute
{
    public string description;

    public DescriptionAttribute(string description)
    {
        this.description = description;
    }
}