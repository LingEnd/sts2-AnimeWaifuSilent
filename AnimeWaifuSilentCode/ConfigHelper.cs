namespace AnimeWaifuSilent.AnimeWaifuSilentCode;

public static class ConfigHelper
{
    private const bool DefaultAncientStyleEnabled = true;

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
