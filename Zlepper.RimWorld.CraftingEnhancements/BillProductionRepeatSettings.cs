namespace Zlepper.RimWorld.CraftingEnhancements;

public class BillProductionRepeatSettings
{
    public int targetCount;
    public bool pauseWhenSatisfied;
    public int unpauseWhenYouHave;
    public bool includeEquipped;
    public bool includeTainted;
    public FloatRange hpRange;
    public QualityRange qualityRange;
    public bool limitToAllowedStuff;
    public BillStoreModeDef storeMode;
    public ISlotGroup slotGroup;
    public ISlotGroup includeGroup;


    public BillProductionRepeatSettings(Bill_Production bill)
    {
        targetCount = bill.targetCount;
        pauseWhenSatisfied = bill.pauseWhenSatisfied;
        unpauseWhenYouHave = bill.unpauseWhenYouHave;
        includeEquipped = bill.includeEquipped;
        includeTainted = bill.includeTainted;
        hpRange = bill.hpRange;
        qualityRange = bill.qualityRange;
        limitToAllowedStuff = bill.limitToAllowedStuff;
        storeMode = bill.GetStoreMode();
        slotGroup = bill.GetSlotGroup();
        includeGroup = bill.GetIncludeSlotGroup();
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
        bill.SetStoreMode(storeMode, slotGroup);
        bill.SetIncludeGroup(includeGroup);
    }
}