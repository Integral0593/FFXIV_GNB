using Godot;
using GunbreakerMod.GunbreakerModCode.Cards;
using MegaCrit.Sts2.Core.Entities.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;
using STS2RitsuLib.Scaffolding.Characters.Visuals.Definition;
using STS2RitsuLib.Scaffolding.Visuals.Definition;

namespace GunbreakerMod.GunbreakerModCode.Characters;

// Framework-only pass. Character-select identity (icon/locked-icon/top-panel icon/select
// background/map marker) AND the in-combat body use our own mod-owned placeholder assets,
// not Ironclad's. Confirmed by decompiling RitsuLib's runtime factory patches: setting
// Scenes.VisualsPath to a plain PNG is safe (CharacterCreatureVisualsRuntimeFactoryPatch
// auto-wraps a Texture2D into a valid NCreatureVisuals via RitsuGodotNodeFactories -
// same for the Ui paths above, which all accept a PackedScene OR a Texture2D).
//
// EnergyCounterPath / Spine / trail vfx / sfx are still deliberately left unset (falling back
// to Ironclad via PlaceholderCharacterId). EnergyCounterPath genuinely needs a real .tscn
// (RitsuNEnergyCounterNodeFactory.CreateBareFromResourceImpl explicitly throws
// NotSupportedException for a plain texture, confirmed via decompile - unlike VisualsPath/
// Merchant/RestSite, no texture-only path exists here). A hand-written .tscn was tried and even
// compiled fine, but this project's build pipeline packs mod assets with a lightweight custom
// packer (BSchneppe.StS2.PckPacker) rather than a real Godot editor export, and that packer
// flatly refuses any .tscn: `dotnet build` prints "PCK packing skipped - unsupported files
// detected: energy_counter.tscn" and, worse, skips packing EVERYTHING else in the same build
// when it hits one. Reverted rather than leave that in the tree. Doing this for real would need
// the actual Godot editor to export the mod instead of this project's normal dotnet-build flow -
// a bigger workflow change than just adding art, so flagging it rather than deciding
// unilaterally.
//
// Merchant/rest-site DON'T need a real .tscn though - RitsuLib's WorldProceduralVisuals lets a
// mod supply plain static textures per named "cue" instead of an animated scene
// (CharacterWorldProceduralVisualSetBuilder -> VisualCueSet, confirmed via decompile). The cue
// keys aren't arbitrary: "relaxed_loop" is the literal animation name NMerchantCharacter plays
// for its idle state, and "overgrowth_loop"/"hive_loop"/"glory_loop" are the base game's
// per-Act rest-site loop names (RitsuLib's own doc comment on CharacterRestSiteWorldDefinition
// lists them) - mapping all three to the same static image just means our rest pose looks the
// same in every Act instead of changing.
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
        // Merchant art was rendering small and too low with no style override at all (user
        // screenshot), so it's not just a matter of resizing the source texture - the node this
        // plugs into applies some default placement of its own. Re-exported the source art bigger
        // (500x697, versus the 287x400 used for combat/merchant last round - that smaller size
        // was specifically to dodge the combat hitbox-sync crash, which doesn't apply to the
        // merchant room) and nudged it up with an Offset. This is a first-pass guess, not a
        // decompiled-exact value - needs an in-game screenshot to dial in.
        WorldProceduralVisuals = CharacterWorldProceduralVisualSetBuilder.Create()
            .Merchant(cues => cues.Single(
                "relaxed_loop",
                "res://GunbreakerMod/images/gunbreaker_merchant.png",
                VisualNodeStyle.Create(offset: new Vector2(0f, -160f))))
            .RestSite(cues => cues
                .Single("overgrowth_loop", "res://GunbreakerMod/images/gunbreaker_rest.png")
                .Single("hive_loop", "res://GunbreakerMod/images/gunbreaker_rest.png")
                .Single("glory_loop", "res://GunbreakerMod/images/gunbreaker_rest.png"))
            .Build(),
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
