using UnityEngine;
using Verse.Sound;

namespace Zlepper.RimWorld.Enchanting;

public class Tab_EnchantingTableBills : ITab
{
    private static readonly Vector2 WinSize = new Vector2(420f, 480f);
    private Bill? mouseoverBill;
    
    private Building_WorkTable SelTable => (Building_WorkTable)base.SelThing;
    
    private float viewHeight = 1000f;

    private Vector2 scrollPosition;
    
    public Tab_EnchantingTableBills()
    {
        labelKey = "TabBills";
        size = WinSize;
    }

    protected override void FillTab()
    {
        var rect3 = new Rect(0f, 0f, WinSize.x, WinSize.y).ContractedBy(10f);
        mouseoverBill = SelTable.billStack.DoListing(rect3, recipeOptionsMaker, ref scrollPosition, ref viewHeight);
    }
    
    private List<FloatMenuOption> recipeOptionsMaker()
    {
        return new List<FloatMenuOption>
        {
            new FloatMenuOption("Enchant item", () =>
            {
                Find.WindowStack.Add(new EnchantingWindow(SelTable));
            })
        };
    }

    public override void TabUpdate()
    {
        if (mouseoverBill != null)
        {
            mouseoverBill.TryDrawIngredientSearchRadiusOnMap(SelTable.Position);
            mouseoverBill = null;
        }
    }
}