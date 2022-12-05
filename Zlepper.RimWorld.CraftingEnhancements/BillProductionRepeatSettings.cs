namespace Zlepper.RimWorld.CraftingEnhancements;

public class BillProductionRepeatSettings
{
    public int targetCount;
    public bool pauseWhenSatisfied;
    public int unpauseWhenYouHave;
    public bool includeEquipped;
    public bool includeTainted;
    public Zone_Stockpile? includeFromZone;
    public FloatRange hpRange;
    public QualityRange qualityRange;
    public bool limitToAllowedStuff;

    public BillProductionRepeatSettings(Bill_Production bill)
    {
        targetCount = bill.targetCount;
        pauseWhenSatisfied = bill.pauseWhenSatisfied;
        unpauseWhenYouHave = bill.unpauseWhenYouHave;
        includeEquipped = bill.includeEquipped;
        includeTainted = bill.includeTainted;
        includeFromZone = bill.includeFromZone;
        hpRange = bill.hpRange;
        qualityRange = bill.qualityRange;
        limitToAllowedStuff = bill.limitToAllowedStuff;
    }

    public void ApplyToBill(Bill_Production bill)
    {
        bill.targetCount = targetCount;
        bill.pauseWhenSatisfied = pauseWhenSatisfied;
        bill.unpauseWhenYouHave = unpauseWhenYouHave;
        bill.includeEquipped = includeEquipped;
        bill.includeTainted = includeTainted;
        bill.hpRange = hpRange;
        bill.qualityRange = qualityRange;
        bill.limitToAllowedStuff = limitToAllowedStuff;

        if (includeFromZone != null)
        {
            var matchedZone = Find.CurrentMap?.zoneManager?.AllZones?.FirstOrDefault(s => s.ID == includeFromZone.ID);
            if(matchedZone is Zone_Stockpile stockpile)
            {
                bill.includeFromZone = stockpile;
            }
            else
            {
                bill.includeFromZone = null;
                includeFromZone = null;
            }
        }
        else
        {
            bill.includeFromZone = null;
        }
    }
}