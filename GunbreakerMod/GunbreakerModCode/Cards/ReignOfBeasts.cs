using System.Linq;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
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
        // Generate into Hand first (shows the normal "fly into hand" reveal, same as the other
        // combo chains) then move it to the top of the draw pile - a card materializing directly
        // into Draw with no prior pile gets no visual treatment at all (confirmed by decompiling
        // CardPileCmd's tween-selection logic: it only animates pile changes that either start or
        // end in Hand/Play, or move between two already-invisible piles like Draw/Discard/Exhaust -
        // a brand new card with no old pile matches neither case). Per user request: show the
        // generated card before it lands on the deck, mirroring how Regent's DecisionsDecisions
        // moves cards from hand to the top of the draw pile.
        await CardPileCmd.AddGeneratedCardToCombat(nobleBlood, PileType.Hand, Owner);
        await Cmd.Wait(0.75f);
        await CardPileCmd.Add(nobleBlood, PileType.Draw, CardPilePosition.Top);
    }

    protected override void OnUpgrade()
    {
        DynamicVars["DamageMain"].UpgradeValueBy(5m);
        DynamicVars["DamageSplash"].UpgradeValueBy(1m);
    }
}
