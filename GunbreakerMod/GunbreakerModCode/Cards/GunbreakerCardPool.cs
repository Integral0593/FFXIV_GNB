using Godot;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Cards;

public sealed class GunbreakerCardPool : TypeListCardPoolModel
{
    public override string Title => "gunbreaker";

    // Placeholder: reuses Defect's built-in energy icon / frame material until custom Gunbreaker art ships.
    public override string EnergyColorName => "defect";
    public override string CardFrameMaterialPath => "card_frame_blue";
    public override Color DeckEntryCardColor => new("3EB3ED");

    public override bool IsColorless => false;
}
