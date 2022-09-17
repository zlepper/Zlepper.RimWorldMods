using System;
using System.Collections.Generic;
using System.Linq;
using HugsLib;
using RimWorld;
using Verse;

namespace Zlepper.RimWorld.ExtremeTraits;

public class ExtremeTraitsMod : ModBase
{
    public override string ModIdentifier => "Zlepper.RimWorld.ExtremeTraits";

    public override void DefsLoaded()
    {
        if (!ModIsActive)
        {
            return;
        }
        RegisterTraits();
    }


    private void RegisterTraits()
    {
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
                AddDegrees(positiveDegrees, traitDef, configuration.MaxDegree);
            }

            if (negativeDegrees.Count > 0)
            {
                AddDegrees(negativeDegrees, traitDef, configuration.MinDegree);
            }

            traitDef.degreeDatas.SortByDescending(d => d.degree);
        }
    }

    private void AddDegrees(List<TraitDegreeData> degrees, TraitDef traitDef, int mostExtreme)
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

        var valueProvider = CreateValueProvider(degrees, extremeDegree);

        var offset = extremeDegree.degree > 0 ? 1 : 0;

        var number = 1;
        foreach (var degreeNumber in Range(mostExtreme, extremeDegree.degree))
        {
            var actualDegreeNumber = degreeNumber + offset;
            var newDegree = Copy(extremeDegree);
            newDegree.degree = actualDegreeNumber;
            newDegree.label += " +" + number++;
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
                var calculator = valueProvider[skillGain.Key];
                
                newDegree.skillGains[skillGain.Key] = (int)calculator.GetValue(actualDegreeNumber);
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
        }
        
    }

    private static Dictionary<object, IStatCreator> CreateValueProvider(List<TraitDegreeData> degrees, TraitDegreeData extremeDegree)
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
                .Select(d => d.skillGains.TryGetValue(skillGain.Key, out var v) ? v : 0)
                .Select(i => (float) i)
                .ToList();

            valueProvider[skillGain.Key] = StatCalculator.InferStatValues(values);
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
            skillGains = sample.skillGains?.ToDictionary(s => s.Key, s => s.Value),
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
}