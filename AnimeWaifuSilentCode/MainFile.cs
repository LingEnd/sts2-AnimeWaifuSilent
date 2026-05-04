using System.Reflection;

using Godot;
using HarmonyLib;

using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

using LogType = MegaCrit.Sts2.Core.Logging.LogType;
using Logger = MegaCrit.Sts2.Core.Logging.Logger;

namespace AnimeWaifuSilent.AnimeWaifuSilentCode;

[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    public const string ModId = "AnimeWaifuSilent";

    public static Logger Logger { get; } = new(ModId, LogType.Generic);

    public static void Initialize()
    {
        Harmony harmony = new(ModId);
        harmony.PatchAll(Assembly.GetExecutingAssembly());

        Logger.Info("AnimeWaifuSilent has been initialized!");

        if (BaseLibIntegration.IsBaseLibLoaded)
        {
            BaseLibIntegration.RegisterBaseLibConfig();
        }
    }
}