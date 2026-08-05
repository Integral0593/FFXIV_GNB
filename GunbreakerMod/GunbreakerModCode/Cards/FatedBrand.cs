using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Cards;

// 命运之印 Fated Brand - Fated Circle's Continuation-only bonus token. Pure AoE damage.
[RegisterCard(typeof(GunbreakerCardPool))]
public sealed class FatedBrand() : ModCardTemplate(0, CardType.Attack, CardRarity.Token, TargetType.AllEnemies)
{
    public override CardAssetProfile AssetProfile => new()
    {
        PortraitPath = "res://GunbreakerMod/images/card_portraits/fated_brand.png",
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(3m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .TargetingAllOpponents(CombatState)
            .Execute(choiceContext);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(1m);
    }
}
