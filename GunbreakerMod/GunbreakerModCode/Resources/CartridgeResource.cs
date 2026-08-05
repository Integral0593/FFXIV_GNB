using System.Linq;
using GunbreakerMod.GunbreakerModCode;
using GunbreakerMod.GunbreakerModCode.Characters;
using Godot;
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
// Positioning: NCombatUi exposes its energy-orb container via the scene-unique name
// "%EnergyCounterContainer" (confirmed via decompiling NCombatUi._Ready/Activate). Each update tick,
// this repositions the pip row's GlobalPosition directly above that container instead of hardcoding a
// pixel offset - correct regardless of resolution or the star-counter-related repositioning NCombatUi
// itself sometimes applies to the container.
public static class CartridgeResource
{
    private const string LocalId = "cartridge";
    private const int Slots = 3;
    private const float PipSize = 44f;
    private const float PipGap = 8f;
    private const float GapAboveEnergyOrb = 14f;

    private static readonly Color LitColor = new(0.25f, 0.85f, 1f);
    private static readonly Color UnlitColor = new(0.28f, 0.28f, 0.32f);

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
                baseMaxAmount: Slots,
                turnStartPolicy: SecondaryResourceTurnStartPolicy.None,
                persistencePolicy: SecondaryResourcePersistencePolicy.Combat,
                smallIconPath: "res://GunbreakerMod/images/cartridge_icon_small.png",
                largeIconPath: "res://GunbreakerMod/images/cartridge_icon_large.png"));

        resources.AlwaysShowInCombatUiForCharacter<Gunbreaker>(LocalId, 0);
        resources.RegisterCombatUi(
            LocalId,
            parent => CreatePipRow(definition),
            update: UpdatePipRow);

        return definition;
    }

    private static HBoxContainer CreatePipRow(SecondaryResourceDefinition definition)
    {
        var row = new HBoxContainer { MouseFilter = Control.MouseFilterEnum.Ignore };
        row.AddThemeConstantOverride("separation", (int)PipGap);

        var texture = GD.Load<Texture2D>(definition.SmallIconPath);
        for (var i = 0; i < Slots; i++)
        {
            row.AddChild(new TextureRect
            {
                Texture = texture,
                CustomMinimumSize = new Vector2(PipSize, PipSize),
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
                StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                Modulate = UnlitColor,
                MouseFilter = Control.MouseFilterEnum.Ignore,
            });
        }

        return row;
    }

    private static void UpdatePipRow(SecondaryResourceCombatUiContext<NCombatUi, HBoxContainer> ctx)
    {
        var row = ctx.Node;
        var isVisible = ctx.Player != null && ctx.VisibleDefinitions.Any(d => d.Id == Id);
        row.Visible = isVisible;
        if (!isVisible)
        {
            return;
        }

        var amount = SecondaryResourceStateStore.GetAmount(ctx.Player!, Id);
        for (var i = 0; i < row.GetChildCount(); i++)
        {
            if (row.GetChild(i) is TextureRect pip)
            {
                pip.Modulate = i < amount ? LitColor : UnlitColor;
            }
        }

        var energyContainer = ctx.Parent.GetNodeOrNull<Control>("%EnergyCounterContainer");
        if (energyContainer != null)
        {
            const float rowWidth = Slots * PipSize + (Slots - 1) * PipGap;
            var targetX = energyContainer.GlobalPosition.X + (energyContainer.Size.X / 2f) - (rowWidth / 2f);
            var targetY = energyContainer.GlobalPosition.Y - (PipSize + GapAboveEnergyOrb);
            row.GlobalPosition = new Vector2(targetX, targetY);
        }
    }
}
