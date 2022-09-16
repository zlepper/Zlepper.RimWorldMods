using System;
using System.Collections.Generic;
using System.Linq;
using HugsLib;
using RimWorld;
using Verse;
using Verse.Noise;

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
                Logger.Message("Skipping trait " + traitDef.defName + " because it has no degrees");
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
            Logger.Message("Trait degree {0} cannot be scaled", extremeDegree.label);
            return;
        }

        var valueProvider = new Dictionary<Def, IStatCreator>();
        
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
        
        var offset = extremeDegree.degree > 0 ? 1 : 0;

        var number = 1;
        foreach (var degreeNumber in Range(mostExtreme, extremeDegree.degree))
        {
            var actualDegreeNumber = degreeNumber + offset;
            var newDegree = Copy(extremeDegree);
            newDegree.degree = actualDegreeNumber;
            newDegree.label += " +" + number++;
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
            traitDef.degreeDatas.Add(newDegree);
        }
        
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

public class ExtremeTraitsModExtension : DefModExtension
{
    public int MaxDegree = 10;
    public int MinDegree = -10;

    public override IEnumerable<string> ConfigErrors()
    {
        if (MaxDegree < 1)
        {
            yield return "MaxDegree must be greater than 0";
        }

        if (MinDegree > -1)
        {
            yield return "MinDegree must be less than 0";
        }
    }
}