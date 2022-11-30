namespace Zlepper.RimWorld.PersonalitySurgery;

public sealed class ExtractTraitBodyPartRecord : BodyPartRecord
{
    public Trait TraitRef = null!;

    private bool Equals(ExtractTraitBodyPartRecord other)
    {
        return TraitRef.Equals(other.TraitRef);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is ExtractTraitBodyPartRecord other && Equals(other);
    }

    public override int GetHashCode()
    {
        return TraitRef.GetHashCode();
    }
}