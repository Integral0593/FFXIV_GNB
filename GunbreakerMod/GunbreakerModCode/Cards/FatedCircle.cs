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

// 命运之环 Fated Circle - AoE Cartridge spender. Unlike Gnashing Fang, it has no normal-chain
// follow-up of its own; Fated Brand only appears with Continuation.
[RegisterCard(typeof(GunbreakerCardPool))]
public sealed class FatedCircle() : ModCardTemplate(0, CardType.Attack, CardRarity.Uncommon, TargetType.AllEnemies)
{
    public override CardAssetProfile AssetProfile => new()
    {
        PortraitPath = "res://GunbreakerMod/images/card_portraits/fated_circle.png",
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move)];

    protected override bool ShouldGlowGoldInternal => CartridgeResource.HasAtLeast(Owner, 1);

    // AfterCloned(), not AfterCreated() - see BurstStrike.cs for why.
    protected override void AfterCloned()
    {
        base.AfterCloned();
        this.SecondaryCosts().Set(CartridgeResource.Id, 1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState)
            .Execute(choiceContext);

        if (Owner.Creature.HasPower<ContinuationPower>())
        {
            var fatedBrand = CombatState.CreateCard<FatedBrand>(Owner);
            if (IsUpgraded)
            {
                CardCmd.Upgrade(fatedBrand);
            }
            await CardPileCmd.AddGeneratedCardToCombat(fatedBrand, PileType.Hand, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
