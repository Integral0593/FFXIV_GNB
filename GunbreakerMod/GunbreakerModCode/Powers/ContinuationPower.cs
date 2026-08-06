using MegaCrit.Sts2.Core.Entities.Powers;
using MegaCrit.Sts2.Core.Models;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Powers;

// 续剑 Continuation - presence-only marker power (StackType.Single: amount is hidden, always 1).
// Cards that spend Cartridge check Owner.Creature.HasPower<ContinuationPower>() in their own OnPlay
// to decide whether to generate their paired follow-up card (see BurstStrike -> Hypervelocity).
// No shared hook/registry: each future Cartridge-spending card (Gnashing Fang chain, Fated Circle,
// etc.) adds its own check, matching the codebase's existing per-card generation pattern already used
// by KeenEdge/BrutalShell/SolidBarrel.
[RegisterPower]
public sealed class ContinuationPower : ModPowerTemplate
{
    public override PowerType Type => PowerType.Buff;

    public override PowerStackType StackType => PowerStackType.Single;

    public override PowerAssetProfile AssetProfile => new(
        IconPath: "res://GunbreakerMod/images/powers/continuation_power.png",
        BigIconPath: "res://GunbreakerMod/images/powers/continuation_power_big.png");
}
