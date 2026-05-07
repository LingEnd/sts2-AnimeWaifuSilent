namespace AnimeWaifuSilent.AnimeWaifuSilentCode;

/// <summary>
/// 配置值获取辅助类。
/// </summary>
public static class ConfigHelper
{
    private const bool DefaultAncientStyleEnabled = true;

    /// <summary>
    /// 获取指定卡牌的先古样式是否启用。
    /// </summary>
    public static bool IsAncientStyleEnabled(string cardTypeName)
    {
        try
        {
            return BaseLibIntegration.GetAncientStyleConfig(cardTypeName);
        }
        catch
        {
            return DefaultAncientStyleEnabled;
        }
    }
}