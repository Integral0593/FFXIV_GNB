using System.Reflection;
using Godot;
using GunbreakerMod.GunbreakerModCode.Resources;
using MegaCrit.Sts2.Core.Modding;
using STS2RitsuLib;
using STS2RitsuLib.Interop;

namespace GunbreakerMod.GunbreakerModCode;

//You're recommended but not required to keep all your code in this package and all your assets in the GunbreakerMod folder.
[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "GunbreakerMod"; //At the moment, this is used only for the Logger name.

    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; private set; } = null!;

    public static void Initialize()
    {
        var assembly = Assembly.GetExecutingAssembly();

        Logger = RitsuLibFramework.CreateLogger(ModId);
        ModTypeDiscoveryHub.RegisterModAssembly(ModId, assembly);

        _ = CartridgeResource.Definition; // force secondary-resource registration early
    }
}
