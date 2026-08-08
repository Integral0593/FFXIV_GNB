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
    // (EnergyIconHelper.GetPath).
    public override string? BigEnergyIconPath => "res://GunbreakerMod/images/energy.png";

    // The small inline energy icon embedded in rich-text (card/relic descriptions referencing
    // {Energy}, hover tips, etc.) - separate system from BigEnergyIconPath above
    // (IModTextEnergyIconPool, confirmed via decompile). Without this, that inline icon still
    // fell back to the base game's "{EnergyColorName}_energy_icon.png" convention, which for our
    // borrowed EnergyColorName "defect" meant Defect's own blue icon (and even then that
    // particular sprite-font asset doesn't exist for us, logging "Asset not cached" warnings).
    // Uses a dedicated small (48x48, matching cartridge_icon_small.png's size) crop of the energy
    // art, NOT the full energy.png - the [img] BBCode tag this feeds into has no size constraint,
    // so it renders at the source image's native pixel size; the full 256x256 background art
    // rendered giant inline in card text.
    public override string? TextEnergyIconPath => "res://GunbreakerMod/images/energy_icon_small.png";

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
