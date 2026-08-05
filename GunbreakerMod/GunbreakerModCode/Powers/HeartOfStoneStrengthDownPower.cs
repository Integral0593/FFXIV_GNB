using GunbreakerMod.GunbreakerModCode.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;

// AbstractModel and ModelDb both live in MegaCrit.Sts2.Core.Models, imported explicitly above -
// unlike the base game's own TemporaryStrengthPower subclasses, this mod's Powers namespace isn't
// nested under MegaCrit.Sts2.Core.Models, so the unqualified references there don't resolve for free.

namespace GunbreakerMod.GunbreakerModCode.Powers;

// Strength-down half of Heart of Stone's effect. TemporaryStrengthPower is abstract in the base game
// (subclassed per source, e.g. FlexPotionPower/MonarchsGazeStrengthDownPower - confirmed via decompile),
// so this mirrors that pattern instead of trying to apply it directly. It already reverts automatically
// at end of the current turn (AfterSideTurnEnd in the base class), matching the design's "this turn only".
[RegisterPower]
public sealed class HeartOfStoneStrengthDownPower : TemporaryStrengthPower
{
    public override AbstractModel OriginModel => ModelDb.Card<HeartOfStone>();

    protected override bool IsPositive => false;
}
