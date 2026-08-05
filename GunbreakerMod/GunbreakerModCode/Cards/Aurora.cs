using GunbreakerMod.GunbreakerModCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Cards;

// 极光 Aurora - 0-cost, consumes 1 Cartridge, grants Regeneration. Base game's RegenPower
// (confirmed via decompile) already does exactly the "heal at end of turn, then decrement" HoT
// behavior the design calls for, so no custom power is needed here.
[RegisterCard(typeof(GunbreakerCardPool))]
public sealed class Aurora() : ModCardTemplate(0, CardType.Skill, CardRarity.Common, TargetType.Self)
{
    public override CardAssetProfile AssetProfile => new()
    {
        PortraitPath = "res://GunbreakerMod/images/card_portraits/aurora.png",
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => [new PowerVar<RegenPower>(5m)];

    public override void AfterCreated()
    {
        base.AfterCreated();
        this.SecondaryCosts().Set(CartridgeResource.Id, 1);
    }

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<RegenPower>(
            choiceContext, Owner.Creature, DynamicVars["RegenPower"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["RegenPower"].UpgradeValueBy(2m);
    }
}
