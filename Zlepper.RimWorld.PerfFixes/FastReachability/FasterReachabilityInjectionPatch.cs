using HarmonyLib;
using Verse.AI;

namespace Zlepper.RimWorld.PerfFixes.FastReachability;
//
// [HarmonyPatch]
// public static class FasterReachabilityInjectionPatch
// {
//     [HarmonyPatch(typeof(Reachability), nameof(Reachability.CanReach), typeof(IntVec3), typeof(LocalTargetInfo),
//         typeof(PathEndMode), typeof(TraverseParms))]
//     [HarmonyPrefix]
//     public static bool Prefix_CanReach(IntVec3 start,
//         LocalTargetInfo dest,
//         PathEndMode peMode,
//         TraverseParms traverseParams, Map ___map, ref bool __result)
//     {
//         var c = ___map.GetComponent<FasterReachabilityComponent>();
//         __result = c.Reachability.CanReach(start, dest, peMode, traverseParams);
//
//         return false;
//     }
//
//     [HarmonyPatch(typeof(ThingGrid), "RegisterInCell", typeof(Thing), typeof(IntVec3))]
//     [HarmonyPrefix]
//     public static void Prefix_ThingGrid_RegisterInCell(Thing t, IntVec3 c, Map ___map)
//     {
//         if (t.def?.AffectsReachability ?? false)
//         {
//             var reach = ___map.GetComponent<FasterReachabilityComponent>();
//             reach.Reachability.MarkDirty();
//         }
//     }
//
//     [HarmonyPatch(typeof(ThingGrid), "DeregisterInCell", typeof(Thing), typeof(IntVec3))]
//     [HarmonyPrefix]
//     public static void Prefix_ThingGrid_DeregisterInCell(Thing t, IntVec3 c, Map ___map)
//     {
//         if (t.def?.AffectsReachability ?? false)
//         {
//             var reach = ___map.GetComponent<FasterReachabilityComponent>();
//             reach.Reachability.MarkDirty();
//         }
//     }
// }