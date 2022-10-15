namespace Zlepper.RimWorld.PersonalitySurgery.Recipes;

public abstract class BaseBioRecipeDef : RecipeDef
{
    public abstract bool CheckIfSurgeryIsPossible(Pawn pawn);
    public abstract void ApplyChanges(Pawn pawn, Pawn billDoer);
    
}