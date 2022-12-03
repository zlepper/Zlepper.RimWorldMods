namespace Zlepper.RimWorld.PersonalitySurgery;

public static class Helpers
{
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