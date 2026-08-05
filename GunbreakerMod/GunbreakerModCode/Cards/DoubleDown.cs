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

// 倍攻 Double Down - the one Cartridge-spending attack that deliberately does NOT interact with
// Continuation (per design: it's the only attack costing 2 Cartridge instead of 1, and has no
// designated follow-up token), so unlike BurstStrike/GnashingFang-family cards, its OnPlay has no
// HasPower<ContinuationPower>() check at all.
[RegisterCard(typeof(GunbreakerCardPool))]
public sealed class DoubleDown() : ModCardTemplate(0, CardType.Attack, CardRarity.Rare, TargetType.AllEnemies)
{
    public override CardAssetProfile AssetProfile => new()
    {
        PortraitPath = "res://GunbreakerMod/images/card_portraits/double_down.png",
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(40m, ValueProp.Move)];

    protected override bool ShouldGlowGoldInternal => CartridgeResource.HasAtLeast(Owner, 2);

    public override void AfterCreated()
    {
        base.AfterCreated();
        this.SecondaryCosts().Set(CartridgeResource.Id, 2);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(10m);
    }
}
