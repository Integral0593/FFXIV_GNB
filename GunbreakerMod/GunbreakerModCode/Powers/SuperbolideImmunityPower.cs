using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GunbreakerMod.GunbreakerModCode.Powers;

// Backs Superbolide's "take no damage this turn". The base game's BufferPower (confirmed via
// decompile) already blocks HP loss entirely and decrements once per instance absorbed - applying a
// large stack (comfortably more than any single turn could realistically land hits) gets "no damage
// this turn" for free without a bespoke damage-immunity power. This wrapper just grants that stack
// and guarantees any of it left over gets cleared at the end of the turn it was granted, since Buffer
// itself has no turn-based expiry (only decrements when it actually blocks something).
[RegisterPower]
public sealed class SuperbolideImmunityPower : PowerModel
{
    private const int BufferStacks = 99;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    // Only the underlying Buffer power's icon should show - see HeartOfStoneStrengthDownPower for
    // why (avoids a second, redundant power icon).
    protected override bool IsVisibleInternal => false;

    public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        await PowerCmd.Apply<BufferPower>(new ThrowingPlayerChoiceContext(), target, BufferStacks, applier, cardSource, silent: true);
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (!participants.Contains(Owner))
        {
            return;
        }

        await PowerCmd.Remove(this);
        var leftoverBuffer = Owner.GetPower<BufferPower>();
        if (leftoverBuffer != null)
        {
            await PowerCmd.Remove(leftoverBuffer);
        }
    }
}
