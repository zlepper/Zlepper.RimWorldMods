using Verse.AI;

namespace Zlepper.RimWorld.RoyaltyImprovements;

public class WorkGiver_Meditate : WorkGiver
{
    public override Job? NonScanJob(Pawn pawn)
    {
        if (!MeditationUtility.CanMeditateNow(pawn))
        {
            return null;
        }
        
        var job = GetMeditationJob(pawn);
        if (job == null)
        {
            return null;
        }
        if (!MeditationUtility.SafeEnvironmentalConditions(pawn, job.targetA.Cell, pawn.MapHeld))
        {
            JobMaker.ReturnToPool(job);
            return null;
        }
        

        return job;
    }
    
    private static Job? GetMeditationJob(Pawn pawn)
    {
        var meditationSpot = MeditationUtility.FindMeditationSpot(pawn);
        if (!meditationSpot.IsValid)
            return null;
        Job meditationJob;
        if (meditationSpot.focus.Thing is Building_Throne thing)
        {
            meditationJob = JobMaker.MakeJob(JobDefOf.ReignMore,  thing,  null,  thing);
        }
        else
        {
            meditationJob = JobMaker.MakeJob(JobDefOf.MeditateMore, meditationSpot.spot,  null, meditationSpot.focus);
        }
        meditationJob.ignoreJoyTimeAssignment = false;
        return meditationJob;
    }
}