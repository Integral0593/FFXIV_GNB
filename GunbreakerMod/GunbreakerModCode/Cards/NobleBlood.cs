using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.CardTargeting;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Cards;

// 支配之心 Noble Blood - middle of the Terminal Trigger chain. Puts Lion Heart on top of the draw pile.
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

        // See ReignOfBeasts.cs for why this uses TargetingFiltered (instead of a manual loop) and
        // .ToList() (instead of a lazy .Where() over a live, mutating collection).
        var splashTargets = CombatState.GetOpponentsOf(Owner.Creature).Where(enemy => enemy != cardPlay.Target).ToList();
        await DamageCmd.Attack(DynamicVars["DamageSplash"].BaseValue)
            .FromCard(this, cardPlay)
            .TargetingFiltered(splashTargets)
            .Execute(choiceContext);

        var lionHeart = CombatState.CreateCard<LionHeart>(Owner);
        if (IsUpgraded)
        {
            CardCmd.Upgrade(lionHeart);
        }
        // See ReignOfBeasts.cs for why this goes through Hand before moving to the draw-pile top.
        await CardPileCmd.AddGeneratedCardToCombat(lionHeart, PileType.Hand, Owner);
        await Cmd.Wait(0.75f);
        await CardPileCmd.Add(lionHeart, PileType.Draw, CardPilePosition.Top);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DamageMain"].UpgradeValueBy(5m);
        DynamicVars["DamageSplash"].UpgradeValueBy(1m);
    }
}
