using System;
using System.Collections.Generic;

using Godot;
using HarmonyLib;

using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace AnimeWaifuSilent.AnimeWaifuSilentCode;

[HarmonyPatch(typeof(CardModel), nameof(CardModel.Portrait), MethodType.Getter)]
public static class CardPortraitReplacementPatch
{
    private const string ModPortraitRoot = "res://AnimeWaifuSilent/card_portraits/";

    private static readonly Dictionary<string, Texture2D> ReplacementTextureCache = new(StringComparer.OrdinalIgnoreCase);
    private static readonly HashSet<ulong> ReplacementTextureIds = new();

    /// <summary>
    /// Replaces the card portrait with a custom mod image based on player configuration.
    /// </summary>
    static void Postfix(CardModel __instance, ref Texture2D __result)
    {
        string cardTypeName = __instance.GetType().Name;
        bool hasReplacement = false;

        if (CardReplacementConfig.IsAncientCard(cardTypeName))
        {
            if (ConfigHelper.IsAncientStyleEnabled(cardTypeName))
            {
                if (CardReplacementConfig.TryGetAncientPortrait(cardTypeName, out string? ancientPath))
                {
                    Texture2D? portrait = LoadReplacementTexture(ancientPath);
                    if (portrait != null)
                    {
                        __result = portrait;
                        hasReplacement = true;
                    }
                }
            }
            else
            {
                if (CardReplacementConfig.TryGetFallbackPortrait(cardTypeName, out string? fallbackPath))
                {
                    Texture2D? portrait = LoadReplacementTexture(fallbackPath);
                    if (portrait != null)
                    {
                        __result = portrait;
                        hasReplacement = true;
                    }
                }
            }
        }

        if (!hasReplacement && CardReplacementConfig.TryGetNormalPortrait(cardTypeName, out string? normalPath))
        {
            Texture2D? portrait = LoadReplacementTexture(normalPath);
            if (portrait != null)
            {
                __result = portrait;
            }
        }
    }

    public static bool IsModPortraitTexture(Texture2D? texture)
    {
        if (texture == null)
        {
            return false;
        }

        if (ReplacementTextureIds.Contains(texture.GetInstanceId()))
        {
            return true;
        }

        string resourcePath = texture.ResourcePath ?? string.Empty;
        return resourcePath.StartsWith(ModPortraitRoot, StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsModPortraitModel(CardModel? model)
    {
        return IsModPortraitTexture(model?.Portrait);
    }

    private static Texture2D? LoadReplacementTexture(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        if (ReplacementTextureCache.TryGetValue(path, out Texture2D? cached))
        {
            return cached;
        }

        if (!ResourceLoader.Exists(path))
        {
            return null;
        }

        Texture2D? sourceTexture = ResourceLoader.Load<Texture2D>(path, null, ResourceLoader.CacheMode.Reuse);
        if (sourceTexture == null)
        {
            return null;
        }

        AtlasTexture pseudoAtlas = new AtlasTexture
        {
            Atlas = sourceTexture,
            Region = new Rect2(0, 0, sourceTexture.GetWidth(), sourceTexture.GetHeight())
        };

        ReplacementTextureCache[path] = pseudoAtlas;
        ReplacementTextureIds.Add(pseudoAtlas.GetInstanceId());

        return pseudoAtlas;
    }
}

[HarmonyPatch(typeof(CardModel), "PortraitPath", MethodType.Getter)]
public static class CardPortraitPathReplacementPatch
{
    static void Postfix(CardModel __instance, ref string __result)
    {
        string cardTypeName = __instance.GetType().Name;

        if (CardReplacementConfig.IsAncientCard(cardTypeName))
        {
            if (ConfigHelper.IsAncientStyleEnabled(cardTypeName))
            {
                if (CardReplacementConfig.TryGetAncientPortrait(cardTypeName, out string? path) && path != null)
                {
                    __result = path;
                    return;
                }
            }
            else
            {
                if (CardReplacementConfig.TryGetFallbackPortrait(cardTypeName, out string? path) && path != null)
                {
                    __result = path;
                    return;
                }
            }
        }
        else if (CardReplacementConfig.TryGetNormalPortrait(cardTypeName, out string? normalPath) && normalPath != null)
        {
            __result = normalPath;
        }
    }
}
