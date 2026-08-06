using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Cards;

// 支配之心 Noble Blood - middle of the Finisher chain. Puts Lion Heart on top of the draw pile.
[RegisterCard(typeof(GunbreakerCardPool))]
public sealed class NobleBlood() : ModCardTemplate(2, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
{
    public override CardAssetProfile AssetProfile => new()
    {
        PortraitPath = "res://GunbreakerMod/images/card_portraits/noble_blood.png",
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar("DamageMain", 35m, ValueProp.Move), new DamageVar("DamageSplash", 7m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars["DamageMain"].BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        foreach (var enemy in CombatState.GetOpponentsOf(Owner.Creature))
        {
            if (enemy == cardPlay.Target)
            {
                continue;
            }
            await DamageCmd.Attack(DynamicVars["DamageSplash"].BaseValue)
                .FromCard(this, cardPlay)
                .Targeting(enemy)
                .Execute(choiceContext);
        }

        var lionHeart = CombatState.CreateCard<LionHeart>(Owner);
        if (IsUpgraded)
        {
            CardCmd.Upgrade(lionHeart);
        }
        await CardPileCmd.AddGeneratedCardToCombat(lionHeart, PileType.Draw, Owner, CardPilePosition.Top);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DamageMain"].UpgradeValueBy(5m);
        DynamicVars["DamageSplash"].UpgradeValueBy(1m);
    }
}
