using Godot;
using GunbreakerMod.GunbreakerModCode.Cards;
using MegaCrit.Sts2.Core.Entities.Characters;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Characters;

namespace GunbreakerMod.GunbreakerModCode.Characters;

// Framework-only pass: no custom art yet, so AssetProfile is left at its default (empty),
// which makes ModCharacterTemplate fall back to Ironclad's visuals/animations/icons entirely
// via PlaceholderCharacterId. Epoch/Timeline (Ancients) content is opted out of below, since
// none is designed yet. Starting deck comes from [RegisterCharacterStarterCard] on the cards
// themselves (Strike_GNB x3, Defend_GNB x4); no starting relic yet.
[RegisterCharacter]
public sealed class Gunbreaker : ModCharacterTemplate<GunbreakerCardPool, GunbreakerRelicPool, GunbreakerPotionPool>
{
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
