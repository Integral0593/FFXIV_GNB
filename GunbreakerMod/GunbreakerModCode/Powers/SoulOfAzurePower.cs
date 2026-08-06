using GunbreakerMod.GunbreakerModCode.Resources;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GunbreakerMod.GunbreakerModCode.Powers;

// 灵魂之青 Soul of Azure - at the start of each of the owner's turns, grants Buffer and 1 Cartridge.
// Design note ("具体刷新规则需要写代码时明确"): Buffer here ADDS to whatever's left from the previous
// turn rather than resetting to a fixed value - matches how the base game's own BufferPower stacks
// (PowerStackType.Counter, PowerCmd.Apply adds to Amount), and avoids wasting unused stacks from a
// turn where the player wasn't fully attacked.
[RegisterPower]
public sealed class SoulOfAzurePower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task AfterSideTurnStart(CombatSide side, IReadOnlyList<Creature> participants, ICombatState combatState)
    {
        if (!participants.Contains(Owner) || Owner.Player == null)
        {
            return;
        }

        var ctx = new ThrowingPlayerChoiceContext();
        await PowerCmd.Apply<BufferPower>(ctx, Owner, Amount, Owner, null);
        await SecondaryResourceCmd.Gain(Owner.Player, CartridgeResource.Id, 1, source: this);
    }
}
