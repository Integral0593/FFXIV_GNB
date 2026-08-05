using GunbreakerMod.GunbreakerModCode;
using GunbreakerMod.GunbreakerModCode.Characters;
using Godot;
using STS2RitsuLib;
using STS2RitsuLib.Combat.SecondaryResources;

namespace GunbreakerMod.GunbreakerModCode.Resources;

// 晶壤/Cartridge: independent secondary resource, cap 3, separate from energy.
// Overflow rule (confirmed via decompiling SecondaryResourceCmd): Gain() adds then clamps to
// [MinAmount, BaseMaxAmount] in SetCore, so gaining while already at the cap is a no-op for the
// excess automatically - no custom "don't exceed 3" logic is needed here.
// Doesn't reset between turns (persists until spent); resets between combats (Combat scope, not Run).
public static class CartridgeResource
{
    private const string LocalId = "cartridge";

    private static SecondaryResourceDefinition? _definition;

    public static SecondaryResourceDefinition Definition => _definition ??= Register();

    public static string Id => Definition.Id;

    private static SecondaryResourceDefinition Register()
    {
        var resources = RitsuLibFramework.GetSecondaryResourceRegistry(MainFile.ModId);
        var definition = resources.Register(
            LocalId,
            new SecondaryResourceDefinition(
                defaultAmount: 0,
                baseMaxAmount: 3,
                turnStartPolicy: SecondaryResourceTurnStartPolicy.None,
                persistencePolicy: SecondaryResourcePersistencePolicy.Combat,
                smallIconPath: "res://GunbreakerMod/images/cartridge_icon_small.png",
                largeIconPath: "res://GunbreakerMod/images/cartridge_icon_large.png"));

        // Show the counter even before the first Cartridge is gained, and use RitsuLib's
        // built-in icon+number widget rather than a hand-rolled node tree (lower risk than
        // building a custom "3 pips" Control from scratch for a first pass).
        // NodeAttachmentOptions has no position field - the attachment itself succeeds (confirmed
        // via the "[NodeAttachment] Registered ..." log line) but with no Position set it likely
        // lands at NCombatUi's local (0,0), which may be off-screen or hidden behind other HUD
        // elements. Setting an explicit position here as a first guess; may need adjusting once
        // it's actually visible in-game.
        resources.AlwaysShowInCombatUiForCharacter<Gunbreaker>(LocalId, 0);
        resources.RegisterCombatUi(
            LocalId,
            parent =>
            {
                var node = NSecondaryResourceCounter.Create(definition);
                node.Position = new Vector2(30, 150);
                node.Visible = true;
                return node;
            },
            update: ctx => ctx.Node.Bind(ctx.Player));

        return definition;
    }
}
