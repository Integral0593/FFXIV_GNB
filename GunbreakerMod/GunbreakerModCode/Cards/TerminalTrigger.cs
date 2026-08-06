using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using GunbreakerMod.GunbreakerModCode.Resources;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Cards;

// 终结击 Terminal Trigger (renamed from Finisher per the design table) - opens the Reign of Beasts /
// Noble Blood / Lion Heart chain. Gains Cartridge and puts Reign of Beasts on top of the draw pile -
// confirmed with the user this chain is strictly sequential (play one, the next lands on top of the
// draw pile), not a 3-choice branch.
[RegisterCard(typeof(GunbreakerCardPool))]
public sealed class TerminalTrigger() : ModCardTemplate(2, CardType.Skill, CardRarity.Rare, TargetType.Self)
{
    public override CardAssetProfile AssetProfile => new()
    {
        PortraitPath = "res://GunbreakerMod/images/card_portraits/terminal_trigger.png",
    };

    public override IEnumerable<CardKeyword> CanonicalKeywords => [CardKeyword.Exhaust];

    protected override IEnumerable<DynamicVar> CanonicalVars => [new DynamicVar("CartridgeGain", 2m)];

    protected override IEnumerable<IHoverTip> AdditionalHoverTips => [HoverTipFactory.FromCard<ReignOfBeasts>()];

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await SecondaryResourceCmd.Gain(Owner, CartridgeResource.Id, (int)DynamicVars["CartridgeGain"].BaseValue, source: this);

        var reignOfBeasts = CombatState.CreateCard<ReignOfBeasts>(Owner);
        if (IsUpgraded)
        {
            CardCmd.Upgrade(reignOfBeasts);
        }
        // See ReignOfBeasts.cs for why this generates straight to Draw-top + PreviewCardPileAdd
        // instead of routing through Hand first.
        var result = await CardPileCmd.AddGeneratedCardToCombat(reignOfBeasts, PileType.Draw, Owner, CardPilePosition.Top);
        CardCmd.PreviewCardPileAdd(result);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
    }
}
