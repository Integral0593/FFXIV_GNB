using GunbreakerMod.GunbreakerModCode.Powers;
using GunbreakerMod.GunbreakerModCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Cards;

// 烈牙 Gnashing Fang - opens the Gnashing Fang combo chain. Consumes 1 Cartridge, always generates
// Savage Claw (the normal chain progression). With Continuation, ALSO generates Jugular Rip - a
// separate side token, not a second Savage Claw (confirmed with the user: each Cartridge-spending
// step in this chain has its own distinct Continuation-only bonus token, paired per the design
// table's Continuation row, not a duplicate of the normal chain card).
[RegisterCard(typeof(GunbreakerCardPool))]
public sealed class GnashingFang() : ModCardTemplate(0, CardType.Attack, CardRarity.Uncommon, TargetType.AnyEnemy)
{
    public override CardAssetProfile AssetProfile => new()
    {
        PortraitPath = "res://GunbreakerMod/images/card_portraits/gnashing_fang.png",
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(8m, ValueProp.Move)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromCard<SavageClaw>()];

    // AfterCloned(), not AfterCreated() - see BurstStrike.cs for why.
    protected override void AfterCloned()
    {
        base.AfterCloned();
        this.SecondaryCosts().Set(CartridgeResource.Id, 1);
    }

    protected override bool ShouldGlowGoldInternal => CartridgeResource.HasAtLeast(Owner, 1);

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        var savageClaw = CombatState.CreateCard<SavageClaw>(Owner);
        if (IsUpgraded)
        {
            CardCmd.Upgrade(savageClaw);
        }
        await CardPileCmd.AddGeneratedCardToCombat(savageClaw, PileType.Hand, Owner);

        if (Owner.Creature.HasPower<ContinuationPower>())
        {
            var jugularRip = CombatState.CreateCard<JugularRip>(Owner);
            if (IsUpgraded)
            {
                CardCmd.Upgrade(jugularRip);
            }
            await CardPileCmd.AddGeneratedCardToCombat(jugularRip, PileType.Hand, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
