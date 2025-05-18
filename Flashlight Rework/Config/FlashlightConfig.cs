using System;
using BepInEx.Configuration;
using UnityEngine;

namespace PxntxrezStudio.RepoFlashlightRework
{
    public static class FlashlightConfig
    {
        public static ConfigEntry<int> LightColorRed;
        public static ConfigEntry<int> LightColorGreen;
        public static ConfigEntry<int> LightColorBlue;
        public static ConfigEntry<bool> UseCustomRGB;

        public static ConfigEntry<string> FlashlightToggleKey;
        public static ConfigEntry<bool> EnableFlicker;

        private static ConfigFile config;

        public static void Init(ConfigFile cfg)
        {
            config = cfg;

            UseCustomRGB = cfg.Bind(
                "Light Color",
                "Use Custom RGB Color",
                false,
                "Enable or disable RGB color override"
            );

            LightColorRed = cfg.Bind(
                "Light Color",
                "Red",
                255,
                new ConfigDescription("Light Color Red", new AcceptableValueRange<int>(0, 255))
            );
            LightColorGreen = cfg.Bind(
                "Light Color",
                "Green",
                255,
                new ConfigDescription("Light Color Green", new AcceptableValueRange<int>(0, 255))
            );
            LightColorBlue = cfg.Bind(
                "Light Color",
                "Blue",
                255,
                new ConfigDescription("Light Color Blue", new AcceptableValueRange<int>(0, 255))
            );

            FlashlightToggleKey = cfg.Bind(
                "Controls",
                "Toggle Key",
                "F",
                new ConfigDescription(
                    "Flashlight toggle key",
                    new AcceptableValueList<string>(
                        "F",
                        "R",
                        "T",
                        "Y",
                        "U",
                        "I",
                        "O",
                        "P",
                        "G",
                        "H",
                        "J",
                        "K",
                        "L",
                        "Z",
                        "X",
                        "C",
                        "V",
                        "B",
                        "N",
                        "M"
                    )
                )
            );

            EnableFlicker = cfg.Bind(
                "Flicker Settings",
                "Enable Flicker",
                true,
                "Enable flickering when enemy is nearby"
            );

            cfg.SettingChanged += OnConfigChanged;
        }

        private static void OnConfigChanged(object sender, SettingChangedEventArgs e)
        {
            if (e.ChangedSetting.Definition.Key == "Enable Flicker")
            {
                Patch_FlashlightController_Start.OnFlickerSettingChanged();
            }
        }

        public static KeyCode GetKeyCode()
        {
            string key = FlashlightToggleKey.Value.ToUpper();
            if (Enum.TryParse(key, out KeyCode parsed))
                return parsed;

            return KeyCode.F;
        }

        public static Color GetColor()
        {
            return new Color(
                LightColorRed.Value / 255f,
                LightColorGreen.Value / 255f,
                LightColorBlue.Value / 255f
            );
        }
    }
}
