using HarmonyLib;

namespace Zlepper.RimWorld.PerfFixes.FastReachability;
//
// [HarmonyPatch]
// public static class PawnUtilityPatch
// {
//     public static bool EnemiesAreNearby(
//         Pawn pawn,
//         int regionsToScan,
//         bool passDoors,
//         float maxDistance,
//         int maxCount,
//         out bool __result)
//     {
//         var map = pawn.MapHeld;
//         if (map == null)
//         {
//             __result = false;
//             return false;
//         }
//
//         if (maxCount == 1)
//         {
//             
//         }
//     }
// }