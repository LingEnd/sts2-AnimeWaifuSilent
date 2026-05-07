using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

using Godot;

using FileAccess = Godot.FileAccess;

namespace AnimeWaifuSilent.AnimeWaifuSilentCode;

public class NormalReplacementEntry
{
    public string CardType { get; set; } = string.Empty;
    public string PortraitPath { get; set; } = string.Empty;
}

public class AncientReplacementEntry
{
    public string CardType { get; set; } = string.Empty;
    public string NormalPortrait { get; set; } = string.Empty;
    public string AncientPortrait { get; set; } = string.Empty;
    public string ConfigKey { get; set; } = string.Empty;
}

public class CardReplacementData
{
    public List<NormalReplacementEntry> NormalReplacements { get; set; } = new();
    public List<AncientReplacementEntry> AncientReplacements { get; set; } = new();
}

public static class CardReplacementConfig
{
    private static CardReplacementData? _data;
    private static bool _loaded;
    private static bool _loadFailed;
    private static Dictionary<string, string>? _cachedNormalReplacements;

    private const string ConfigPath = "res://AnimeWaifuSilent/card_replacements.json";

    public static Dictionary<string, string> NormalReplacements
    {
        get
        {
            EnsureLoaded();
            if (_cachedNormalReplacements != null)
            {
                return _cachedNormalReplacements;
            }

            _cachedNormalReplacements = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (_data?.NormalReplacements != null)
            {
                foreach (var entry in _data.NormalReplacements)
                {
                    if (!string.IsNullOrEmpty(entry.CardType) && !string.IsNullOrEmpty(entry.PortraitPath))
                    {
                        _cachedNormalReplacements[entry.CardType] = entry.PortraitPath;
                    }
                }
            }

            return _cachedNormalReplacements;
        }
    }

    public static List<AncientReplacementEntry> AncientReplacements
    {
        get
        {
            EnsureLoaded();
            return _data?.AncientReplacements ?? new List<AncientReplacementEntry>();
        }
    }

    public static List<string> AncientCardTypes
    {
        get
        {
            EnsureLoaded();
            return AncientReplacements
                .Where(a => !string.IsNullOrEmpty(a.CardType))
                .Select(a => a.CardType)
                .ToList();
        }
    }

    public static AncientReplacementEntry? GetAncientConfig(string cardType)
    {
        EnsureLoaded();
        return AncientReplacements.FirstOrDefault(a =>
            string.Equals(a.CardType, cardType, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsAncientCard(string cardType)
    {
        EnsureLoaded();
        return AncientReplacements.Any(a =>
            string.Equals(a.CardType, cardType, StringComparison.OrdinalIgnoreCase));
    }

    public static bool IsNormalReplacement(string cardType)
    {
        EnsureLoaded();
        return NormalReplacements.ContainsKey(cardType);
    }

    public static bool TryGetNormalPortrait(string cardType, out string? path)
    {
        return NormalReplacements.TryGetValue(cardType, out path);
    }

    public static bool TryGetAncientPortrait(string cardType, out string? path)
    {
        path = null;
        var config = GetAncientConfig(cardType);

        if (config != null && !string.IsNullOrEmpty(config.AncientPortrait))
        {
            path = config.AncientPortrait;
            return true;
        }

        return false;
    }

    public static bool TryGetFallbackPortrait(string cardType, out string? path)
    {
        var config = GetAncientConfig(cardType);

        if (config != null && !string.IsNullOrEmpty(config.NormalPortrait))
        {
            path = config.NormalPortrait;
            return true;
        }

        return TryGetNormalPortrait(cardType, out path);
    }

    private static void EnsureLoaded()
    {
        if (_loaded || _loadFailed)
        {
            return;
        }

        _loaded = true;

        if (!FileAccess.FileExists(ConfigPath))
        {
            _loadFailed = true;
            MainFile.Logger.Error($"[Config] Card replacement config not found at {ConfigPath}");
            throw new Exception($"Card replacement config not found: {ConfigPath}");
        }

        using var file = FileAccess.Open(ConfigPath, FileAccess.ModeFlags.Read);
        if (file == null)
        {
            _loadFailed = true;
            MainFile.Logger.Error($"[Config] Failed to open config file: {ConfigPath}");
            throw new Exception($"Failed to open config file: {ConfigPath}");
        }

        string jsonContent = file.GetAsText();

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        try
        {
            _data = JsonSerializer.Deserialize<CardReplacementData>(jsonContent, options);
        }
        catch (JsonException jsonEx)
        {
            _loadFailed = true;
            MainFile.Logger.Error($"[Config] JSON parsing error: {jsonEx.Message}");
            throw new Exception($"JSON parsing error: {jsonEx.Message}");
        }

        if (_data == null)
        {
            _loadFailed = true;
            MainFile.Logger.Error("[Config] Failed to parse config file");
            throw new Exception("Failed to parse config file");
        }

        MainFile.Logger.Info($"[Config] Loaded: {NormalReplacements.Count} normal replacements, {AncientReplacements.Count} ancient replacements");
    }
}
