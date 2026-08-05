using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;

namespace GunbreakerMod.GunbreakerModCode.Powers;

// 王室亲卫 Royal Guard - whenever an attack lands on the owner, the attacking creature loses
// Strength. Mirrors the base game's ThornsPower (same BeforeDamageReceived hook, same
// props.IsPoweredAttack() guard to only react to real attacks), just targeting Strength on the
// dealer instead of dealing damage back.
[RegisterPower]
public sealed class RoyalGuardPower : PowerModel
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override async Task BeforeDamageReceived(
        PlayerChoiceContext choiceContext, Creature target, decimal amount, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner && dealer != null && props.IsPoweredAttack())
        {
            Flash();
            await PowerCmd.Apply<StrengthPower>(choiceContext, dealer, -Amount, Owner, null);
        }
    }
}
