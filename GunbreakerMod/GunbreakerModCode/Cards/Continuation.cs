using System.Linq;
using GunbreakerMod.GunbreakerModCode.Powers;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Runs;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Cards;

// 续剑 Continuation - grants ContinuationPower. Per the design table, the base version has no
// Innate; upgrading both adds Innate and reduces cost to 0.
//
// Effectively unique: holding a second copy would be a real bug (which Cartridge-spending card's
// generation logic "wins" isn't meaningful with two Continuations active), so this excludes itself
// from every reward/merchant pool the moment the player already owns one. Both hooks are dispatched
// to every AbstractModel in the player's deck (confirmed via decompiling RunState.IterateHookListeners -
// player.Deck.Cards participates directly), so the canonical Continuation instance sitting in the
// deck sees its own presence and self-excludes - no external "is this card unique" flag exists in the
// base game to lean on instead.
[RegisterCard(typeof(GunbreakerCardPool))]
public sealed class Continuation() : ModCardTemplate(1, CardType.Power, CardRarity.Uncommon, TargetType.Self)
{
    public override CardAssetProfile AssetProfile => new()
    {
        PortraitPath = "res://GunbreakerMod/images/card_portraits/continuation.png",
    };

    protected override async Task OnPlay(PlayerChoiceContext choiceContext, CardPlay cardPlay)
    {
        await PowerCmd.Apply<ContinuationPower>(choiceContext, Owner.Creature, 1m, Owner.Creature, this);
    }

    protected override void OnUpgrade()
    {
        EnergyCost.UpgradeBy(-1);
        AddKeyword(CardKeyword.Innate);
    }

    public override CardCreationOptions ModifyCardRewardCreationOptions(Player player, CardCreationOptions options)
    {
        options = base.ModifyCardRewardCreationOptions(player, options);
        var existingFilter = options.CardPoolFilter;
        return options.WithFilter(c => c is not Continuation && (existingFilter == null || existingFilter(c)));
    }

    public override IEnumerable<CardModel> ModifyMerchantCardPool(Player player, IEnumerable<CardModel> options)
    {
        return base.ModifyMerchantCardPool(player, options).Where(c => c is not Continuation);
    }
}
