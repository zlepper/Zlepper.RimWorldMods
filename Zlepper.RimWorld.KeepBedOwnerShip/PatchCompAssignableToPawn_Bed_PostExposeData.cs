using HarmonyLib;

namespace Zlepper.RimWorld.KeepBedOwnerShip;

/// Normally the game removes ownership of beds if pawn.ownership doesn't reflect the ownership. This patch stops that.
[HarmonyPatch(typeof(CompAssignableToPawn_Bed), nameof(CompAssignableToPawn_Bed.PostExposeData))]
internal class PatchCompAssignableToPawn_Bed_PostExposeData
{
    [HarmonyPrefix]
    private static bool Prefix(List<Pawn> ___assignedPawns, ThingWithComps ___parent)
    {
        if (Scribe.mode != LoadSaveMode.PostLoadInit) return true;

        var unreciprocatedOwners = ___assignedPawns
            .Where(p => p?.ownership?.OwnedBed != ___parent)
            .ToList();
        if (unreciprocatedOwners.Count > 0)
        {
            if (unreciprocatedOwners.Any(p => p.IsFreeColonist))
            {
                return false;
            }
            ___assignedPawns.RemoveAll(p => p?.IsFreeColonist == false && unreciprocatedOwners.Contains(p));
        }
        return true;
    }
}