using Zlepper.RimWorld.ModSdk.Utilities;
using Zlepper.RimWorld.ModSdk.Tests.Utilities.RimWorldVerseCopy;

namespace Zlepper.RimWorld.ModSdk.Tests.Utilities;

[TestFixture]
public class DefToSchemaConverterTests
{
    private static readonly List<Type> _allDefTypes = new()
    {
        typeof(Def), typeof(WorkTypeDef), typeof(SkillDef), typeof(InteractionDef), typeof(BackstoryDef),
        typeof(BodyTypeDef), typeof(DefWithWorkerClass)
    };
    
    [Test]
    public async Task GeneratesSchemaForSimpleDef()
    {
        
        var defContext = new DefContext(typeof(Def));
        var _defReader = new DefReader(_allDefTypes, defContext);
        
        var defContent = await File.ReadAllTextAsync("Utilities/RimWorldVerseCopy/SampleDefFile.xml");
        var defined = _defReader.ParseDefContent(defContent);

        await Verify(defined).UseDirectory("../Snapshots").UseMethodName("ParseDefContent");
        
        
        var _converter = new DefToSchemaConverter(defContext, defined);

        var schema = _converter.CreateSchema(_allDefTypes);
        var result = XmlUtilities.GetSchemaAsString(schema);

        await VerifyXml(result).UseDirectory("../Snapshots").UseMethodName("CreateSchema");
    }
}