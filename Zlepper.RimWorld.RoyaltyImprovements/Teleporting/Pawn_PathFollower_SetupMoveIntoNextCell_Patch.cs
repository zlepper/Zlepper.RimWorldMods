using HarmonyLib;
using Verse.AI;

namespace Zlepper.RimWorld.RoyaltyImprovements.Teleporting;

[HarmonyPatch(typeof(Pawn_PathFollower))]
public static class Pawn_PathFollower_TryEnterNextPathCell_Patch
{
    [HarmonyPatch(typeof(Pawn_PathFollower), "TryEnterNextPathCell")]
    [HarmonyPostfix]
    public static void Postfix(Pawn_PathFollower __instance, Pawn ___pawn)
    {
        if (!(___pawn.Faction?.IsPlayer ?? false))
        {
            return;
        }

        var map = ___pawn.Map;
        var tracker = map.GetComponent<TeleporterDistanceTracker>();
        if (tracker == null)
        {
            return;
        }

        var currentCell = ___pawn.Position;

        var nextCell = __instance.nextCell;

        if (!currentCell.AdjacentTo8WayOrInside(nextCell))
        {
            var toSkipdoor = tracker.GetTeleporter(nextCell);
            if (toSkipdoor != null)
            {
                var fromSkipdoor = tracker.GetSkipdoorInArea(currentCell);
                if (fromSkipdoor != null)
                {
                    if (fromSkipdoor == toSkipdoor)
                    {
                        return;
                    }


                    FleckMaker.Static(___pawn.TrueCenter(), map, FleckDefOf.PsycastSkipFlashEntry);
                    FleckMaker.Static(nextCell, map, FleckDefOf.PsycastSkipOuterRingExit);
                    FleckMaker.Static(nextCell, map, FleckDefOf.PsycastSkipInnerExit);
                    ___pawn.Position = toSkipdoor.Position;
                    ___pawn.Drawer.tweener.ResetTweenedPosToRoot();
                    __instance.nextCellCostLeft = 0;
                }
            }
        }
    }

    [HarmonyPatch("NeedNewPath")]
    [HarmonyPostfix]
    public static void NeedNewPath_Postfix(Pawn_PathFollower __instance, ref bool __result, Pawn ___pawn)
    {
        if (!__result)
        {
            if (!(___pawn.Faction?.IsPlayer ?? false))
            {
                return;
            }
            
            
            var tracker = ___pawn.Map?.GetComponent<TeleporterDistanceTracker>();
            if (tracker == null)
            {
                return;
            }

            for (var i = 1; i < 20 && i < __instance.curPath.NodesLeftCount; i++)
            {
                var current = __instance.curPath.Peek(i - 1);
                var next = __instance.curPath.Peek(i);
                if (!current.AdjacentTo8WayOrInside(next))
                {
                    var toSkipdoor = tracker.GetTeleporter(next);
                    var fromSkipdoor = tracker.GetSkipdoorInArea(current);

                    if (toSkipdoor == null || fromSkipdoor == null)
                    {
                        __result = true;
                        return;
                    }
                }
            }
        }
    }
}