using Godot;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Cards;

public sealed class GunbreakerCardPool : TypeListCardPoolModel
{
    public override string Title => "gunbreaker";

    // Placeholder: reuses Defect's built-in energy icon until custom Gunbreaker art ships.
    public override string EnergyColorName => "defect";

    // The persistent energy-orb background shown during combat. TypeListCardPoolModel already
    // implements IModBigEnergyIconPool with a virtual null default - just override it, no need
    // to re-declare the interface. This only overrides the large background icon
    // (EnergyIconHelper.GetPath) - the small per-card cost badge still comes from EnergyColorName
    // above until we have art for that too.
    public override string? BigEnergyIconPath => "res://GunbreakerMod/images/energy.png";

    // Fallback if PoolFrameMaterial below can't be resolved for some reason.
    public override string CardFrameMaterialPath => "card_frame_blue";

    public override Color DeckEntryCardColor => new("01FCFE");

    public override bool IsColorless => false;

    // EXPERIMENTAL: recolor an existing frame material to our own brand color (#01FCFE) instead
    // of shipping full custom frame art. The vanilla .tres is packed inside SlayTheSpire2.pck so
    // its actual shader parameter name couldn't be confirmed ahead of time - this tries several
    // common naming conventions; unknown parameter names are silently ignored by Godot, so this
    // is harmless if all of them miss (frame just renders as unmodified card_frame_blue).
    // Please check in-game whether the border actually turned cyan.
    public override Material? PoolFrameMaterial
    {
        get
        {
            if (GD.Load(FrameMaterialPath) is not ShaderMaterial baseMaterial)
            {
                return null;
            }

            var recolored = (ShaderMaterial)baseMaterial.Duplicate();
            var brandColor = new Color("01FCFE");
            foreach (var paramName in new[] { "tint_color", "color", "frame_color", "albedo_color", "base_color", "modulate_color" })
            {
                recolored.SetShaderParameter(paramName, brandColor);
            }

            return recolored;
        }
    }
}
