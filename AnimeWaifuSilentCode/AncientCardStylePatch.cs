using System;

using Godot;
using HarmonyLib;

using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace AnimeWaifuSilent.AnimeWaifuSilentCode;

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
        TextureRect ancientTextBg = card.GetNode<TextureRect>("%AncientTextBg");
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
        card.GetNode<TextureRect>("%AncientHighlight").Visible = true;
        ancientTextBg.Visible = true;
        card.GetNode<Control>("%AncientBanner").Visible = true;
        card.GetNode<Control>("%TitleBanner").Visible = false;

        ApplyAncientBorderTextures(card);

        ApplyCanvasGroupMaskMaterial(portraitCanvasGroup);

        ancientPortrait.ExpandMode = (TextureRect.ExpandModeEnum)1;
        ancientPortrait.StretchMode = (TextureRect.StretchModeEnum)6;

        // 获取卡牌类型并应用正确的纹理
        object? model = card.Model;
        if (model != null)
        {
            var typeProperty = model.GetType().GetProperty("Type");
            if (typeProperty != null)
            {
                var typeValue = typeProperty.GetValue(model);
                if (typeValue != null)
                {
                    CardType cardType = (CardType)typeValue;
                    ApplyAncientTextBg(cardType, ancientTextBg, cardTypeName);
                }
            }
        }
    }

    private static void ApplyAncientTextBg(CardType cardType, TextureRect ancientTextBg, string cardTypeName)
    {
        if (ancientTextBg == null)
        {
            return;
        }

        string cardTypeStr = cardType switch
        {
            CardType.None => "skill",
            CardType.Status => "skill",
            CardType.Curse => "skill",
            CardType.Quest => "skill",
            CardType.Attack => "attack",
            CardType.Skill => "skill",
            CardType.Power => "power",
            _ => "skill"
        };

        string textBgPath = $"res://images/atlases/compressed.sprites/card_template/ancient_card_text_bg_{cardTypeStr}.tres";

        if (ResourceLoader.Exists(textBgPath))
        {
            Texture2D? textBgTexture = ResourceLoader.Load<Texture2D>(textBgPath, null, ResourceLoader.CacheMode.Reuse);
            if (textBgTexture != null)
            {
                ancientTextBg.Texture = textBgTexture;
                MainFile.Logger.Debug($"[AncientStyle] Applied {cardTypeStr} text bg for {cardTypeName}");
            }
            else
            {
                MainFile.Logger.Error($"[AncientStyle] Failed to load texture: {textBgPath}");
            }
        }
        else
        {
            MainFile.Logger.Error($"[AncientStyle] Texture not found: {textBgPath}");
        }
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

    private static void ApplyAncientBorderTextures(NCard card)
    {
        try
        {
            object? model = card.Model;
            if (model == null)
            {
                return;
            }

            var ancientBorderProp = model.GetType().GetProperty("AncientBorder");
            if (ancientBorderProp != null)
            {
                var texture = ancientBorderProp.GetValue(model) as Texture2D;
                if (texture != null)
                {
                    card.GetNode<TextureRect>("%AncientBorder").Texture = texture;
                }
            }

            var ancientHighlightProp = model.GetType().GetProperty("AncientHighlight");
            if (ancientHighlightProp != null)
            {
                var texture = ancientHighlightProp.GetValue(model) as Texture2D;
                if (texture != null)
                {
                    card.GetNode<TextureRect>("%AncientHighlight").Texture = texture;
                }
            }
        }
        catch (Exception e)
        {
            MainFile.Logger.Error($"[AncientStyle] Border texture error: {e.Message}");
        }
    }
}