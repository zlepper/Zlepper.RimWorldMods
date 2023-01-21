using UnityEngine;

namespace Zlepper.RimWorld.RoyaltyImprovements.Teleporting;

public record Teleporter(int Id, IntVec3 Position)
{
    public readonly int Id = Id;
    public readonly IntVec3 Position = Position;

    private static readonly Color[] DebugColors = {
        Color.red,
        Color.green,
        Color.blue,
        Color.black,
        Color.yellow,
        Color.cyan,
        Color.magenta,
    };

    public readonly Color Color = DebugColors[Id % DebugColors.Length];

    public Teleporter(Thing thing) : this(thing.thingIDNumber, thing.Position)
    {
    }

    public virtual bool Equals(Teleporter? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id;
    }
    
    
    private static float GetColorValue(int from, int offset)
    {
        var v = (from + offset * 70) % 255;
        return v / 255f;
    }

}