using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace _7D2D_DecayMod
{
    public class Config
    {
        public string Language { get; set; } = "English";
        public ButtonConfig MainMenuButton { get; set; }
        public ButtonConfig InGameButton { get; set; }
        public bool ShowZombieKills { get; set; } = true;
        public bool ShowLevel { get; set; } = true;

        public bool UseGameName { get; set; } = true;
        public static Config Load(string path)
        {
            Config config = new Config();
            if (File.Exists(path))
            {
                try
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(path);
                    XmlNode root = doc.DocumentElement;

                    config.Language = root.SelectSingleNode("Language")?.InnerText ?? "English";
                    config.MainMenuButton = LoadButtonConfig(root.SelectSingleNode("MainMenuButton"));
                    config.InGameButton = LoadButtonConfig(root.SelectSingleNode("InGameButton"));
                    config.ShowZombieKills = bool.Parse(root.SelectSingleNode("ShowZombieKills")?.InnerText ?? "true");
                    config.ShowLevel = bool.Parse(root.SelectSingleNode("ShowLevel")?.InnerText ?? "true");
                    config.UseGameName = bool.Parse(root.SelectSingleNode("UseGameName")?.InnerText ?? "true");
                }
                catch (Exception e)
                {
                    Log.Error($"[DiscordRPCMod]: Error loading config - {e.Message}");
                }
            }
            else
            {
                Log.Out("[DiscordRPCMod]: Config file not found, using defaults");
            }
            return config;
        }

        private static ButtonConfig LoadButtonConfig(XmlNode node)
        {
            if (node == null) return null;
            bool enabled = bool.Parse(node.Attributes["enabled"]?.Value ?? "false");
            if (!enabled) return null;
            return new ButtonConfig
            {
                Label = node.SelectSingleNode("Label")?.InnerText,
                Url = node.SelectSingleNode("Url")?.InnerText,
                MultiplayerOnly = bool.Parse(node.Attributes["multiplayerOnly"]?.Value ?? "false")
            };
        }
        public class ButtonConfig
        {
            public string Label { get; set; }
            public string Url { get; set; }
            public bool MultiplayerOnly { get; set; }
        }
    }
}
