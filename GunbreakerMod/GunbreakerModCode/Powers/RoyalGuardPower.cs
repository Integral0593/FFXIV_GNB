using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Powers;

// 王室亲卫 Royal Guard - whenever an attack lands on the owner, the attacking creature loses
// Strength for its next turn (not permanently - see RoyalGuardWeaknessPower). Mirrors the base
// game's ThornsPower for the reaction hook (same BeforeDamageReceived, same props.IsPoweredAttack()
// guard to only react to real attacks); the actual Strength change is delegated to
// RoyalGuardWeaknessPower on the attacker, which owns the temporary-duration logic.
[RegisterPower]
public sealed class RoyalGuardPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GunbreakerMod/images/powers/royal_guard_power.png",
        BigIconPath: "res://GunbreakerMod/images/powers/royal_guard_power_big.png");

    public override async Task BeforeDamageReceived(
        PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner && dealer != null && props.IsPoweredAttack())
        {
            Flash();
            await PowerCmd.Apply<RoyalGuardWeaknessPower>(choiceContext, dealer, Amount, Owner, null);
        }
    }
}
