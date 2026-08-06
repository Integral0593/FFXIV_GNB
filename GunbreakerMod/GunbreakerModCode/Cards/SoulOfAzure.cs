using GunbreakerMod.GunbreakerModCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Cards;

// 灵魂之青 Soul of Azure - high-cost persistent Power, dual resource output per turn (Buffer +
// Cartridge). Matches the design table's comparison to Mental Fortress-style expensive standing
// Powers.
[RegisterCard(typeof(GunbreakerCardPool))]
public sealed class SoulOfAzure() : ModCardTemplate(3, CardType.Power, CardRarity.Rare, TargetType.Self)
{
    public override CardAssetProfile AssetProfile => new()
    {
        PortraitPath = "res://GunbreakerMod/images/card_portraits/soul_of_azure.png",
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Buffer", 2m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<SoulOfAzurePower>(choiceContext, Owner.Creature, DynamicVars["Buffer"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Buffer"].UpgradeValueBy(1m);
    }
}
