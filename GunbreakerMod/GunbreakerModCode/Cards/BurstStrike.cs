using GunbreakerMod.GunbreakerModCode.Characters;
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

// 爆发打击 Burst Strike - 0-cost Cartridge spender, starter card.
// TODO: design also calls for "if your Cartridge is at the cap, generate a Burst Strike in
// hand" as a passive trigger whenever Cartridge reaches 3 by ANY means, not just from playing
// this card. That needs an ISecondaryResourceHookListener watching every Cartridge gain across
// the whole mod, which is a bigger separate piece of infrastructure - deferred for now, only
// the base damage + Cartridge cost is implemented here.
[RegisterCard(typeof(GunbreakerCardPool))]
[RegisterCharacterStarterCard(typeof(Gunbreaker), 1)]
public sealed class BurstStrike() : ModCardTemplate(0, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
{
    public override CardAssetProfile AssetProfile => new()
    {
        PortraitPath = "res://GunbreakerMod/images/card_portraits/burst_strike.png",
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move)];

    public override void AfterCreated()
    {
        base.AfterCreated();
        this.SecondaryCosts().Set(CartridgeResource.Id, 1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
