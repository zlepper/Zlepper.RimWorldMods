using Zlepper.RimWorld.ModSdk.Utilities;
using Zlepper.RimWorld.ModSdk.Tests.Utilities.RimWorldVerseCopy;

namespace Zlepper.RimWorld.ModSdk.Tests.Utilities;

[TestFixture]
public class DefToSchemaConverterTests
{
    private readonly DefToSchemaConverter _converter = new(typeof(Def));

    private string GetSchemaFor(params Type[] types)
    {
        var schema = _converter.CreateSchema(types.ToList());
        return XmlUtilities.GetSchemaAsString(schema);
    }

    [Test]
    public Task GeneratesSchemaForSimpleDef()
    {
        var result = GetSchemaFor(typeof(Def), typeof(WorkTypeDef), typeof(SkillDef), typeof(InteractionDef), typeof(BackstoryDef), typeof(BodyTypeDef), typeof(DefWithWorkerClass));

        return VerifyXml(result).UseDirectory("../Snapshots");
    }
}