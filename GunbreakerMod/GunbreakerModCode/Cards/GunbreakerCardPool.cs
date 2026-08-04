using Godot;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Cards;

// TEMPORARY: marks the pool "shared" so ModelDb.AllCards (and the dev console) can see its cards
// before a real Gunbreaker character exists to own it. Remove this attribute once a
// ModCharacterTemplate<GunbreakerCardPool, ...> is registered - a character-owned pool doesn't need it.
[RegisterSharedCardPool]
public sealed class GunbreakerCardPool : TypeListCardPoolModel
{
    public override string Title => "gunbreaker";

    // Placeholder: reuses Defect's built-in energy icon / frame material until custom Gunbreaker art ships.
    public override string EnergyColorName => "defect";
    public override string CardFrameMaterialPath => "card_frame_blue";
    public override Color DeckEntryCardColor => new("3EB3ED");

    public override bool IsColorless => false;
}
