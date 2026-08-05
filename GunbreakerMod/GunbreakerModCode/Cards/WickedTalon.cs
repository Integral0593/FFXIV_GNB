using GunbreakerMod.GunbreakerModCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Cards;

// 凶禽爪 Wicked Talon - end of the Gnashing Fang chain. No normal-chain follow-up (per the design
// table, its base effect is pure damage); with Continuation, generates Eye Gouge.
[RegisterCard(typeof(GunbreakerCardPool))]
public sealed class WickedTalon() : ModCardTemplate(0, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
{
    public override CardAssetProfile AssetProfile => new()
    {
        PortraitPath = "res://GunbreakerMod/images/card_portraits/wicked_talon.png",
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(12m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        if (Owner.Creature.HasPower<ContinuationPower>())
        {
            var eyeGouge = CombatState.CreateCard<EyeGouge>(Owner);
            if (IsUpgraded)
            {
                CardCmd.Upgrade(eyeGouge);
            }
            await CardPileCmd.AddGeneratedCardToCombat(eyeGouge, PileType.Hand, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
