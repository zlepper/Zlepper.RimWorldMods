using System.Xml;

namespace Zlepper.RimWorld.ModSdk.Tests.Utilities.RimWorldVerseCopy;

public class StatModifier
{
    public SkillDef stat = null!;
    public float value;

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
    }
}