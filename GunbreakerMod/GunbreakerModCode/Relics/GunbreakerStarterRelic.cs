using GunbreakerMod.GunbreakerModCode.Cards;
using GunbreakerMod.GunbreakerModCode.Characters;
using GunbreakerMod.GunbreakerModCode.Vfx;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Relics;

// Placeholder starter relic: no real gameplay effect, just satisfies the game's requirement
// that every character have at least one starting relic (NCharacterSelectScreen reads
// StartingRelics[0] unconditionally when populating the character-select info panel). It DOES
// carry one piece of presentation logic though: since it's always in the player's possession
// for the whole run, it's a convenient place to hang the "knocked back when hit" reaction -
// relics are AbstractModel just like Powers, so AfterDamageReceived fires the same way, and
// unlike a Power there's no need to re-apply it at the start of every combat.
[RegisterRelic(typeof(GunbreakerRelicPool))]
[RegisterCharacterStarterRelic(typeof(Gunbreaker), 1)]
public sealed class GunbreakerStarterRelic : ModRelicTemplate
{
    private const float BackwardOffsetX = -30f;
    private const float BackwardDuration = 0.08f;
    private const float ReturnDuration = 0.22f;

    public override RelicRarity Rarity => RelicRarity.Starter;

    public override RelicAssetProfile AssetProfile => new()
    {
        IconPath = "res://GunbreakerMod/images/relic_starter_icon.png",
        IconOutlinePath = "res://GunbreakerMod/images/relic_starter_icon_outline.png",
        BigIconPath = "res://GunbreakerMod/images/relic_starter_icon_big.png",
    };

    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner.Creature && result.UnblockedDamage > 0)
        {
            CreatureBumpAnimator.PlayBump(target, BackwardOffsetX, BackwardDuration, ReturnDuration);
        }

        return Task.CompletedTask;
    }
}
