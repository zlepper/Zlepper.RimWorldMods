using UnityEngine;

namespace Zlepper.RimWorld.PersonalitySurgery;

[StaticConstructorOnStartup]
public abstract class PersonalityThing : ThingWithComps, IModifyStat
{
    private Pawn? TargetPawn;
    private static readonly CachedTexture ImplantTex = new CachedTexture("UI/Gizmos/ImplantGenes");
    private static readonly Texture2D CancelIcon = ContentFinder<Texture2D>.Get("UI/Designators/Cancel");

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_References.Look(ref TargetPawn, nameof(TargetPawn));
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos())
        {
            yield return gizmo;
        }

        if (!IsValid())
        {
            yield break;
        }


        if (TargetPawn == null)
        {
            var baseCommand = GetBaseImplantCommand();

            baseCommand.icon = ImplantTex.Texture;
            baseCommand.action = () =>
            {
                var options = new List<FloatMenuOption>();
                foreach (var pawn in MapHeld.mapPawns.AllPawnsSpawned)
                {
                    if (pawn.IsQuestLodger())
                    {
                        continue;
                    }

                    if (!pawn.IsColonistPlayerControlled && !pawn.IsPrisonerOfColony && !pawn.IsSlaveOfColony)
                    {
                        continue;
                    }

                    var reason = GetNotImplantableReason(pawn);
                    if (reason != null)
                    {
                        options.Add(new FloatMenuOption(reason, null, pawn, Color.white));
                        continue;
                    }

                    options.Add(new FloatMenuOption(pawn.LabelShortCap, () => { SetTargetPawn(pawn); }, pawn,
                        Color.white));
                }

                if (!options.Any())
                {
                    options.Add(new FloatMenuOption("No pawns available", null));
                }

                Find.WindowStack.Add(new FloatMenu(options));
            };

            yield return baseCommand;
        }
        else
        {
            var existing = GetExistingBill();
            if (existing == null)
            {
                TargetPawn = null;
                yield break;
            }
            
            var baseCommand = GetBaseCancelCommand(TargetPawn);
            baseCommand.icon = CancelIcon;
            baseCommand.action = RemoveCurrentTarget;
            yield return baseCommand;
        }
    }

    protected abstract bool IsValid();

    protected abstract Command_Action GetBaseCancelCommand(Pawn targetPawn);
    protected abstract Command_Action GetBaseImplantCommand();

    protected abstract string? GetNotImplantableReason(Pawn pawn);

    private void RemoveCurrentTarget()
    {
        var bill = GetExistingBill();
        if (bill != null)
        {
            TargetPawn?.BillStack?.Delete(bill);
        }
    }

    private Bill_Medical? GetExistingBill()
    {
        return TargetPawn?.BillStack?.Bills.OfType<Bill_Medical>()
            .FirstOrDefault(b => b.uniqueRequiredIngredients.NotNullAndContains(this));
    }

    protected abstract string? GetWarningText(Pawn target);

    public void SetTargetPawn(Pawn newTarget)
    {
        var warningText = GetWarningText(newTarget);

        if (warningText != null)
        {
            Find.WindowStack.Add(Dialog_MessageBox.CreateConfirmation(warningText,
                () => { _internalSetTargetPawn(newTarget); }, destructive: true));
        }
        else
        {
            _internalSetTargetPawn(newTarget);
        }
    }

    private void _internalSetTargetPawn(Pawn newTarget)
    {
        PersonalitySurgeryMod.ModLogger.Message("Setting target pawn to " + newTarget);
        RemoveCurrentTarget();

        var brain = newTarget.health.hediffSet.GetBrain();
        HealthCardUtility.CreateSurgeryBill(newTarget, PersonalitySurgeryModDefOf.InstallBioPropertySurgery, brain,
            new List<Thing>
            {
                this
            });
        TargetPawn = newTarget;
    }

    public abstract void OnSurgerySuccess(Pawn pawn);
    public float PostGetBaseValueFor(StatDef stat, float baseValue)
    {
        if (stat == StatDefOf.MarketValue)
        {
            return CalculateBaseMarketValue(baseValue);
        }

        return baseValue;
    }
    
    
    protected abstract float CalculateBaseMarketValue(float originalValue);

}