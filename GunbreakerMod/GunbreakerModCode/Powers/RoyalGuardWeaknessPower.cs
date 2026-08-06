using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GunbreakerMod.GunbreakerModCode.Powers;

// Strength-down applied to an attacker by Royal Guard. Per user feedback, permanently-stacking
// Strength loss on every attack was too strong - this instead lasts through the attacker's next
// turn, then fully reverts. Mirrors NoMercyPower's "apply Strength, revert on removal" pattern,
// with the countdown reset on every additional stack (so repeated attacks keep the debuff alive
// rather than each attack running its own independent timer). Uses AfterPowerAmountChanged for
// re-stacks because BeforeApplied only fires on the very first application to a given target
// (confirmed via decompiling PowerModel's own doc comments) - subsequent attacks within the same
// combat need the second hook to actually apply their share of the Strength reduction.
[RegisterPower]
public sealed class RoyalGuardWeaknessPower : PowerModel
{
    private int _turnsRemaining = 2;

    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    // Only the underlying StrengthPower debuff should show on the attacker - see
    // HeartOfStoneStrengthDownPower for why (avoids a second, redundant power icon).
    protected override bool IsVisibleInternal => false;

    public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        _turnsRemaining = 2;
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), target, -amount, applier, cardSource, silent: true);
    }

    public override async Task AfterPowerAmountChanged(
        PlayerChoiceContext choiceContext, PowerModel power, decimal amount, Creature? applier, CardModel? cardSource)
    {
        if (power == this && amount != Amount)
        {
            _turnsRemaining = 2;
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, -amount, applier, cardSource, silent: true);
        }
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner))
        {
            return;
        }

        _turnsRemaining--;
        if (_turnsRemaining <= 0)
        {
            await PowerCmd.Remove(this);
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, Amount, Owner, null);
        }
    }
}
