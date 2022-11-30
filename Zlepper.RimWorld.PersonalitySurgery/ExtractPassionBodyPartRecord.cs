namespace Zlepper.RimWorld.PersonalitySurgery;

public sealed class ExtractPassionBodyPartRecord : BodyPartRecord
{
    public Passion Passion;
    public SkillDef SkillDef = null!;

    private bool Equals(ExtractPassionBodyPartRecord other)
    {
        return Passion == other.Passion && SkillDef.Equals(other.SkillDef);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is ExtractPassionBodyPartRecord other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return ((int) Passion * 397) ^ SkillDef.GetHashCode();
        }
    }
}



