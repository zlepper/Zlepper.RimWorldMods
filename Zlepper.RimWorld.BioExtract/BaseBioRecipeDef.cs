using Verse;

namespace Zlepper.Rimworld.BioExtract;

public abstract class BaseBioRecipeDef : RecipeDef
{
    public abstract bool CheckIfSurgeryIsPossible(Pawn pawn);
    public abstract void ApplyChanges(Pawn pawn, Pawn billDoer);
}