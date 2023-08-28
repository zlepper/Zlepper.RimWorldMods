using RimWorld.Planet;

namespace Zlepper.RimWorld.Warnings;

public class Alert_ActiveHostiles : Alert
{
    public Alert_ActiveHostiles()
    {
        defaultLabel = "Alert_ActiveHostiles_Label".Translate();
        defaultExplanation = "Alert_ActiveHostiles_Description".Translate();
        defaultPriority = AlertPriority.Critical;
    }

    public override AlertReport GetReport()
    {
        var playerFaction = Faction.OfPlayer;
        var hostiles = new List<GlobalTargetInfo>();

        foreach (var map in Find.Maps)
        {
            foreach (var pawn in map.mapPawns.AllPawns)
            {
                if (!pawn.Downed && pawn.Spawned && pawn.HostileTo(playerFaction) && pawn.MentalStateDef != MentalStateDefOf.PanicFlee)
                {
                    hostiles.Add(pawn);
                }
            }

            foreach (var building in map.listerBuildings.allBuildingsNonColonist)
            {
                if (building.Spawned && building.HostileTo(playerFaction))
                {
                    if (building.GetComp<CompCanBeDormant>() is {Awake: false})
                    {
                        continue;
                    }

                    if (building.def.building is {combatPower: 0})
                    {
                        continue;
                    }
                    
                    hostiles.Add(building);
                }
            }
        }

        return AlertReport.CulpritsAre(hostiles);
    }
}