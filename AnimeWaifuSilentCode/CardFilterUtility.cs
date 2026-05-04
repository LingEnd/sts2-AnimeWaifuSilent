using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace AnimeWaifuSilent.AnimeWaifuSilentCode;

[HarmonyPatch(typeof(NTinyCard), nameof(NTinyCard._Ready))]
public static class TinyCardTextureFilterPatch
{
    static void Postfix(NTinyCard __instance)
    {
        CardFilterUtility.TryApplyTinyCardPortraitFilters(__instance);
    }
}

[HarmonyPatch(typeof(NCard), nameof(NCard._Ready))]
public static class CardTextureFilterPatch
{
    static void Postfix(NCard __instance)
    {
        CardFilterUtility.TryApplyNCardPortraitFilters(__instance);
    }
}

[HarmonyPatch(typeof(NCard), "Reload")]
public static class CardTextureFilterReloadPatch
{
    static void Postfix(NCard __instance)
    {
        CardFilterUtility.TryApplyNCardPortraitFilters(__instance);
    }
}

public static class CardFilterUtility
{
    private static readonly string[] NCardPortraitNodePaths =
    {
        "%Portrait",
        "%AncientPortrait",
    };

    private static readonly string[] NCardCanvasGroupPaths =
    {
        "%PortraitCanvasGroup",
    };

    public static bool TryApplyNCardPortraitFilters(NCard card)
    {
        if (!CardPortraitReplacementPatch.IsModPortraitModel(card.Model))
        {
            return false;
        }

        ApplyNCardPortraitFilters(card);
        return true;
    }

    public static void ApplyNCardPortraitFilters(NCard card)
    {
        foreach (string nodePath in NCardPortraitNodePaths)
        {
            ApplyFilterToNode(card.GetNodeOrNull<CanvasItem>(nodePath));
        }

        foreach (string nodePath in NCardCanvasGroupPaths)
        {
            CanvasItem? canvasItem = card.GetNodeOrNull<CanvasItem>(nodePath);
            if (canvasItem != null)
            {
                ApplySmoothFilteringRecursively(canvasItem);
            }
        }
    }

    public static bool TryApplyTinyCardPortraitFilters(NTinyCard tinyCard)
    {
        TextureRect? portrait = tinyCard.GetNodeOrNull<TextureRect>("%Portrait");
        if (portrait == null)
        {
            return false;
        }

        if (!CardPortraitReplacementPatch.IsModPortraitTexture(portrait.Texture))
        {
            return false;
        }

        ApplyFilterToNode(portrait);
        ApplyFilterToNode(tinyCard.GetNodeOrNull<CanvasItem>("%PortraitShadow"));
        return true;
    }

    private static void ApplySmoothFilteringRecursively(Node node)
    {
        ApplyFilterToNode(node as CanvasItem);

        foreach (Node child in node.GetChildren())
        {
            ApplySmoothFilteringRecursively(child);
        }
    }

    private static void ApplyFilterToNode(CanvasItem? canvasItem)
    {
        if (canvasItem == null)
        {
            return;
        }

        canvasItem.TextureFilter = (CanvasItem.TextureFilterEnum)5;
    }
}
