using GunbreakerMod.GunbreakerModCode;
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
    private static SecondaryResourceDefinition? _definition;

    public static SecondaryResourceDefinition Definition => _definition ??= Register();

    public static string Id => Definition.Id;

    private static SecondaryResourceDefinition Register()
    {
        return RitsuLibFramework.GetSecondaryResourceRegistry(MainFile.ModId).Register(
            "cartridge",
            new SecondaryResourceDefinition(
                defaultAmount: 0,
                baseMaxAmount: 3,
                turnStartPolicy: SecondaryResourceTurnStartPolicy.None,
                persistencePolicy: SecondaryResourcePersistencePolicy.Combat,
                smallIconPath: "res://GunbreakerMod/images/cartridge_icon_small.png",
                largeIconPath: "res://GunbreakerMod/images/cartridge_icon_large.png"));
    }
}
