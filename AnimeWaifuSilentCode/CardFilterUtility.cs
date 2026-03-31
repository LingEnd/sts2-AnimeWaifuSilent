using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Nodes.Cards;

namespace AnimeWaifuSilent.AnimeWaifuSilentCode;

/// <summary>
/// 负责处理替换卡图在不同卡牌节点上的平滑过滤。
/// 之所以把这部分逻辑单独拆到本文件，是因为仅替换立绘资源还不够：
/// 当卡牌在手牌、小卡、放大预览等场景中频繁缩放时，默认过滤模式容易让边缘出现明显锯齿。
/// 因此这里会在相关节点初始化或重载时，把贴图过滤方式统一切换为更适合预览的平滑模式。
/// </summary>
[HarmonyPatch(typeof(NTinyCard), nameof(NTinyCard._Ready))]
public static class TinyCardTextureFilterPatch
{
  /// <summary>
  /// 小卡片节点创建完成后，若当前显示的是本模组替换立绘，则立即应用平滑过滤。
  /// </summary>
  static void Postfix(NTinyCard __instance)
  {
    CardFilterUtility.TryApplyTinyCardPortraitFilters(__instance);
  }
}

/// <summary>
/// 当大卡片第一次创建完成时，为替换立绘应用抗锯齿过滤，减少预览时的锯齿感。
/// </summary>
[HarmonyPatch(typeof(NCard), nameof(NCard._Ready))]
public static class CardTextureFilterPatch
{
  static void Postfix(NCard __instance)
  {
    CardFilterUtility.TryApplyNCardPortraitFilters(__instance);
  }
}

/// <summary>
/// 当大卡片因状态变化或重新生成而刷新时，重新补上平滑过滤，避免刷新后退回到默认锯齿渲染。
/// </summary>
[HarmonyPatch(typeof(NCard), "Reload")]
public static class CardTextureFilterReloadPatch
{
  static void Postfix(NCard __instance)
  {
    CardFilterUtility.TryApplyNCardPortraitFilters(__instance);
  }
}

/// <summary>
/// 提供卡牌立绘抗锯齿相关的公共方法。
/// 主要做法是把相关节点的 <see cref="CanvasItem.TextureFilter"/> 改为带 Mipmap 的线性过滤，
/// 让替换卡图在缩小、放大和悬停预览时都尽量保持平滑。
/// </summary>
public static class CardFilterUtility
{
  // Godot 4 中数值 4 对应“带 Mipmap 的线性过滤”。
  // 选择它是为了让卡图在各种缩放比例下都更平滑，从而减轻预览卡片时的锯齿。
  private const CanvasItem.TextureFilterEnum PreferredFilter = (CanvasItem.TextureFilterEnum)4;

  private static readonly string[] NCardPortraitNodePaths =
  {
        "%Portrait",
        "%AncientPortrait",
        "%PortraitBorder",
        "%PortraitCanvasGroup",
    };

  /// <summary>
  /// 尝试对大卡片应用平滑过滤。
  /// 只有当前卡面来自本模组替换立绘时才会生效，避免影响原版或其他模组的卡图。
  /// </summary>
  public static bool TryApplyNCardPortraitFilters(NCard card)
  {
    if (!CardPortraitReplacementPatch.IsModPortraitModel(card.Model))
    {
      return false;
    }

    ApplyNCardPortraitFilters(card);
    return true;
  }

  /// <summary>
  /// 为大卡片常用的立绘节点逐一设置平滑过滤。
  /// 同时递归处理 PortraitCanvasGroup 下的额外绘制节点，防止局部节点仍沿用默认最近邻采样。
  /// </summary>
  public static void ApplyNCardPortraitFilters(NCard card)
  {
    foreach (string nodePath in NCardPortraitNodePaths)
    {
      ApplyFilterToNode(card.GetNodeOrNull<CanvasItem>(nodePath));
    }

    CanvasItem? portraitCanvas = card.GetNodeOrNull<CanvasItem>("%PortraitCanvasGroup");
    if (portraitCanvas != null)
    {
      ApplySmoothFilteringRecursively(portraitCanvas);
    }
  }

  /// <summary>
  /// 尝试为小卡片的头像区域应用平滑过滤。
  /// 小卡在卡组、手牌和奖励界面里缩放更频繁，因此也更容易出现锯齿。
  /// </summary>
  public static bool TryApplyTinyCardPortraitFilters(NTinyCard tinyCard)
  {
    TextureRect? portrait = tinyCard.GetNodeOrNull<TextureRect>("%Portrait");
    if (portrait == null || !CardPortraitReplacementPatch.IsModPortraitTexture(portrait.Texture))
    {
      return false;
    }

    ApplyFilterToNode(portrait);
    ApplyFilterToNode(tinyCard.GetNodeOrNull<CanvasItem>("%PortraitShadow"));
    return true;
  }

  /// <summary>
  /// 递归处理某个节点及其全部子节点，确保相关画布项都统一使用平滑过滤。
  /// </summary>
  private static void ApplySmoothFilteringRecursively(Node node)
  {
    ApplyFilterToNode(node as CanvasItem);

    foreach (Node child in node.GetChildren())
    {
      ApplySmoothFilteringRecursively(child);
    }
  }

  /// <summary>
  /// 为单个画布节点设置首选过滤模式。
  /// </summary>
  private static void ApplyFilterToNode(CanvasItem? canvasItem)
  {
    if (canvasItem == null)
    {
      return;
    }

    canvasItem.TextureFilter = PreferredFilter;
  }
}