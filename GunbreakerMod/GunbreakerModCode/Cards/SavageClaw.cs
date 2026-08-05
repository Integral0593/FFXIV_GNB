using GunbreakerMod.GunbreakerModCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Cards;

// 猛兽爪 Savage Claw - middle of the Gnashing Fang chain. Always generates Wicked Talon; with
// Continuation, also generates Abdomen Tear (its own distinct side token).
[RegisterCard(typeof(GunbreakerCardPool))]
public sealed class SavageClaw() : ModCardTemplate(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
{
    public override CardAssetProfile AssetProfile => new()
    {
        PortraitPath = "res://GunbreakerMod/images/card_portraits/savage_claw.png",
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(10m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        var wickedTalon = CombatState.CreateCard<WickedTalon>(Owner);
        if (IsUpgraded)
        {
            CardCmd.Upgrade(wickedTalon);
        }
        await CardPileCmd.AddGeneratedCardToCombat(wickedTalon, PileType.Hand, Owner);

        if (Owner.Creature.HasPower<ContinuationPower>())
        {
            var abdomenTear = CombatState.CreateCard<AbdomenTear>(Owner);
            if (IsUpgraded)
            {
                CardCmd.Upgrade(abdomenTear);
            }
            await CardPileCmd.AddGeneratedCardToCombat(abdomenTear, PileType.Hand, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
