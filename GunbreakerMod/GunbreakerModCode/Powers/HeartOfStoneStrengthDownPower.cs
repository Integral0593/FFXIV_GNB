using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GunbreakerMod.GunbreakerModCode.Powers;

// Strength-down half of Heart of Stone's effect, lasting only until the end of the current turn.
// Mirrors NoMercyPower's own "apply Strength, revert on removal" pattern instead of subclassing the
// base game's abstract TemporaryStrengthPower - that path pulls in vanilla's own Title/Description/
// HoverTip resolution via OriginModel, which didn't render cleanly for a modded card in testing.
// IsVisibleInternal = false so only the underlying StrengthPower debuff shows on the enemy - this
// tracking power still applies/reverts Strength normally, it just doesn't render its own separate
// icon (per user feedback: the enemy should show exactly one debuff, not two stacked icons).
[RegisterPower]
public sealed class HeartOfStoneStrengthDownPower : PowerModel
{
    public override PowerType Type => PowerType.Debuff;

    public override PowerStackType StackType => PowerStackType.Counter;

    protected override bool IsVisibleInternal => false;

    public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), target, -amount, applier, cardSource, silent: true);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner))
        {
            return;
        }

        await PowerCmd.Remove(this);
        await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, Amount, Owner, null);
    }
}
