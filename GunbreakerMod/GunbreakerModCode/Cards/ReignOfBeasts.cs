using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.CardTargeting;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Cards;

// 崛起之心 Reign of Beasts - big damage to the chosen target, splash to the rest. Puts Noble Blood
// on top of the draw pile.
[RegisterCard(typeof(GunbreakerCardPool))]
public sealed class ReignOfBeasts() : ModCardTemplate(2, CardType.Attack, CardRarity.Token, TargetType.AnyEnemy)
{
    public override CardAssetProfile AssetProfile => new()
    {
        PortraitPath = "res://GunbreakerMod/images/card_portraits/reign_of_beasts.png",
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust, CardKeyword.Ethereal];

    protected override IEnumerable<DynamicVar> CanonicalVars =>
        [new DamageVar("DamageMain", 25m, ValueProp.Move), new DamageVar("DamageSplash", 5m, ValueProp.Move)];

    // Matches vanilla's own convention (BladeDance -> Shiv, etc.): hovering this card shows a
    // preview of the token it generates.
    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromCard<NobleBlood>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        ArgumentNullException.ThrowIfNull(cardPlay.Target);
        await DamageCmd.Attack(DynamicVars["DamageMain"].BaseValue)
            .FromCard(this, cardPlay)
            .Targeting(cardPlay.Target)
            .Execute(choiceContext);

        // A manual per-enemy loop with repeated .Targeting(single).Execute() calls only ever hit
        // one splash target regardless of how many enemies were on the field (confirmed by the
        // user). TargetingFiltered runs the whole splash hit as a single AttackCommand against an
        // explicit target list instead, which is the pattern RitsuLib provides for exactly this
        // "primary target + everyone else" shape.
        // .ToList() materializes the query eagerly - CombatState.GetOpponentsOf returns a live
        // collection, and if splash damage kills one of the splash targets mid-resolution, a lazy
        // .Where() re-enumerating that same live collection throws "Collection was modified" and
        // aborts the rest of OnPlay (this was silently killing card generation below - confirmed
        // via godot.log stack trace pointing into this method's enumeration).
        var splashTargets = CombatState.GetOpponentsOf(Owner.Creature).Where(enemy => enemy != cardPlay.Target).ToList();
        await DamageCmd.Attack(DynamicVars["DamageSplash"].BaseValue)
            .FromCard(this, cardPlay)
            .TargetingFiltered(splashTargets)
            .Execute(choiceContext);

        var nobleBlood = CombatState.CreateCard<NobleBlood>(Owner);
        if (IsUpgraded)
        {
            CardCmd.Upgrade(nobleBlood);
        }
        // Generate straight into the top of the draw pile in one step, then pop the dedicated
        // floating card-preview popup (CardCmd.PreviewCardPileAdd) instead of routing through Hand
        // first. Decompiling vanilla's own generator cards (Turbo -> Void, Overclock -> Burn,
        // GunkUp -> Slimed, FightThrough -> Wound, BoostAway -> Dazed) shows this exact pattern -
        // AddGeneratedCardToCombat straight to the destination pile, wrapped in
        // CardCmd.PreviewCardPileAdd for the visual - none of them stage through Hand first.
        // (An earlier "generate into Hand, then move to Draw top" attempt was a guess at a
        // DecisionsDecisions precedent that turned out, on actually decompiling that card, to not
        // do anything of the sort - it doesn't move cards to the draw pile at all.)
        var result = await CardPileCmd.AddGeneratedCardToCombat(nobleBlood, PileType.Draw, Owner, CardPilePosition.Top);
        CardCmd.PreviewCardPileAdd(result);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DamageMain"].UpgradeValueBy(5m);
        DynamicVars["DamageSplash"].UpgradeValueBy(1m);
    }
}
