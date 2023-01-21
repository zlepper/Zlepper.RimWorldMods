namespace Zlepper.RimWorld.RoyaltyImprovements.Teleporting;

public static class AdjacencyArrows
{
    public static string GetArrow(IntVec3 vec)
    {
        if (vec.x == 0)
        {
            if (vec.z == 0)
            {
                return "O";
            }

            if (vec.z > 0)
            {
                return "↑";
            }

            return "↓";
        }

        if (vec.x > 0)
        {
            if (vec.z == 0)
            {
                return "→";
            }

            if (vec.z > 0)
            {
                return "↗";
            }

            return "↘";
        }

        if (vec.z == 0)
        {
            return "←";
        }

        if (vec.z > 0)
        {
            return "↖";
        }

        return "↙";
    }
}