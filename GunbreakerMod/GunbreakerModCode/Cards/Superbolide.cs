using GunbreakerMod.GunbreakerModCode.Powers;
using GunbreakerMod.GunbreakerModCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Cards;

// 超火流星 Superbolide - lose half current HP, take no damage this turn. HP loss is applied BEFORE
// the immunity power (order matters: the immunity would otherwise block this card's own HP cost
// too, since Buffer intercepts all HP loss, not just attack damage). Uses ValueProp.Unblockable
// (the "HP loss like Poison" flag) so it isn't affected by Block.
[RegisterCard(typeof(GunbreakerCardPool))]
public sealed class Superbolide() : ModCardTemplate(0, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override CardAssetProfile AssetProfile => new()
    {
        PortraitPath = "res://GunbreakerMod/images/card_portraits/superbolide.png",
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("HPLossPercent", 50m)];

    // AfterCloned(), not AfterCreated() - see BurstStrike.cs for why.
    protected override void AfterCloned()
    {
        base.AfterCloned();
        this.SecondaryCosts().Set(CartridgeResource.Id, 2);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        var hpLoss = (int)Math.Ceiling(Owner.Creature.CurrentHp * (DynamicVars["HPLossPercent"].BaseValue / 100m));
        await CreatureCmd.Damage(choiceContext, Owner.Creature, hpLoss, ValueProp.Unblockable, this, cardPlay);

        await PowerCmd.Apply<SuperbolideImmunityPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        AddKeyword(CardKeyword.Retain);
    }
}
