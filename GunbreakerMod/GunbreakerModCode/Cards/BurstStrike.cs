using GunbreakerMod.GunbreakerModCode.Characters;
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

// 爆发打击 Burst Strike - 0-cost Cartridge spender, starter card.
// Two independent on-play generation effects, both confirmed against the design table:
// 1. Its own innate effect: if Cartridge was at the cap right before this card paid its 1-Cartridge
//    cost, generate another Burst Strike into hand. Secondary costs are paid before OnPlay runs
//    (confirmed by decompiling CardModel.OnPlayWrapper - it receives the already-resolved
//    ResourceInfo), so by the time OnPlay executes the 1 Cartridge is already spent; "was at cap"
//    is reconstructed as currentAmount + 1 >= maxAmount.
// 2. Continuation's effect: if the player has ContinuationPower, generate a Hypervelocity into hand.
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

        var maxCartridge = SecondaryResourceStateStore.GetMaxAmount(Owner, CartridgeResource.Id) ?? 0;
        var currentCartridge = SecondaryResourceStateStore.GetAmount(Owner, CartridgeResource.Id);
        if (currentCartridge + 1 >= maxCartridge)
        {
            var extraBurstStrike = CombatState.CreateCard<BurstStrike>(Owner);
            if (IsUpgraded)
            {
                CardCmd.Upgrade(extraBurstStrike);
            }
            await CardPileCmd.AddGeneratedCardToCombat(extraBurstStrike, PileType.Hand, Owner);
        }

        if (Owner.Creature.HasPower<ContinuationPower>())
        {
            var hypervelocity = CombatState.CreateCard<Hypervelocity>(Owner);
            if (IsUpgraded)
            {
                CardCmd.Upgrade(hypervelocity);
            }
            await CardPileCmd.AddGeneratedCardToCombat(hypervelocity, PileType.Hand, Owner);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
