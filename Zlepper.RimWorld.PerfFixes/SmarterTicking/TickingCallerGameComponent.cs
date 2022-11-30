namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

public class TickingCallerGameComponent : GameComponent
{
    public TickingCallerGameComponent(Game game)
    {
        
    }
    
    public override void GameComponentTick()
    {
        WorldPawns_GetSituation_Patches.Ticker.Tick();
        Toils_BedPatches.Ticker.Tick();
        RestUtility_ShouldWakeUp_Patches.Ticker.Tick();
        ForbidUtility_CaresAboutForbidden_Patches.Tick();
    }

    public override void StartedNewGame()
    {
        LoadedGame();
    }

    public override void LoadedGame()
    {
        WorldPawns_GetSituation_Patches.Ticker.Reset();
        Toils_BedPatches.Ticker.Reset();
        RestUtility_ShouldWakeUp_Patches.Ticker.Reset();
        ForbidUtility_CaresAboutForbidden_Patches.Reset();
    }
}