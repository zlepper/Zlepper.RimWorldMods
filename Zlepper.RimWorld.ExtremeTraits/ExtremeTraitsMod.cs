using HarmonyLib;

namespace Zlepper.RimWorld.ExtremeTraits;

public class ExtremeTraitsMod : Mod
{
    public ExtremeTraitsMod(ModContentPack content) : base(content)
    {
        InjectedDefHasher.PrepareReflection();
        LongEventHandler.QueueLongEvent(RegisterTraits, "RegisterExtremeTrait", true, err =>
        {
            Log.Error("Failed to register extreme traits: " + err);
        });
        RegisterTraits();
    }

    private void RegisterTraits()
    {
        var thoughts = DefDatabase<ThoughtDef>.AllDefs
            .Where(d => d.requiredTraitsDegree != int.MinValue && d.workerClass == typeof(ThoughtWorker_AlwaysActive));

        var thoughtDict = new Dictionary<TraitDef, Dictionary<int, ThoughtDef>>();
        
        foreach (var thoughtDef in thoughts)
        {
            foreach (var requiredTrait in thoughtDef.requiredTraits)
            {
                if (!thoughtDict.TryGetValue(requiredTrait, out var thoughtDegrees))
                {
                    thoughtDict[requiredTrait] = thoughtDegrees = new();
                }

                
                thoughtDegrees[thoughtDef.requiredTraitsDegree] = thoughtDef;
            }
        }

        foreach (var traitDef in DefDatabase<TraitDef>.AllDefs)
        {
            if (traitDef.degreeDatas.Count <= 1)
            {
                continue;
            }

            var configuration = traitDef.GetModExtension<ExtremeTraitsModExtension>() ?? new ExtremeTraitsModExtension();
            
            var positiveDegrees = traitDef.degreeDatas.Where(d => d.degree > 0).OrderBy(d => d.degree).ToList();
            var negativeDegrees = traitDef.degreeDatas.Where(d => d.degree < 0).OrderByDescending(d => d.degree).ToList();

            if (positiveDegrees.Count > 0)
            {
                AddDegrees(positiveDegrees, traitDef, configuration.MaxDegree, thoughtDict);
            }

            if (negativeDegrees.Count > 0)
            {
                AddDegrees(negativeDegrees, traitDef, configuration.MinDegree, thoughtDict);
            }

            traitDef.degreeDatas.SortByDescending(d => d.degree);
        }
        ThoughtUtility.Reset();
    }

    private void AddDegrees(List<TraitDegreeData> degrees, TraitDef traitDef, int mostExtreme,
        Dictionary<TraitDef, Dictionary<int, ThoughtDef>> thoughtDict)
    {
        var extremeDegree = degrees.Last();

        var canBeScaled = extremeDegree.statFactors?.Count > 0
                          || extremeDegree.statOffsets?.Count > 0
                          || extremeDegree.skillGains?.Count > 0
                          || Math.Abs(extremeDegree.socialFightChanceFactor - 1f) > float.Epsilon
                          || Math.Abs(extremeDegree.marketValueFactorOffset - 1f) > float.Epsilon
                          || extremeDegree.randomDiseaseMtbDays != 0f
                          || Math.Abs(extremeDegree.hungerRateFactor - 1f) > float.Epsilon;

        if (!canBeScaled)
        {
            return;
        }

        var extremeThoughts = degrees
            .Select(d => thoughtDict.GetValueSafe(traitDef)?.GetValueSafe(d.degree))
            .NotNull()
            .ToList();
        
        var mostExtremeThought = extremeThoughts.FirstOrDefault(t => t.requiredTraitsDegree == extremeDegree.degree);

        var valueProvider = CreateValueProvider(degrees, extremeDegree, extremeThoughts);

        var offset = extremeDegree.degree > 0 ? 1 : 0;

        var number = 1;
        foreach (var degreeNumber in Range(mostExtreme, extremeDegree.degree))
        {
            var index = number++;
            var actualDegreeNumber = degreeNumber + offset;
            var newDegree = Copy(extremeDegree);
            newDegree.degree = actualDegreeNumber;
            newDegree.label += " +" + index;
            newDegree.commonality = 0;
            
            foreach (var factor in newDegree.statFactors ?? new())
            {
                var calculator = valueProvider[factor.stat];
                factor.value = calculator.GetValue(actualDegreeNumber);
            }
            foreach (var factor in newDegree.statOffsets ?? new())
            {
                var calculator = valueProvider[factor.stat];
                factor.value = calculator.GetValue(actualDegreeNumber);
            }
            
            
            foreach (var skillGain in extremeDegree.skillGains ?? new())
            {
                var calculator = valueProvider[skillGain.skill];

                skillGain.amount = (int)calculator.GetValue(actualDegreeNumber);
            }

            if (extremeDegree.randomDiseaseMtbDays != 0f)
            {
                newDegree.randomDiseaseMtbDays = (float) (Math.Pow(2, 1 - Math.Abs(actualDegreeNumber)) *
                                                          extremeDegree.randomDiseaseMtbDays);
            }

            if (valueProvider.TryGetValue(nameof(extremeDegree.socialFightChanceFactor), out var fightChanceCalculator))
            {
                newDegree.socialFightChanceFactor = fightChanceCalculator.GetValue(actualDegreeNumber);
            }
            
            if (valueProvider.TryGetValue(nameof(extremeDegree.marketValueFactorOffset), out var marketValueCalculator))
            {
                newDegree.marketValueFactorOffset = marketValueCalculator.GetValue(actualDegreeNumber);
            }
            
            if (valueProvider.TryGetValue(nameof(extremeDegree.hungerRateFactor), out var hungerRateCalculator))
            {
                newDegree.hungerRateFactor = hungerRateCalculator.GetValue(actualDegreeNumber);
            }
            
            
            traitDef.degreeDatas.Add(newDegree);
            
            if (mostExtremeThought != null)
            {
                var newThought = Copy(mostExtremeThought);
                newThought.generated = true;
                newThought.requiredTraitsDegree = actualDegreeNumber;
                
                newThought.defName = newThought.defName + "_" + index;
                newThought.label += " +" + index;
                var stage = newThought.stages.First();
                stage.label += " +" + index;
                stage.description = stage.description.Replace(extremeDegree.label, newDegree.label);
                
                stage.baseMoodEffect = valueProvider[nameof(ThoughtStage.baseMoodEffect)].GetValue(actualDegreeNumber);
                stage.baseOpinionOffset = valueProvider[nameof(ThoughtStage.baseOpinionOffset)].GetValue(actualDegreeNumber);
                
                stage.untranslatedLabel = stage.untranslatedLabel.Replace(extremeDegree.label, newDegree.label);
                
                InjectedDefHasher.GiveShortHashToDef(newThought, typeof(ThoughtDef));
                
                DefDatabase<ThoughtDef>.Add(newThought);
            }
        }
        
    }

    private static Dictionary<object, IStatCreator> CreateValueProvider(List<TraitDegreeData> degrees, TraitDegreeData extremeDegree, List<ThoughtDef> thoughts)
    {
        var valueProvider = new Dictionary<object, IStatCreator>();

        foreach (var factor in extremeDegree.statFactors ?? new())
        {
            var values = degrees
                .Select(d => d.statFactors.FirstOrDefault(f => f.stat == factor.stat)?.value)
                .Select(v => v.GetValueOrDefault())
                .ToList();


            valueProvider[factor.stat] = StatCalculator.InferStatValues(values);
        }

        foreach (var factor in extremeDegree.statOffsets ?? new())
        {
            var values = degrees
                .Select(d => d.statOffsets.FirstOrDefault(f => f.stat == factor.stat)?.value)
                .Select(v => v.GetValueOrDefault())
                .ToList();

            valueProvider[factor.stat] = StatCalculator.InferStatValues(values);
        }

        foreach (var skillGain in extremeDegree.skillGains ?? new())
        {
            var values = degrees
                .Select(d => d.skillGains.FirstOrDefault(sg => sg.skill.defName == skillGain.skill.defName)?.amount ?? 0)
                .Select(i => (float) i)
                .ToList();

            valueProvider[skillGain.skill.defName] = StatCalculator.InferStatValues(values);
        }
        
        if(Math.Abs(extremeDegree.socialFightChanceFactor - 1f) > float.Epsilon)
        {
            var values = degrees
                .Select(d => d.socialFightChanceFactor)
                .ToList();

            valueProvider[nameof(extremeDegree.socialFightChanceFactor)] = StatCalculator.InferStatValues(values);
        }

        if(Math.Abs(extremeDegree.marketValueFactorOffset - 1f) > float.Epsilon)
        {
            var values = degrees
                .Select(d => d.marketValueFactorOffset)
                .ToList();

            valueProvider[nameof(extremeDegree.marketValueFactorOffset)] = StatCalculator.InferStatValues(values);
        }

        if(Math.Abs(extremeDegree.hungerRateFactor - 1f) > float.Epsilon)
        {
            var values = degrees
                .Select(d => d.hungerRateFactor)
                .ToList();

            valueProvider[nameof(extremeDegree.hungerRateFactor)] = StatCalculator.InferStatValues(values);
        }

        if (thoughts.Count > 0)
        {
            var baseMoodEffect = thoughts.Select(t => t.stages.First().baseMoodEffect).ToList();

            valueProvider[nameof(ThoughtStage.baseMoodEffect)] = StatCalculator.InferStatValues(baseMoodEffect);

            var baseOpinionOffset = thoughts.Select(t => t.stages.First().baseOpinionOffset).ToList();

            valueProvider[nameof(ThoughtStage.baseOpinionOffset)] = StatCalculator.InferStatValues(baseOpinionOffset);
        }

        return valueProvider;
    }

    private IEnumerable<int> Range(int end1, int end2)
    {
        var start = Math.Min(end1, end2);
        var end = Math.Max(end1, end2);
        
        return Enumerable.Range(start, end - start).OrderBy(Math.Abs);
    }

    private TraitDegreeData Copy(TraitDegreeData sample)
    {
        return new TraitDegreeData
        {
            label = sample.label,
            labelMale = sample.labelMale,
            labelFemale = sample.labelFemale,
            untranslatedLabel = sample.untranslatedLabel,
            description = sample.description,
            degree = sample.degree,
            commonality = sample.commonality,
            statOffsets = sample.statOffsets?.Select(s => new StatModifier()
            {
                stat = s.stat,
                value = s.value
            }).ToList(),
            statFactors = sample.statFactors?.Select(s => new StatModifier()
            {
                stat = s.stat,
                value = s.value
            }).ToList(),
            thinkTree = sample.thinkTree,
            randomMentalState = sample.randomMentalState,
            randomMentalStateMtbDaysMoodCurve = sample.randomMentalStateMtbDaysMoodCurve,
            forcedMentalState = sample.forcedMentalState,
            forcedMentalStateMtbDays = sample.forcedMentalStateMtbDays,
            disallowedMentalStates = sample.disallowedMentalStates,
            disallowedThoughts = sample.disallowedThoughts,
            disallowedThoughtsFromIngestion = sample.disallowedThoughtsFromIngestion,
            extraThoughtsFromIngestion = sample.extraThoughtsFromIngestion,
            disallowedInspirations = sample.disallowedInspirations,
            mentalBreakInspirationGainSet = sample.mentalBreakInspirationGainSet,
            mentalBreakInspirationGainReasonText = sample.mentalBreakInspirationGainReasonText,
            allowedMeditationFocusTypes = sample.allowedMeditationFocusTypes,
            disallowedMeditationFocusTypes = sample.disallowedMeditationFocusTypes,
            mentalBreakInspirationGainChance = sample.mentalBreakInspirationGainChance,
            theOnlyAllowedMentalBreaks = sample.theOnlyAllowedMentalBreaks,
            skillGains = sample.skillGains.Select(sg => new SkillGain()
            {
                amount = sg.amount,
                skill = sg.skill,
            }).ToList(),
            socialFightChanceFactor = sample.socialFightChanceFactor,
            marketValueFactorOffset = sample.marketValueFactorOffset,
            randomDiseaseMtbDays = sample.randomDiseaseMtbDays,
            hungerRateFactor = sample.hungerRateFactor,
            mentalStateGiverClass = sample.mentalStateGiverClass,
            abilities = sample.abilities,
            needs = sample.needs,
            ingestibleModifiers = sample.ingestibleModifiers,
        };
    }

    private ThoughtDef Copy(ThoughtDef source)
    {
        return new ThoughtDef
        {
            label = source.label,
            defName = source.defName,
            description = source.description,
            ignoreConfigErrors = source.ignoreConfigErrors,
            ignoreIllegalLabelCharacterConfigError = source.ignoreIllegalLabelCharacterConfigError,
            modExtensions = source.modExtensions,
            generated = true,
            thoughtClass = source.thoughtClass,
            workerClass = source.workerClass,
            stages = source.stages.Select(s => new ThoughtStage
            {
                label = s.label,
                labelSocial = s.labelSocial,
                description = s.description,
                baseMoodEffect = s.baseMoodEffect,
                baseOpinionOffset = s.baseOpinionOffset,
                visible = s.visible,
                untranslatedLabel = s.untranslatedLabel,
                untranslatedLabelSocial = s.untranslatedLabelSocial
            }).ToList(),
            stackLimit = source.stackLimit,
            stackedEffectMultiplier = source.stackedEffectMultiplier,
            durationDays = source.durationDays,
            invert = source.invert,
            validWhileDespawned = source.validWhileDespawned,
            nextThought = source.nextThought,
            producesMemoryThought = source.producesMemoryThought,
            nullifyingTraits = source.nullifyingTraits,
            neverNullifyIfAnyTrait = source.neverNullifyIfAnyTrait,
            nullifyingTraitDegrees = source.nullifyingTraitDegrees,
            nullifyingOwnTales = source.nullifyingOwnTales,
            nullifyingPrecepts = source.nullifyingPrecepts,
            requiredTraits = source.requiredTraits,
            requiredTraitsDegree = source.requiredTraitsDegree,
            effectMultiplyingStat = source.effectMultiplyingStat,
            hediff = source.hediff,
            gameCondition = source.gameCondition,
            nullifiedIfNotColonist = source.nullifiedIfNotColonist,
            thoughtToMake = source.thoughtToMake,
            showBubble = source.showBubble,
            minExpectation = source.minExpectation,
            minExpectationForNegativeThought = source.minExpectationForNegativeThought,
            lerpMoodToZero = source.lerpMoodToZero,
            stackLimitForSameOtherPawn = source.stackLimitForSameOtherPawn,
            lerpOpinionToZeroAfterDurationPct = source.lerpOpinionToZeroAfterDurationPct,
            doNotApplyToQuestLodgers = source.doNotApplyToQuestLodgers,
            maxCumulatedOpinionOffset = source.maxCumulatedOpinionOffset,
            taleDef = source.taleDef,
            gender = source.gender
        };
    }
}