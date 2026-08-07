using GunbreakerMod.GunbreakerModCode;
using GunbreakerMod.GunbreakerModCode.Characters;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Nodes.Combat;
using STS2RitsuLib;
using STS2RitsuLib.Combat.SecondaryResources;

namespace GunbreakerMod.GunbreakerModCode.Resources;

// 晶壤/Cartridge: independent secondary resource, cap 3, separate from energy.
// Overflow rule (confirmed via decompiling SecondaryResourceCmd): Gain() adds then clamps to
// [MinAmount, BaseMaxAmount] in SetCore, so gaining while already at the cap is a no-op for the
// excess automatically - no custom "don't exceed 3" logic is needed here.
// Doesn't reset between turns (persists until spent); resets between combats (Combat scope, not Run).
//
// UI: renders as 3 pip icons (FF14 GNB job-gauge style) instead of RitsuLib's built-in "icon+number"
// counter, since the design calls for lit/unlit slots. There's no built-in "segmented" secondary
// resource style in RitsuLib (confirmed via decompiling SecondaryResourceCounterStyle/NSecondaryResourceIcon -
// neither supports multiple discrete pips), so this is a small custom Control built by hand.
//
// IMPORTANT: this node MUST be a plain HBoxContainer/TextureRect/Timer tree, NOT a custom Node
// subclass with overridden _Ready()/_Process(). An earlier attempt at a custom `PipRow : HBoxContainer`
// with its own _Process() crashed NCombatUi's setup on every combat start (confirmed via log stack
// trace: MonoMod.Core.Interop.CoreCLR.V60.InvokeCompileMethod threw ArgumentException from inside
// PipRow.InvokeGodotClassMethod during NCombatUi._Ready's node-attachment pass, aborting
// NRun.SetCurrentRoom entirely - which is why the character visuals and HP bar disappeared too,
// not just the pips). Root cause not fully understood (looks like a JIT/hot-patch conflict specific
// to custom Godot node subclasses in this modded environment), so "self-refreshing" behavior here
// is built from a plain built-in Timer node's Timeout event instead - stock Godot type, no override.
public static class CartridgeResource
{
    private const string LocalId = "cartridge";
    private const int Slots = 3;
    private const float PipSize = 44f;
    private const float PipGap = 8f;
    private const float BorderThickness = 3f;
    private const float GapAboveEnergyOrb = 14f;
    private const float SelfRefreshIntervalSeconds = 0.2f;

    private static readonly Color LitColor = new(0.25f, 0.85f, 1f);
    private static readonly Color UnlitColor = new(0.3f, 0.3f, 0.33f);

    private static SecondaryResourceDefinition? _definition;

    public static SecondaryResourceDefinition Definition => _definition ??= Register();

    public static string Id => Definition.Id;

    // Shared by cards that want the "glow gold when affordable" highlight (see GrandFinale's
    // ShouldGlowGoldInternal pattern in the base game) - e.g. Burst Strike/Double Down.
    public static bool HasAtLeast(Player owner, int amount) => SecondaryResourceStateStore.GetAmount(owner, Id) >= amount;

    private static SecondaryResourceDefinition Register()
    {
        var resources = RitsuLibFramework.GetSecondaryResourceRegistry(MainFile.ModId);
        var definition = resources.Register(
            LocalId,
            new SecondaryResourceDefinition(
                defaultAmount: 0,
                baseMaxAmount: Slots,
                turnStartPolicy: SecondaryResourceTurnStartPolicy.None,
                persistencePolicy: SecondaryResourcePersistencePolicy.Combat,
                smallIconPath: "res://GunbreakerMod/images/cartridge_icon_small.png",
                largeIconPath: "res://GunbreakerMod/images/cartridge_icon_large.png"));

        resources.AlwaysShowInCombatUiForCharacter<Gunbreaker>(LocalId, 0);
        resources.RegisterCombatUi(
            LocalId,
            parent => CreatePipRow(),
            update: ctx => RefreshPipRow(ctx.Node, ctx.Player));

        return definition;
    }

    private static HBoxContainer CreatePipRow()
    {
        var row = new HBoxContainer { MouseFilter = Control.MouseFilterEnum.Ignore };
        row.AddThemeConstantOverride("separation", (int)PipGap);

        var texture = GD.Load<Texture2D>(Definition.SmallIconPath);
        for (var i = 0; i < Slots; i++)
        {
            var cell = new Control
            {
                CustomMinimumSize = new Vector2(PipSize, PipSize),
                MouseFilter = Control.MouseFilterEnum.Ignore,
            };

            var border = new TextureRect
            {
                Texture = texture,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                Modulate = Colors.White,
                MouseFilter = Control.MouseFilterEnum.Ignore,
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = -BorderThickness,
                OffsetTop = -BorderThickness,
                OffsetRight = BorderThickness,
                OffsetBottom = BorderThickness,
            };
            cell.AddChild(border);

            var fill = new TextureRect
            {
                Texture = texture,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                Modulate = UnlitColor,
                MouseFilter = Control.MouseFilterEnum.Ignore,
                AnchorRight = 1f,
                AnchorBottom = 1f,
            };
            cell.AddChild(fill);

            row.AddChild(cell);
        }

        // Self-healing safety net: RitsuLib's own "update" callback (wired above) is the normal
        // path, but it can miss the very first refresh at combat start - RitsuLib hides every
        // freshly-registered combat-UI attachment synchronously right after this factory returns
        // (SecondaryResourceUiRuntime.HideCombatUi), and NCombatUi.Activate()'s one-shot refresh can
        // race against that same-frame hide and lose (confirmed reproducible even with a
        // CallDeferred Show() attempt). Rather than guess further at RitsuLib's internal dispatch
        // timing, this Timer independently recomputes the correct state every 0.2s using
        // CombatManager/LocalContext directly - the same lookup RitsuLib's own Activate patch uses
        // to resolve "the local player" - so it converges on the right answer regardless of whether
        // the event-driven path fired correctly.
        var timer = new Godot.Timer
        {
            WaitTime = SelfRefreshIntervalSeconds,
            Autostart = true,
            OneShot = false,
        };
        timer.Timeout += () =>
        {
            if (!GodotObject.IsInstanceValid(row))
            {
                return;
            }
            var state = CombatManager.Instance.DebugOnlyGetState();
            RefreshPipRow(row, LocalContext.GetMe(state));
        };
        row.AddChild(timer);

        return row;
    }

    private static void RefreshPipRow(HBoxContainer row, Player? player)
    {
        var isVisible = player?.Character is Gunbreaker;
        row.Visible = isVisible;
        if (!isVisible)
        {
            return;
        }

        var amount = SecondaryResourceStateStore.GetAmount(player!, Id);
        for (var i = 0; i < row.GetChildCount(); i++)
        {
            if (row.GetChild(i) is not Control cell || cell.GetChildCount() < 2)
            {
                continue;
            }
            if (cell.GetChild(1) is TextureRect fill)
            {
                fill.Modulate = i < amount ? LitColor : UnlitColor;
            }
        }

        if (row.GetParent() is not NCombatUi combatUi)
        {
            return;
        }
        var energyContainer = combatUi.GetNodeOrNull<Control>("%EnergyCounterContainer");
        if (energyContainer == null)
        {
            return;
        }

        // Center over the actual energy-orb visual (the container's first child, added at runtime
        // by NCombatUi.Activate), not the container itself - EnergyCounterContainer's own Size
        // doesn't tightly bound the orb (it's laid out to anchor a larger HUD region).
        var orb = energyContainer.GetChildOrNull<Control>(0) ?? energyContainer;
        const float rowWidth = Slots * PipSize + (Slots - 1) * PipGap;
        var targetX = orb.GlobalPosition.X + (orb.Size.X / 2f) - (rowWidth / 2f);
        // NEnergyCounter.AnimIn() tweens the counter ROOT's Position back to Vector2.Zero every
        // combat (confirmed via decompile), so scenes/energy_counter.tscn can't shift the visible
        // art by moving the root - it pushes the art down via an offset baked into each CHILD
        // node instead (currently 30px - keep this in sync with that .tscn's per-child
        // offset_top). orb.GlobalPosition still reports the unshifted root, so the pip row needs
        // that same offset added back in to stay glued above the actually-visible artwork.
        const float energyCounterContentOffsetY = 30f;
        var targetY = orb.GlobalPosition.Y + energyCounterContentOffsetY - (PipSize + GapAboveEnergyOrb);
        row.GlobalPosition = new Vector2(targetX, targetY);
    }
}
