using Godot;
using GunbreakerMod.GunbreakerModCode.Cards;
using MegaCrit.Sts2.Core.Entities.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;

namespace GunbreakerMod.GunbreakerModCode.Characters;

// Framework-only pass. Character-select identity (icon/locked-icon/top-panel icon/select
// background/map marker) uses our own mod-owned placeholder assets, NOT Ironclad's - reusing
// Ironclad's literal res:// paths there made this character visually indistinguishable from
// the real Ironclad in the character-select screen (confirmed via game logs: the actual run
// that started was CHARACTER.IRONCLAD, not ours - the user was clicking the vanilla character
// by mistake because the icons were identical).
//
// Everything NOT overridden below (in-combat Spine body, energy counter scene, merchant/rest-site
// anim, trail vfx, sfx) still falls back to Ironclad via PlaceholderCharacterId. That's lower risk
// to leave aliased for now since it doesn't affect character *selection*, only in-combat rendering,
// and building a safe custom NCreatureVisuals node tree without a real Godot scene is nontrivial.
// Epoch/Timeline (Ancients) content is opted out of below, since none is designed yet. Starting
// deck comes from [RegisterCharacterStarterCard] on the cards themselves (Strike_GNB x3,
// Defend_GNB x4); no starting relic yet.
[RegisterCharacter]
public sealed class Gunbreaker : ModCharacterTemplate<GunbreakerCardPool, GunbreakerRelicPool, GunbreakerPotionPool>
{
    public override CharacterAssetProfile AssetProfile => new()
    {
        Ui = new()
        {
            CharacterSelectBgPath = "res://GunbreakerMod/images/character_select_bg.png",
            CharacterSelectIconPath = "res://GunbreakerMod/images/character_select_icon.png",
            CharacterSelectLockedIconPath = "res://GunbreakerMod/images/character_select_icon_locked.png",
            IconTexturePath = "res://GunbreakerMod/images/icon.png",
            IconOutlineTexturePath = "res://GunbreakerMod/images/icon_outline.png",
            MapMarkerPath = "res://GunbreakerMod/images/map_marker.png",
        },
    };

    public override bool RequiresEpochAndTimeline => false;

    public override CharacterGender Gender => CharacterGender.Neutral;

    // Placeholder accent color, matches the card pool's placeholder steel-blue theme.
    public override Color NameColor => new("3EB3ED");

    // Placeholder values, borrowed from Ironclad alongside the placeholder visuals.
    public override int StartingHp => 80;
    public override int StartingGold => 99;
    public override float AttackAnimDelay => 0.15f;
    public override float CastAnimDelay => 0.25f;

    public override List<string> GetArchitectAttackVfx() =>
    [
        "vfx/vfx_attack_blunt",
        "vfx/vfx_heavy_blunt",
        "vfx/vfx_attack_slash",
        "vfx/vfx_bloody_impact",
        "vfx/vfx_rock_shatter",
    ];
}
