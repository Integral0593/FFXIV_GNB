using GunbreakerMod.GunbreakerModCode;
using GunbreakerMod.GunbreakerModCode.Characters;
using Godot;
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
public static partial class CartridgeResource
{
    private const string LocalId = "cartridge";
    private const int Slots = 3;
    private const float PipSize = 44f;
    private const float PipGap = 8f;
    private const float BorderThickness = 3f;
    private const float GapAboveEnergyOrb = 14f;

    private static readonly Color LitColor = new(0.25f, 0.85f, 1f);
    private static readonly Color UnlitColor = new(0.55f, 0.55f, 0.6f);

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
            parent => new PipRow(),
            update: ctx => ctx.Node.Owner = ctx.Player);

        return definition;
    }

    // Drives its own visibility/lit-state/position every frame instead of relying on RitsuLib's
    // external "update" callback timing. That callback IS still used (see Owner assignment above),
    // but only to learn who's playing - not to decide when to render. Root cause of the "pips don't
    // show until the first Cartridge gain" bug: RegisterCombatUi hides new attachments immediately
    // after registering them (SecondaryResourceUiRuntime.HideCombatUi), and the very first
    // NCombatUi.Activate()-triggered refresh can race against that same-frame registration and miss
    // it entirely - the row then sits hidden until the next resource-changed event happens to fire
    // an update. Owning our own per-frame refresh sidesteps that race completely.
    private sealed partial class PipRow : HBoxContainer
    {
        public Player? Owner;

        private readonly TextureRect[] _fills = new TextureRect[Slots];

        public override void _Ready()
        {
            MouseFilter = MouseFilterEnum.Ignore;
            AddThemeConstantOverride("separation", (int)PipGap);

            var texture = GD.Load<Texture2D>(Definition.SmallIconPath);
            for (var i = 0; i < Slots; i++)
            {
                var cell = new Control
                {
                    CustomMinimumSize = new Vector2(PipSize, PipSize),
                    MouseFilter = MouseFilterEnum.Ignore,
                };

                var border = new TextureRect
                {
                    Texture = texture,
                    StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered,
                    Modulate = Colors.White,
                    MouseFilter = MouseFilterEnum.Ignore,
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
                    MouseFilter = MouseFilterEnum.Ignore,
                    AnchorRight = 1f,
                    AnchorBottom = 1f,
                };
                cell.AddChild(fill);

                AddChild(cell);
                _fills[i] = fill;
            }
        }

        public override void _Process(double delta)
        {
            if (Owner == null)
            {
                Visible = false;
                return;
            }

            Visible = true;
            var amount = SecondaryResourceStateStore.GetAmount(Owner, Id);
            for (var i = 0; i < _fills.Length; i++)
            {
                _fills[i].Modulate = i < amount ? LitColor : UnlitColor;
            }

            if (GetParent() is not NCombatUi combatUi)
            {
                return;
            }

            var energyContainer = combatUi.GetNodeOrNull<Control>("%EnergyCounterContainer");
            if (energyContainer == null)
            {
                return;
            }

            // Center over the actual energy-orb visual (the container's first child, added at
            // runtime by NCombatUi.Activate), not the container itself - EnergyCounterContainer's
            // own Size doesn't tightly bound the orb (it's laid out to anchor a larger HUD region).
            var orb = energyContainer.GetChildOrNull<Control>(0) ?? energyContainer;
            const float rowWidth = Slots * PipSize + (Slots - 1) * PipGap;
            var targetX = orb.GlobalPosition.X + (orb.Size.X / 2f) - (rowWidth / 2f);
            var targetY = orb.GlobalPosition.Y - (PipSize + GapAboveEnergyOrb);
            GlobalPosition = new Vector2(targetX, targetY);
        }
    }
}
