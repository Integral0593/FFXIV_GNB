using GunbreakerMod.GunbreakerModCode.Cards;
using GunbreakerMod.GunbreakerModCode.Characters;
using GunbreakerMod.GunbreakerModCode.Resources;
using GunbreakerMod.GunbreakerModCode.Vfx;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization.DynamicVars;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.ValueProps;
using STS2RitsuLib.Combat.SecondaryResources;
using STS2RitsuLib.Interop.AutoRegistration;
using STS2RitsuLib.Scaffolding.Content;

namespace GunbreakerMod.GunbreakerModCode.Relics;

// 绝枪战士之证 - starter relic. Gains 1 Cartridge at the start of each combat (BeforeCombatStart,
// same hook vanilla's own Anchor/Bag of Marbles use for their "once per combat, not every turn"
// effects - confirmed via decompile). Also satisfies the game's requirement that every character
// have at least one starting relic (NCharacterSelectScreen reads StartingRelics[0]
// unconditionally when populating the character-select info panel), and hosts the "knocked back
// when hit" reaction: since it's always in the player's possession for the whole run, it's a
// convenient place to hang that - relics are AbstractModel just like Powers, so
// AfterDamageReceived fires the same way, and unlike a Power there's no need to re-apply it at
// the start of every combat.
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

    // SecondaryResourceVar (not a plain DynamicVar) is what lets the description reference
    // {Cartridge:secondaryResourceIcons} - RitsuLib's SecondaryResourceIconsFormatter renders it
    // as the actual Cartridge pip icon(s) in the resource's own highlight color, matching how
    // vanilla relics like Booming Conch use EnergyVar to render the Energy icon inline instead of
    // a plain "1".
    protected override IEnumerable<DynamicVar> CanonicalVars => [new SecondaryResourceVar("Cartridge", CartridgeResource.Id, 1m)];

    // Mirrors ModRelicTemplate.IncludeEnergyHoverTip's own HoverTipFactory.ForEnergy(this) pattern,
    // but for our own secondary resource - SecondaryResourceHoverTipFactory.Create builds the same
    // kind of title+icon+description hover tip box vanilla shows for Energy (see
    // static_hover_tips.json for the Cartridge resource's own title/description text it reads).
    protected override IEnumerable<IHoverTip> AdditionalHoverTips =>
        [SecondaryResourceHoverTipFactory.Create(CartridgeResource.Definition, 1, 3)];

    public override async Task BeforeCombatStart()
    {
        Flash();
        await SecondaryResourceCmd.Gain(Owner, CartridgeResource.Id, 1, source: this);
    }

    public override Task AfterDamageReceived(PlayerChoiceContext choiceContext, Creature target, DamageResult result, ValueProp props, Creature? dealer, CardModel? cardSource)
    {
        if (target == Owner.Creature && result.UnblockedDamage > 0)
        {
            CreatureBumpAnimator.PlayBump(target, BackwardOffsetX, BackwardDuration, ReturnDuration);
        }

        return Task.CompletedTask;
    }
}
