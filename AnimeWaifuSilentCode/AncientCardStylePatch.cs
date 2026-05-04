using System;

using Godot;
using HarmonyLib;

using MegaCrit.Sts2.Core.Nodes.Cards;

namespace AnimeWaifuSilent.AnimeWaifuSilentCode;

/// <summary>
/// 拦截 <see cref="NCard"/> 的Reload方法，为配置为先古样式的卡牌应用先古卡牌视觉效果。
/// </summary>
[HarmonyPatch(typeof(NCard), "Reload")]
public static class AncientCardStylePatch
{
    static void Postfix(NCard __instance)
    {
        if (__instance?.Model == null)
        {
            return;
        }

        string cardTypeName = __instance.Model.GetType().Name;

        if (!CardReplacementConfig.IsAncientCard(cardTypeName))
        {
            return;
        }

        if (!ConfigHelper.IsAncientStyleEnabled(cardTypeName))
        {
            return;
        }

        try
        {
            ApplyAncientStyle(__instance, cardTypeName);
        }
        catch (Exception e)
        {
            MainFile.Logger.Error($"[AncientStyle] Error: {e.Message}");
        }
    }

    private static void ApplyAncientStyle(NCard card, string cardTypeName)
    {
        TextureRect portrait = card.GetNode<TextureRect>("%Portrait");
        TextureRect ancientPortrait = card.GetNode<TextureRect>("%AncientPortrait");
        CanvasGroup portraitCanvasGroup = card.GetNode<CanvasGroup>("%PortraitCanvasGroup");

        AncientReplacementEntry? ancientConfig = CardReplacementConfig.GetAncientConfig(cardTypeName);
        if (ancientConfig == null)
        {
            return;
        }

        if (card.Model?.Portrait != null)
        {
            ancientPortrait.Texture = card.Model.Portrait;
        }

        portrait.Visible = false;
        card.GetNode<TextureRect>("%PortraitBorder").Visible = false;
        card.GetNode<Control>("%Frame").Visible = false;

        ancientPortrait.Visible = true;
        card.GetNode<TextureRect>("%AncientBorder").Visible = true;
        card.GetNode<TextureRect>("%AncientTextBg").Visible = true;
        card.GetNode<Control>("%AncientBanner").Visible = true;
        card.GetNode<Control>("%TitleBanner").Visible = false;

        ApplyCanvasGroupMaskMaterial(portraitCanvasGroup);

        ancientPortrait.ExpandMode = (TextureRect.ExpandModeEnum)1;
        ancientPortrait.StretchMode = (TextureRect.StretchModeEnum)5;
    }

    private static void ApplyCanvasGroupMaskMaterial(CanvasGroup canvasGroup)
    {
        if (canvasGroup == null)
        {
            return;
        }

        try
        {
            const string materialPath = "res://scenes/cards/card_canvas_group_mask_material.tres";
            Material? maskMaterial = GD.Load<Material>(materialPath);

            if (maskMaterial != null)
            {
                canvasGroup.Material = maskMaterial;
            }
        }
        catch (Exception e)
        {
            MainFile.Logger.Error($"[AncientStyle] Material error: {e.Message}");
        }
    }
}
