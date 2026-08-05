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
// Two independent effects, both confirmed against the design table:
// 1. Continuation's effect (on play): if the player has ContinuationPower, generate a Hypervelocity
//    into hand.
// 2. Its own innate effect (reactive, NOT tied to playing this card): whenever Cartridge reaches its
//    cap by any means, move this exact card into hand from wherever it currently sits - no new copy,
//    deck size never changes. Implemented via ISecondaryResourceHookListener.AfterSecondaryResourceChanged,
//    which RitsuLib dispatches to every card in the player's deck whose Pile is a combat pile
//    (confirmed via decompiling CardModel.ShouldReceiveCombatHooks and SecondaryResourceHook's
//    listener dispatch) - so this fires regardless of which card caused the gain, not just self-play.
[RegisterCard(typeof(GunbreakerCardPool))]
[RegisterCharacterStarterCard(typeof(Gunbreaker), 1)]
public sealed class BurstStrike() : ModCardTemplate(0, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy),
    ISecondaryResourceHookListener
{
    public override CardAssetProfile AssetProfile => new()
    {
        PortraitPath = "res://GunbreakerMod/images/card_portraits/burst_strike.png",
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(7m, ValueProp.Move)];

    // Gold glow when affordable, mirroring the base game's GrandFinale (ShouldGlowGoldInternal tied
    // to its own special-condition check, confirmed via decompile).
    protected override bool ShouldGlowGoldInternal => CartridgeResource.HasAtLeast(Owner, 1);

    // Secondary costs are stored in an AttachedState dictionary keyed by CardModel instance, not
    // copied by the base game's own clone machinery - AfterCreated() only fires on the specific
    // instance it's called on, and paths like combat-instance cloning (RunState.CloneCard, used by
    // CombatState.CreateCard) never call it again on the clone. AfterCloned() is CardModel's actual
    // universal clone hook (confirmed via decompiling AbstractModel.MutableClone: DeepCloneFields()
    // then AfterCloned(), unconditionally, for every ToMutable()/ClonePreservingMutability() call) -
    // re-applying the cost here instead of AfterCreated() is what was missing a Cartridge requirement
    // on cards played from a fresh per-combat clone.
    protected override void AfterCloned()
    {
        base.AfterCloned();
        this.SecondaryCosts().Set(CartridgeResource.Id, 1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

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

    public async Task AfterSecondaryResourceChanged(SecondaryResourceChangeContext context)
    {
        if (context.Definition.Id != CartridgeResource.Id || context.Player != Owner)
        {
            return;
        }
        if (context.NewAmount < (context.Definition.BaseMaxAmount ?? int.MaxValue))
        {
            return;
        }
        if (Pile?.Type is PileType.Draw or PileType.Discard)
        {
            await CardPileCmd.Add(this, PileType.Hand);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
