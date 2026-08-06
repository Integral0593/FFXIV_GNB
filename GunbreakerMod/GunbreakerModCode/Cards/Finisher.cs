using GunbreakerMod.GunbreakerModCode.Resources;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Cards;

// 终结技 Finisher - opens the Reign of Beasts / Noble Blood / Lion Heart chain. Gains Cartridge and
// puts Reign of Beasts on top of the draw pile - confirmed with the user this chain is strictly
// sequential (play one, the next lands on top of the draw pile), not a 3-choice branch.
[RegisterCard(typeof(GunbreakerCardPool))]
public sealed class Finisher() : ModCardTemplate(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override CardAssetProfile AssetProfile => new()
    {
        PortraitPath = "res://GunbreakerMod/images/card_portraits/finisher.png",
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("CartridgeGain", 2m)];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SecondaryResourceCmd.Gain(Owner, CartridgeResource.Id, (int)DynamicVars["CartridgeGain"].BaseValue, source: this);

        var reignOfBeasts = CombatState.CreateCard<ReignOfBeasts>(Owner);
        if (IsUpgraded)
        {
            CardCmd.Upgrade(reignOfBeasts);
        }
        // Generate into Hand first so the player actually sees the card (see ReignOfBeasts.cs for
        // why a card materializing directly into Draw gets no visual treatment at all), then move
        // it to the top of the draw pile.
        await CardPileCmd.AddGeneratedCardToCombat(reignOfBeasts, PileType.Hand, Owner);
        await CardPileCmd.Add(reignOfBeasts, PileType.Draw, CardPilePosition.Top);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
