using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models.Powers;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Cards;

// 光之心 Heart of Light - AoE Weak now, Block next turn. Uses the base game's own BlockNextTurnPower
// directly (grants block automatically once the owner's current block clears, i.e. at the start of
// their next turn).
[RegisterCard(typeof(GunbreakerCardPool))]
public sealed class HeartOfLight() : ModCardTemplate(0, CardType.Skill, CardRarity.Uncommon, TargetType.AllEnemies)
{
    public override CardAssetProfile AssetProfile => new()
    {
        PortraitPath = "res://GunbreakerMod/images/card_portraits/heart_of_light.png",
    };

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new PowerVar<WeakPower>(2m), new DynamicVar("BlockNextTurn", 8m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        foreach (var enemy in CombatState.GetOpponentsOf(Owner.Creature))
        {
            await PowerCmd.Apply<WeakPower>(choiceContext, enemy, DynamicVars.Weak.BaseValue, Owner.Creature, this);
        }

        await PowerCmd.Apply<BlockNextTurnPower>(
            choiceContext, Owner.Creature, DynamicVars["BlockNextTurn"].BaseValue, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Weak.UpgradeValueBy(1m);
        DynamicVars["BlockNextTurn"].UpgradeValueBy(2m);
    }
}
