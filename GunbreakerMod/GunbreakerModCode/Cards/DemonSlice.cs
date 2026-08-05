using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Cards;

// 恶魔切 Demon Slice - AoE combo opener. Generates Demon Slaughter into hand on play.
// Common rarity: not a starter card, appears in card rewards/shops.
[RegisterCard(typeof(GunbreakerCardPool))]
public sealed class DemonSlice() : ModCardTemplate(1, CardType.Attack, CardRarity.Common, TargetType.AllEnemies)
{
    public override CardAssetProfile AssetProfile => new()
    {
        PortraitPath = "res://GunbreakerMod/images/card_portraits/demon_slice.png",
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(3m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState)
            .Execute(choiceContext);

        var generated = CombatState.CreateCard<DemonSlaughter>(Owner);
        if (IsUpgraded)
        {
            CardCmd.Upgrade(generated);
        }
        await CardPileCmd.AddGeneratedCardToCombat(generated, PileType.Hand, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(2m);
    }
}
