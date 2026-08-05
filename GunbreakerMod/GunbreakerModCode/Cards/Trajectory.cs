using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Cards;

// 弹道 Trajectory - pure card draw. Common rarity (the design table originally listed this as
// Basic, which the user confirmed was a typo - Basic-rarity cards never appear in the reward pool,
// see the CardFactory.CreateForReward rarity-escalation ring documented on DemonSlice/Bloodfest).
[RegisterCard(typeof(GunbreakerCardPool))]
public sealed class Trajectory() : ModCardTemplate(1, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override CardAssetProfile AssetProfile => new()
    {
        PortraitPath = "res://GunbreakerMod/images/card_portraits/trajectory.png",
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("Draw", 3m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CardPileCmd.Draw(choiceContext, DynamicVars["Draw"].BaseValue, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["Draw"].UpgradeValueBy(1m);
    }
}
