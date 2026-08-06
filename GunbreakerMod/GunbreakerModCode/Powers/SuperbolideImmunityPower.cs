using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Powers;

// Backs Superbolide's "take no damage this turn". Previously implemented by granting a large
// BufferPower stack, but Buffer only blocks HP loss once per stack point - two separate attacks
// in the same turn would burn two stacks, and a big enough hit sequence could still exhaust it.
// That's not true invincibility. Rebuilt to mirror the base game's own IntangiblePower (confirmed
// via decompile): IntangiblePower caps every hit's damage at 1 via ModifyHpLostAfterOsty (an
// unconditional per-hit cap, not a depleting counter), then decrements exactly once when the
// enemy's turn ends. This does the same thing but caps at 0 instead of 1, giving genuine
// "block every hit this turn, no matter how many" immunity.
[RegisterPower]
public sealed class SuperbolideImmunityPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GunbreakerMod/images/powers/superbolide_immunity_power.png",
        BigIconPath: "res://GunbreakerMod/images/powers/superbolide_immunity_power_big.png");

    public override decimal ModifyHpLostAfterOsty(Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        return target != Owner ? amount : 0m;
    }

    public override decimal ModifyDamageCap(Creature? target, ValueProp props, Creature? dealer, CardModel? cardSource, CardPlay? cardPlay)
    {
        return target != Owner ? decimal.MaxValue : 0m;
    }

    public override async Task AfterSideTurnEnd(PlayerChoiceContext choiceContext, CombatSide side, IEnumerable<Creature> participants)
    {
        if (side == CombatSide.Enemy)
        {
            await PowerCmd.Decrement(this);
        }
    }
}
