namespace Zlepper.RimWorld.ModSdk.Tests.Utilities.RimWorldVerseCopy;

public class SkillDef : Def
{
    public string skillLabel = null!;
    public bool usuallyDefinedInBackstories = true;
    public bool pawnCreatorSummaryVisible;
    public WorkTags disablingWorkTags;
    
    public InteractionDef lessonInteraction = null!;
}