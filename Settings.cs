using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace AudiPlayer
{
    public class StickerData
    {
        public string Emoji { get; set; } = "⭐";
        public double X { get; set; } = 50;
        public double Y { get; set; } = 50;
        public double Size { get; set; } = 20;
    }

    public class Settings
    {
        public string ClientId { get; set; } = "";
        public string ClientSecret { get; set; } = "";
        public string RedirectUri { get; set; } = "http://127.0.0.1:8888/callback";
        public int RedirectPort { get; set; } = 8888;

        public bool AlwaysOnTop { get; set; } = true;
        public bool UseGradients { get; set; } = true;

        public List<StickerData> Stickers { get; set; } = new List<StickerData>();

        public string iPodBodyColor { get; set; } = "#F8F8F8";
        public string iPodBodyColor2 { get; set; } = "#D8D8D8";
        public string iPodBorderColor { get; set; } = "#C0C0C0";

        public string ScreenBackgroundColor { get; set; } = "#FAFAFA";
        public string ScreenBackgroundColor2 { get; set; } = "#F0F0F0";
        public string ScreenBorderColor { get; set; } = "#999999";
        public string ScreenTextColor { get; set; } = "#000000";
        public string ScreenSubtextColor { get; set; } = "#666666";

        public string ClickWheelColor { get; set; } = "#F5F5F5";
        public string ClickWheelColor2 { get; set; } = "#D8D8D8";
        public string ClickWheelBorderColor { get; set; } = "#999999";
        public string ClickWheelCenterColor { get; set; } = "#F0F0F0";
        public string ClickWheelCenterColor2 { get; set; } = "#D0D0D0";

        public string ButtonTextColor { get; set; } = "#333333";

        public string ProgressBarColor { get; set; } = "#1ED760";
        public string ProgressBarColor2 { get; set; } = "#1ABC54";
        public string ProgressBarBackgroundColor { get; set; } = "#E0E0E0";

        public int ThemePreset { get; set; } = 0;

        private static string SettingsFilePath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "AudiPlayer",
            "settings.json"
        );

        public static Settings Load()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    return JsonConvert.DeserializeObject<Settings>(json) ?? new Settings();
                }
            }
            catch { }
            return new Settings();
        }

        public void Save()
        {
            try
            {
                string? directory = Path.GetDirectoryName(SettingsFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                string json = JsonConvert.SerializeObject(this, Formatting.Indented);
                File.WriteAllText(SettingsFilePath, json);
            }
            catch { }
        }

        public void ApplyThemePreset(int preset)
        {
            ThemePreset = preset;

            switch (preset)
            {
                case 1: // Classic White
                    UseGradients = false;
                    iPodBodyColor = "#F0F0F0";
                    ScreenBackgroundColor = "#FFFFFF";
                    ClickWheelColor = "#E8E8E8";
                    ProgressBarColor = "#1ED760";
                    break;

                case 2: // Dark Mode
                    UseGradients = true;
                    iPodBodyColor = "#2C2C2C";
                    iPodBodyColor2 = "#1A1A1A";
                    ScreenBackgroundColor = "#1E1E1E";
                    ScreenBackgroundColor2 = "#121212";
                    ScreenBorderColor = "#444444";
                    ScreenTextColor = "#FFFFFF";
                    ScreenSubtextColor = "#AAAAAA";
                    ClickWheelColor = "#3C3C3C";
                    ClickWheelColor2 = "#2A2A2A";
                    ClickWheelCenterColor = "#4A4A4A";
                    ClickWheelCenterColor2 = "#353535";
                    ButtonTextColor = "#CCCCCC";
                    ProgressBarColor = "#1ED760";
                    ProgressBarColor2 = "#1ABC54";
                    ProgressBarBackgroundColor = "#444444";
                    break;

                case 3: // Spotify Green
                    UseGradients = true;
                    iPodBodyColor = "#1DB954";
                    iPodBodyColor2 = "#1AA34A";
                    ScreenBackgroundColor = "#191414";
                    ScreenBackgroundColor2 = "#0E0E0E";
                    ScreenBorderColor = "#1ED760";
                    ScreenTextColor = "#FFFFFF";
                    ScreenSubtextColor = "#B3B3B3";
                    ClickWheelColor = "#1ED760";
                    ClickWheelColor2 = "#1ABC54";
                    ClickWheelCenterColor = "#191414";
                    ClickWheelCenterColor2 = "#0E0E0E";
                    ButtonTextColor = "#FFFFFF";
                    ProgressBarColor = "#FFFFFF";
                    ProgressBarColor2 = "#E0E0E0";
                    ProgressBarBackgroundColor = "#404040";
                    break;

                case 4: // Ocean Blue
                    UseGradients = true;
                    iPodBodyColor = "#4A90E2";
                    iPodBodyColor2 = "#357ABD";
                    ScreenBackgroundColor = "#E8F4F8";
                    ScreenBackgroundColor2 = "#D0E8F0";
                    ScreenBorderColor = "#4A90E2";
                    ScreenTextColor = "#1A3A52";
                    ScreenSubtextColor = "#5B7C8D";
                    ClickWheelColor = "#7BB3E0";
                    ClickWheelColor2 = "#5A9DD5";
                    ClickWheelCenterColor = "#C8E0F0";
                    ClickWheelCenterColor2 = "#A8D0E8";
                    ButtonTextColor = "#1A3A52";
                    ProgressBarColor = "#4A90E2";
                    ProgressBarColor2 = "#357ABD";
                    ProgressBarBackgroundColor = "#D0E8F0";
                    break;

                case 5: // Sunset Orange
                    UseGradients = true;
                    iPodBodyColor = "#FF6B35";
                    iPodBodyColor2 = "#E85D2A";
                    ScreenBackgroundColor = "#FFF8E7";
                    ScreenBackgroundColor2 = "#FFE8CC";
                    ScreenBorderColor = "#FF6B35";
                    ScreenTextColor = "#8B4513";
                    ScreenSubtextColor = "#A0522D";
                    ClickWheelColor = "#FFA07A";
                    ClickWheelColor2 = "#FF8C5A";
                    ClickWheelCenterColor = "#FFD4AA";
                    ClickWheelCenterColor2 = "#FFC090";
                    ButtonTextColor = "#8B4513";
                    ProgressBarColor = "#FF6B35";
                    ProgressBarColor2 = "#E85D2A";
                    ProgressBarBackgroundColor = "#FFE8CC";
                    break;

                case 6: // Purple Dream
                    UseGradients = true;
                    iPodBodyColor = "#9B59B6";
                    iPodBodyColor2 = "#8E44AD";
                    ScreenBackgroundColor = "#F4E8FF";
                    ScreenBackgroundColor2 = "#E8D4FF";
                    ScreenBorderColor = "#9B59B6";
                    ScreenTextColor = "#4A235A";
                    ScreenSubtextColor = "#6C3483";
                    ClickWheelColor = "#C39BD3";
                    ClickWheelColor2 = "#AF7AC5";
                    ClickWheelCenterColor = "#E8DAEF";
                    ClickWheelCenterColor2 = "#D7BDE2";
                    ButtonTextColor = "#4A235A";
                    ProgressBarColor = "#9B59B6";
                    ProgressBarColor2 = "#8E44AD";
                    ProgressBarBackgroundColor = "#E8D4FF";
                    break;
            }
        }
    }
}
