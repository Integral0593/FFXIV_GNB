using GunbreakerMod.GunbreakerModCode.Characters;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Cards;

// 利刃斩 Keen Edge - basic combo opener. Generates Brutal Shell into hand on play.
// Starter count: the design sheet doesn't give an explicit count for this one (unlike
// Strike_GNB "起始卡组3张" / Defend_GNB "4张"), just "起始牌库买一送二，牌库里只有1".
// Defaulting to 1 copy, mirroring how vanilla Ironclad's signature attack (Bash) gets exactly
// 1 starting copy alongside Strike/Defend - please correct the count if that's not what you meant.
[RegisterCard(typeof(GunbreakerCardPool))]
[RegisterCharacterStarterCard(typeof(Gunbreaker), 1)]
public sealed class KeenEdge() : ModCardTemplate(1, CardType.Attack, CardRarity.Basic, TargetType.AnyEnemy)
{
    public override CardAssetProfile AssetProfile => new()
    {
        PortraitPath = "res://GunbreakerMod/images/card_portraits/keen_edge.png",
    };

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DamageVar(6m, ValueProp.Move)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars.Damage.BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        var generated = CombatState.CreateCard<BrutalShell>(Owner);
        if (IsUpgraded)
        {
            CardCmd.Upgrade(generated);
        }
        await CardPileCmd.AddGeneratedCardToCombat(generated, PileType.Hand, Owner);
    }

    protected override void OnUpgrade()
    {
        DynamicVars.Damage.UpgradeValueBy(3m);
    }
}
