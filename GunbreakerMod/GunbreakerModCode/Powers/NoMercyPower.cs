using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Powers;

// Grants Strength that lasts exactly 2 turns then fully reverts. The game's built-in
// TemporaryStrengthPower (used by e.g. Flex Potion) only lasts until the end of the CURRENT
// turn, not a configurable number of turns, so this reimplements its "internally apply
// StrengthPower, revert on removal" pattern with our own turn counter.
[RegisterPower]
public sealed class NoMercyPower : ModPowerTemplate
{
    private int _turnsRemaining = 2;

    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Counter;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GunbreakerMod/images/powers/no_mercy_power.png",
        BigIconPath: "res://GunbreakerMod/images/powers/no_mercy_power_big.png");

    public override async Task BeforeApplied(Creature target, decimal amount, Creature? applier, CardModel? cardSource)
    {
        await PowerCmd.Apply<StrengthPower>(new ThrowingPlayerChoiceContext(), target, amount, applier, cardSource, silent: true);
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
            await PowerCmd.Apply<StrengthPower>(choiceContext, Owner, -Amount, Owner, null);
        }
    }
}
