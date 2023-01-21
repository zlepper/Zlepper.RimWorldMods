namespace Zlepper.RimWorld.RoyaltyImprovements.Teleporting;

public static class DebugActions
{
    [DebugAction(allowedGameStates = AllowedGameStates.PlayingOnMap, actionType = DebugActionType.Action, category = "Map")]
    public static void RecalculateTeleportingGrid()
    {
        Find.CurrentMap?.GetComponent<TeleporterDistanceTracker>()?.MarkAsDirty();
    }
}