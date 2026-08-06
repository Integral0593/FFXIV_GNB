using Godot;
using GunbreakerMod.GunbreakerModCode.Cards;
using MegaCrit.Sts2.Core.Entities.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;

namespace GunbreakerMod.GunbreakerModCode.Characters;

// Framework-only pass. Character-select identity (icon/locked-icon/top-panel icon/select
// background/map marker) AND the in-combat body use our own mod-owned placeholder assets,
// not Ironclad's. Confirmed by decompiling RitsuLib's runtime factory patches: setting
// Scenes.VisualsPath to a plain PNG is safe (CharacterCreatureVisualsRuntimeFactoryPatch
// auto-wraps a Texture2D into a valid NCreatureVisuals via RitsuGodotNodeFactories -
// same for the Ui paths above, which all accept a PackedScene OR a Texture2D).
//
// EnergyCounterPath / MerchantAnimPath / RestSiteAnimPath / Spine / trail vfx / sfx are
// deliberately left unset (falling back to Ironclad via PlaceholderCharacterId): their
// runtime factory patches (e.g. CharacterEnergyCounterRuntimeFactoryPatch) only accept a
// real PackedScene - pointing them at a placeholder PNG would fail ResolveScene() and fall
// through to the game's own direct-instantiate path, which is what crashed for Squall's
// broken character-select scene earlier. Revisit once we either have real art or invest in
// building actual placeholder .tscn scenes for these.
//
// RequiresEpochAndTimeline is intentionally left at its default (true) - NOT overridden to
// false. That was the actual root cause of the "always ends up as Ironclad" bug: the game
// log showed SelectCharacter_Patch4 throwing ArgumentOutOfRangeException the moment this
// character was clicked (right around the Ascension-epoch check), which silently aborted
// the selection and left the game on whatever character was selected before. The RitsuLib
// docs are explicit that RequiresEpochAndTimeline=false is only for characters that do NOT
// go through the normal character-select UI - it is not "opt out of Ancient dialogue", it's
// "opt out of the whole ascension/timeline integration every normally selectable character
// needs". No custom ModEpochTemplate/story content is required for this to work - the base
// game's own epoch/ascension system handles a plain playable character generically, the
// same way it does for vanilla Ironclad.
//
// Starting deck comes from [RegisterCharacterStarterCard] on the cards themselves
// (Strike_GNB x3, Defend_GNB x4); starting relic from [RegisterCharacterStarterRelic] on
// GunbreakerStarterRelic.
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
            // Top-left run/combat HUD portrait - uses the avatar art specifically, per request.
            // Both IconTexturePath (top_panel context) and IconPath (character_icons scene
            // context, texture-fallback also supported) point at it since it wasn't clear which
            // one is the exact element being seen; safe to cover both.
            IconTexturePath = "res://GunbreakerMod/images/map_marker.png",
            IconOutlineTexturePath = "res://GunbreakerMod/images/map_marker.png",
            IconPath = "res://GunbreakerMod/images/map_marker.png",
            MapMarkerPath = "res://GunbreakerMod/images/map_marker.png",
        },
        Scenes = new()
        {
            // Real character art (no animation rig yet, per user - static image is fine for now).
            VisualsPath = "res://GunbreakerMod/images/gunbreaker_char.png",
        },
    };

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
