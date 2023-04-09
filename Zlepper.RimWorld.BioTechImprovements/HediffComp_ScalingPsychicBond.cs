namespace Zlepper.RimWorld.BioTechImprovements;

public class Hediff_PsychicBond_Scaling : Hediff_PsychicBond
{
    public override bool TryMergeWith(Hediff other)
    {
        if (other is not HediffWithTarget hediffWithTarget)
        {
            return false;
        }
        
        if (target != hediffWithTarget.target)
        {
            return false;
        }

        return base.TryMergeWith(other);
    }
}