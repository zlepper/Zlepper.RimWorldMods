namespace Zlepper.RimWorld.PerfFixes.SmarterTicking;

public class HitTracker
{
    public int Hits;
    public int Misses;

    public void Hit()
    {
        Hits++;
        LogStats();
    }

    public void Miss()
    {
        Misses++;
        LogStats();
    }
    
    public void LogStats()
    {
        if (Hits > 0 && (Hits % 1000 == 0 || Misses % 1000 == 0))
        {
            Log.Message($"Hits: {Hits}, Miss: {Misses}");
            Log.ResetMessageCount();
        }
    }
}