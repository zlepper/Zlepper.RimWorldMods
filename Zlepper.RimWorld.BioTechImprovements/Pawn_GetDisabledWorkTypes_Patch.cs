using HarmonyLib;

namespace Zlepper.RimWorld.BioTechImprovements;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.GetDisabledWorkTypes), typeof(bool))]
public static class Pawn_GetDisabledWorkTypes_Patch
{
    [HarmonyPostfix]
    public static List<WorkTypeDef> GetDisabledWorkTypes(List<WorkTypeDef> disabledWorkTypes ,bool permanentOnly, Pawn __instance)
    {
        if (!MechanitorUtility.IsMechanitor(__instance))
        {
            if (!disabledWorkTypes.Contains(BioTechImprovementsWorkTypeDefOf.Mechanitor))
            {
                disabledWorkTypes.Add(BioTechImprovementsWorkTypeDefOf.Mechanitor);
            }
        }

        return disabledWorkTypes;
    }
} 