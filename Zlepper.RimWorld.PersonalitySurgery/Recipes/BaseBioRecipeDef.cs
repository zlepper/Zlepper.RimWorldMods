namespace Zlepper.RimWorld.PersonalitySurgery.Recipes;

public abstract class BaseBioRecipeDef : RecipeDef
{
    public abstract bool CheckIfSurgeryIsPossible(Pawn pawn);
    public abstract void ApplyChanges(Pawn pawn, BodyPartRecord bodyPartRecord, Pawn billDoer);

    protected void DamagePart(BodyPartRecord part, Pawn target, Pawn billDoer)
    {
        var maxHeath = part.def.GetMaxHealth(target);
        var amount = Rand.Range(maxHeath / 6f, maxHeath / 3f);
        
        var damageInfo = new DamageInfo(DamageDefOf.Cut, amount, hitPart: part, instigator: billDoer, spawnFilth: false);
        damageInfo.SetIgnoreArmor(true);
        damageInfo.SetIgnoreInstantKillProtection(true);
        damageInfo.SetInstantPermanentInjury(true);
        target.TakeDamage(damageInfo);
    }

    public virtual TaggedString GetConfirmation(Pawn pawn)
    {
        return null;
    }
}