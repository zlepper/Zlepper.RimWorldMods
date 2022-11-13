namespace Zlepper.RimWorld.ModSdk.Tests.Utilities.RimWorldVerseCopy;

public class Def
{
    
    [Description("The name of this Def. It is used as an identifier by the game code.")]
    public string defName = "UnnamedDef";
    
    [Description("A human-readable label used to identify this in game.")]
    public string label = null!;
    public string testField = null!;
    
    [Unsaved(false)]
    public string fileName = null!;
}