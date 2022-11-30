namespace Zlepper.RimWorld.PersonalitySurgery;

public static class Helpers
{
    public static void DoBrainDamage(Pawn target, Pawn billDoer)
    {
        var brain = target.health.hediffSet.GetBrain();
        var damageAmount = Rand.Range(brain.def.GetMaxHealth(target) * 0.2f, brain.def.GetMaxHealth(target) * 0.4f);
        
        var damageInfo = new DamageInfo(DamageDefOf.Cut, damageAmount, hitPart: brain, instigator: billDoer, spawnFilth: false);
        damageInfo.SetIgnoreArmor(true);
        damageInfo.SetIgnoreInstantKillProtection(true);
        damageInfo.SetInstantPermanentInjury(true);
        target.TakeDamage(damageInfo);
    }
    
    public static Passion GetOriginalPassion(this SkillRecord skillRecord)
    {
        var passionGene =
            skillRecord.Pawn.genes.GenesListForReading
                .Where(g => g.Active)
                .FirstOrDefault(r => r.def.passionMod?.skill == skillRecord.def);

        var originalPassion = passionGene?.NewPassionForOnRemoval(skillRecord) ??
                              skillRecord.passion;
        return originalPassion;
    }
}