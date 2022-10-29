using System.Reflection;

namespace Zlepper.RimWorld.BioTechImprovements;

public class TransferableComparer_Mechanitor : TransferableComparer
{
    public override int Compare(Transferable lhs, Transferable rhs)
    {
        if(lhs.AnyThing is not Pawn {IsColonyMech: true} p1 || rhs.AnyThing is not Pawn {IsColonyMech: true} p2)
            return 0;

        var mechanitor1 = p1.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Overseer);
        var mechanitor2 = p2.relations.GetFirstDirectRelationPawn(PawnRelationDefOf.Overseer);

        if (mechanitor1 == null || mechanitor2 == null)
        {
            return 0;
        }

        return string.Compare(mechanitor1.Name.ToStringFull, mechanitor2.Name.ToStringFull, StringComparison.InvariantCultureIgnoreCase);
    }
}