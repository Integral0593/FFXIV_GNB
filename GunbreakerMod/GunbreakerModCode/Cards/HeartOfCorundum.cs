using GunbreakerMod.GunbreakerModCode.Powers;
using GunbreakerMod.GunbreakerModCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Cards;

// 刚玉之心 Heart of Corundum - Heart of Stone's upgraded-tier sibling: same block + this-turn-only
// Strength debuff, plus a Cartridge refund.
[RegisterCard(typeof(GunbreakerCardPool))]
public sealed class HeartOfCorundum() : ModCardTemplate(2, CardType.Skill, CardRarity.Uncommon, TargetType.Self)
{
    public override CardAssetProfile AssetProfile => new()
    {
        PortraitPath = "res://GunbreakerMod/images/card_portraits/heart_of_corundum.png",
    };

    public override bool GainsBlock => true;

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new BlockVar(8m, ValueProp.Move), new DynamicVar("Strength", 2m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await CreatureCmd.GainBlock(Owner.Creature, DynamicVars.Block, cardPlay);
        await SecondaryResourceCmd.Gain(Owner, CartridgeResource.Id, 1, source: this);

        foreach (var enemy in CombatState.GetOpponentsOf(Owner.Creature))
        {
            await PowerCmd.Apply<HeartOfStoneStrengthDownPower>(
                choiceContext, enemy, DynamicVars["Strength"].BaseValue, Owner.Creature, this);
        }
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Block.UpgradeValueBy(2m);
        DynamicVars["Strength"].UpgradeValueBy(2m);
        EnergyCost.UpgradeBy(-1);
    }
}
