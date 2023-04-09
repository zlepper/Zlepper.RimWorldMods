using UnityEngine;

namespace Zlepper.RimWorld.Enchanting;

public class EnchantingWindow : Window
{
    private Building_WorkTable _selectedTable;

    public EnchantingWindow(Building_WorkTable selectedTable)
    {
        _selectedTable = selectedTable;
        forcePause = true;
        doCloseX = true;
        absorbInputAroundWindow = true;
        onlyOneOfTypeAllowed = true;
        optionalTitle = "Enchant item";
    }

    public override void DoWindowContents(Rect inRect)
    {
        var listing = new Listing_Standard();
        listing.Begin(inRect);
     
        
        
        
        
        listing.End();
    }
}

public class BillEnchanting : Bill
{
    public override bool ShouldDoNow()
    {
        return suspended;
    }
}