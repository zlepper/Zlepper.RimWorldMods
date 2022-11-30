namespace Zlepper.RimWorld.PersonalitySurgery;

public class Recipe_ExtractBioPropertySurgery : Recipe_Surgery
{
    public override string GetLabelWhenUsedOn(Pawn pawn, BodyPartRecord part)
    {
        switch (part)
        {
            case ExtractTraitBodyPartRecord traitPart:
                return $"Extract trait '{traitPart.TraitRef.Label}'";

            case ExtractPassionBodyPartRecord passionPart:
                return $"Extract passion {passionPart.Passion} in {passionPart.SkillDef.label}";
        }

        return base.GetLabelWhenUsedOn(pawn, part);
    }

    public override IEnumerable<BodyPartRecord> GetPartsToApplyOn(Pawn pawn, RecipeDef _)
    {
        var brain = pawn.health.hediffSet.GetBrain();

        if (brain == null)
        {
            return Enumerable.Empty<BodyPartRecord>();
        }

        return GetExtractableTraits(pawn, brain)
            .Concat(GetExtractablePassions(pawn, brain));
    }

    private static IEnumerable<BodyPartRecord> GetExtractablePassions(Pawn pawn,
        BodyPartRecord brain)
    {
        return pawn.skills.skills
            .Where(s => !s.TotallyDisabled)
            .Select(skillRecord =>
            {
                var originalPassion = skillRecord.GetOriginalPassion();
                return new {skillRecord, originalPassion};
            })
            .Where(r => r.originalPassion > Passion.None)
            .Select(r => new ExtractPassionBodyPartRecord
            {
                body = brain.body,
                coverage = 0,
                def = brain.def,
                depth = brain.depth,
                height = brain.height,
                parent = brain,
                coverageAbs = 0,
                Passion = r.originalPassion,
                SkillDef = r.skillRecord.def
            });
    }

    private static IEnumerable<BodyPartRecord> GetExtractableTraits(Pawn pawn, BodyPartRecord brain)
    {
        return pawn.story.traits.allTraits
            .Where(trait => trait.sourceGene == null && !trait.Suppressed)
            .Select(trait => new ExtractTraitBodyPartRecord
            {
                body = brain.body,
                coverage = 0,
                def = brain.def,
                depth = brain.depth,
                height = brain.height,
                parent = brain,
                coverageAbs = 0,
                TraitRef = trait,
            });
    }

    public override void ApplyOnPawn(Pawn pawn, BodyPartRecord part, Pawn? billDoer, List<Thing> ingredients, Bill bill)
    {
        switch (part)
        {
            case ExtractTraitBodyPartRecord traitPart:
                DoSurgery(pawn, part, billDoer, ingredients, bill, () =>
                {
                    var trait = pawn.story.traits.allTraits.FirstOrDefault(t => t == traitPart.TraitRef);
                    if (trait != null)
                    {
                        var thing = (TraitThing) GenSpawn.Spawn(PersonalitySurgeryModDefOf.SurgeryExtractBioTraitItem,
                            billDoer!.Position, billDoer.Map);
                        thing.TraitDef = trait.def;
                        thing.TraitDegree = trait.Degree;
                        pawn.story.traits.RemoveTrait(trait);
                        return new SurgeryResult(true, true);
                    }

                    return new SurgeryResult(false, false);
                });
                break;
            case ExtractPassionBodyPartRecord passionPart:
                DoSurgery(pawn, part, billDoer, ingredients, bill, () =>
                {
                    var skill = pawn.skills.GetSkill(passionPart.SkillDef);

                    if (skill != null &&
                        skill.passion - passionPart.Passion is >= (byte) Passion.None and <= (byte) Passion.Major)
                    {
                        var thing = (PassionThing) GenSpawn.Spawn(
                            PersonalitySurgeryModDefOf.SurgeryExtractBioPassionItem, billDoer!.Position, billDoer.Map);
                        thing.passion = passionPart.Passion;
                        thing.SkillDef = passionPart.SkillDef;

                        skill.passion -= passionPart.Passion;
                        return new SurgeryResult(true, true);

                    }

                    Log.Error("Tried to remove passion from " + pawn.Name +
                              $" but they didn't have it. Skill: {skill}. SkillPassion: {skill?.passion}");

                    return new SurgeryResult(false, false);
                });
                break;
            default:
                PersonalitySurgeryMod.ModLogger.Warning(
                    $"body part {part} is not a {typeof(ExtractTraitBodyPartRecord)}, got {part.GetType()}");
                break;
        }
    }

    private void DoSurgery(Pawn pawn, BodyPartRecord part, Pawn? billDoer, List<Thing> ingredients, Bill bill,
        Func<SurgeryResult> doChanges)
    {
        if (billDoer == null) 
            return;
        
        if (CheckSurgeryFail(billDoer, pawn, ingredients, part, bill))
            return;

        var result = doChanges();
        if (!result.Success) return;
        
        TaleRecorder.RecordTale(TaleDefOf.DidSurgery, billDoer, pawn);

        if (result.DoBrainDamage)
        {
            Helpers.DoBrainDamage(pawn, billDoer);
        }

        if (!IsViolationOnPawn(pawn, part, Faction.OfPlayer))
            return;
            
        ReportViolation(pawn, billDoer, pawn.HomeFaction, -70);
    }

    private readonly record struct SurgeryResult(bool Success, bool DoBrainDamage);
}