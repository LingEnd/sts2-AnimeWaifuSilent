using Godot;

namespace AnimeWaifuSilent.AnimeWaifuSilentCode;

public static class ModConfig
{
    public const string ModId = "AnimeWaifuSilent";

    public static CanvasItem.TextureFilterEnum GetGodotFilterMode()
    {
        return CanvasItem.TextureFilterEnum.LinearWithMipmapsAnisotropic;
    }
}
