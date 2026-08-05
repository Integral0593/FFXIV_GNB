using GunbreakerMod.GunbreakerModCode.Cards;
using GunbreakerMod.GunbreakerModCode.Characters;
using MegaCrit.Sts2.Core.Entities.Relics;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Relics;

// Placeholder starter relic: no gameplay effect yet, just satisfies the game's requirement
// that every character have at least one starting relic (NCharacterSelectScreen reads
// StartingRelics[0] unconditionally when populating the character-select info panel).
[RegisterRelic(typeof(GunbreakerRelicPool))]
[RegisterCharacterStarterRelic(typeof(Gunbreaker), 1)]
public sealed class GunbreakerStarterRelic : ModRelicTemplate
{
    public override RelicRarity Rarity => RelicRarity.Starter;

    public override RelicAssetProfile AssetProfile => new()
    {
        IconPath = "res://GunbreakerMod/images/relic_starter_icon.png",
        IconOutlinePath = "res://GunbreakerMod/images/relic_starter_icon_outline.png",
        BigIconPath = "res://GunbreakerMod/images/relic_starter_icon_big.png",
    };
}
