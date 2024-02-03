using Newtonsoft.Json;
using System.Numerics;

namespace Envision.Util;

public static class Config
{
    internal static int MaximumTextureSize => _maximumTexSize;
    private const int _maximumTexSize = 256;

    public static InternalSettings Settings
    {
        get => _settings;
        set
        {
            _settings = value;
        }
    }
    private static InternalSettings _settings;
    public static string? SaveDirectory { get; set; }

    /// <summary> The settings loaded from a custom config file in debug mode. </summary>
    public static void LoadFromCustomFile(string directory, string shaderPath)
    {
        SaveDirectory = directory;
        string json;
        if (File.Exists($"{SaveDirectory}\\Config.json"))
        {
            json = File.ReadAllText($"{SaveDirectory}\\Config.json");
        }
        else
        {
            Settings = DefaultSettings(true);
            return;
        }

        if (json.Length == 0)
        {
            DebugLogger.Log($"<red>Failed to load config is the path valid?");
            InternalSettings defaultSettings = DefaultSettings(true);
            defaultSettings.ShaderPath = shaderPath;
            Settings = defaultSettings;
            return;
        }

        InternalSettings settings = JsonConvert.DeserializeObject<InternalSettings>(json);

        string? fontPath;
        if (File.Exists($"{directory}\\Fonts\\{settings.Font}.ttf"))
        {
            fontPath = $"{directory}\\Fonts\\{settings.Font}.ttf";
        }
        else
        {
            DebugLogger.Log($"<red>Failed to load font {settings.Font} is the path valid?");
            fontPath = null;
        }

        fontPath ??= $"{directory}\\Fonts\\DroidSans.ttf";

        settings.FontPath = fontPath;
        settings.ShaderPath = shaderPath;
        Settings = settings;
    }

    /// <summary> Loads the settings from the config file relative to the assembly in release mode. </summary>
    public static void LoadFromRelative()
    {
    }

    /// <summary> Saves the settings to the default config file. </summary>
    public static void Save()
    {
        if (SaveDirectory is null) throw new InvalidOperationException("Cannot save config without a path.");
        string json = JsonConvert.SerializeObject(Settings);
        try
        {
            File.WriteAllText($"{SaveDirectory}\\Config.json", json);
        }
        catch (IOException e)
        {
            DebugLogger.Log($"<red>Failed to save config is the path valid?");
            DebugLogger.Log(e.Message);
        }
    }

    private static InternalSettings DefaultSettings(bool debug)
    {
        return new InternalSettings
        {
            Resolution = new Vector2(2560, 1440),
            MousePosition = new Vector2(0, 0),
            MouseSensitivity = new Vector2(0.1f, 0.1f),
            Font = "DroidSans",
            FontSizePixels = 20.0f,
            FontPath = null,
            ShaderPath = "",
            DebugMode = debug
        };
    }

    public struct InternalSettings
    {
        public Vector2 Resolution;
        public Vector2 MousePosition;
        public Vector2 MouseSensitivity;
        public string Font;
        public float FontSizePixels;
        public string? FontPath;
        public string ShaderPath;
        public bool DebugMode;
    }
}
