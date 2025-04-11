using UnityEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib.Tools;
using System;
using System.Linq;
using DiscordRPC;
using static UnityDistantTerrain;
using System.IO;
using System.Xml;

namespace _7D2D_DecayMod
{
    public class DiscordRPCMod : IModApi
    {
        private static bool _inited = false;
        private static DiscordRPCManager _rpcManager;
        public static bool _isLoading = false;
        public static float _lastUpdateTime = 0f;
        public static float _updateCooldown = 5f;
        
        public static string _lastState = null;
        public static Config _config;
        public void InitMod(Mod _modInstance)
        {
            if (!_inited)
            {
                _inited = true;
                Log.Out("[DiscordRPCMod]: Initializing Discord RPC System");

                // โหลด config
                string configPath = Path.Combine(_modInstance.Path, "Config.xml");
                _config = Config.Load(configPath);

                var harmony = new HarmonyLib.Harmony(GetType().ToString());
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                _rpcManager = new DiscordRPCManager("1257747591702249502", _config);
                _rpcManager.UpdatePresence(null);
            }
        }

        // Patch เมื่อเริ่มโหลดเกม
        [HarmonyPatch(typeof(GameManager), "StartGame")]
        public class GameStartPatch
        {
            static void Prefix()
            {
                Log.Out("[DiscordRPCMod]: Game start detected, setting to Loading");
                _isLoading = true; // ตั้งค่าเป็น Loading เมื่อเริ่มเกม
                _rpcManager.UpdatePresence(null);
            }
        }

        // Patch เมื่อผู้เล่นเกิดในโลก
        // เปลี่ยนจาก Awake ไป Update เพื่อให้ Players.list อัปเดตทัน
        [HarmonyPatch(typeof(EntityPlayerLocal), "Update")]
        public class PlayerUpdatePatch
        {
            static void Postfix(EntityPlayerLocal __instance)
            {
                _rpcManager.UpdatePresence(__instance);
            }
        }

        [HarmonyPatch(typeof(GameManager), "Cleanup")]
        public class GameCleanupPatch
        {
            static void Postfix()
            {
                Log.Out("[DiscordRPCMod]: Game cleanup detected, resetting to Main Menu and disposing RPC");
                _isLoading = false;
                _lastState = null;
                _rpcManager.UpdatePresence(null); // ตั้งกลับไป Main Menu ก่อน Dispose
                _rpcManager?.Dispose();
            }
        }

        [HarmonyPatch(typeof(GameManager), "PlayerDisconnected")]
        public class PlayerDisconnectedPatch
        {
            static void Postfix(ClientInfo _cInfo)
            {
                EntityPlayerLocal localPlayer = GameManager.Instance.World?.GetPrimaryPlayer();
                if (localPlayer != null && _cInfo != null && localPlayer.entityId == _cInfo.entityId)
                {
                    Log.Out("[DiscordRPCMod]: Local player disconnected, resetting to Main Menu");
                    _isLoading = false;
                    _lastState = null;
                    _rpcManager.UpdatePresence(null);
                }
                else
                {
                    Log.Out("[DiscordRPCMod]: Non-local player disconnected, no RPC reset");
                }
            }
        }

        [HarmonyPatch(typeof(GameManager), "SaveAndCleanupWorld")]
        public class SaveAndCleanupWorldPatch
        {
            static void Prefix()
            {
                Log.Out("[DiscordRPCMod]: SaveAndCleanupWorld detected, resetting to Main Menu");
                _isLoading = false;
                _lastState = null; // รีเซ็ตเพื่อบังคับอัปเดต RPC
                _rpcManager.UpdatePresence(null); // ตั้งกลับไป Main Menu
            }
        }
    }
    public class Config
    {
        public string Language { get; set; } = "English";
        public ButtonConfig MainMenuButton { get; set; }
        public ButtonConfig InGameButton { get; set; }
        public bool ShowZombieKills { get; set; } = true;
        public bool ShowLevel { get; set; } = true;
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
    }
    public class ButtonConfig
    {
        public string Label { get; set; }
        public string Url { get; set; }
        public bool MultiplayerOnly { get; set; }
    }
    public class DiscordRPCManager
    {
        private readonly DiscordRpcClient _client;
        private readonly string _detailsFormat = "Playing as {0} on {1}"; // อังกฤษ
        private readonly string _detailsFormatThai = "กำลังเล่นเป็น {0} บน {1}"; // ไทย
        private int _cachedMaxSlots;
        private int _lastZombieKills = -1;
        public static int _zombieKills = 0;
        private readonly Config _config;
        public DiscordRPCManager(string applicationId, Config config)
        {
            _client = new DiscordRpcClient(applicationId);
            _client.OnReady += (sender, e) => Log.Out($"[DiscordRPCMod]: Discord RPC Ready for user {e.User.Username}");
            _client.OnError += (sender, e) => Log.Error($"[DiscordRPCMod]: Discord RPC Error - {e.Message}");
            _config = config;
            if (!InitializeClient()) ReInitializeAfterDelay();
            _cachedMaxSlots = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);
            Log.Out("[DiscordRPCMod]: Discord RPC Initialized");
        }

        private bool InitializeClient()
        {
            try
            {
                _client.Initialize();
                return true;
            }
            catch (Exception e)
            {
                Log.Error($"[DiscordRPCMod]: Failed to initialize Discord RPC - {e.Message}");
                return false;
            }
        }

        private void ReInitializeAfterDelay()
        {
            Log.Out("[DiscordRPCMod]: Attempting to re-initialize Discord RPC in 10 seconds");
            DiscordRPCMod._lastUpdateTime = Time.time + 10f;
        }

        private bool IsBloodMoon()
        {
            if (GameManager.Instance.World == null) return false;
            ulong worldTime = GameManager.Instance.World.GetWorldTime();
            int day = (int)(worldTime / 24000UL);
            int timeOfDay = (int)(worldTime % 24000UL);
            bool isBloodMoonDay = (day % 7 == 0);
            bool isNight = timeOfDay >= 12000 && timeOfDay < 22000;
            return isBloodMoonDay && isNight;
        }
        private int GetDayCount()
        {
            if (GameManager.Instance.World == null) return 0;
            ulong worldTime = GameManager.Instance.World.GetWorldTime();
            return (int)(worldTime / 24000UL) + 1; // +1 เพื่อให้เริ่มที่วัน 1 แทน 0
        }

        public void UpdatePresence(EntityPlayerLocal player)
        {
            if (_client == null || !_client.IsInitialized)
            {
                if (Time.time > DiscordRPCMod._lastUpdateTime)
                {
                    Log.Out("[DiscordRPCMod]: Re-initializing Discord RPC");
                    InitializeClient();
                }
                return;
            }

            try
            {
                string currentState;
                bool hasWorld = GameManager.Instance != null && GameManager.Instance.World != null;
                int playerCount = hasWorld ? GameManager.Instance.World.Players.list.Count : 0;

                // อัปเดต _zombieKills และตรวจสอบการเปลี่ยนแปลง
                bool zombieKillsChanged = false;
                if (_config.ShowZombieKills && player != null && player.KilledZombies > _zombieKills)
                {
                    _zombieKills = player.KilledZombies;
                    Log.Out($"[DiscordRPCMod]: Updated zombie kills to {_zombieKills}");
                    zombieKillsChanged = true;
                }

                if (!hasWorld && !DiscordRPCMod._isLoading)
                {
                    currentState = "MainMenu";
                }
                else if (DiscordRPCMod._isLoading && (player == null || !player.IsAlive()))
                {
                    currentState = "Loading";
                }
                else if (player != null && !player.IsAlive())
                {
                    currentState = "Dead";
                }
                else if (player != null && player.IsAlive())
                {
                    currentState = "InGame";
                    DiscordRPCMod._isLoading = false;
                }
                else
                {
                    currentState = "MainMenu";
                }

                // อัปเดต RPC ถ้าสถานะเปลี่ยน หรือ _zombieKills เปลี่ยน และหมด cooldown
                if ((currentState != DiscordRPCMod._lastState || zombieKillsChanged) && Time.time > DiscordRPCMod._lastUpdateTime + DiscordRPCMod._updateCooldown)
                {
                    Log.Out($"[DiscordRPCMod]: Debug - HasWorld: {hasWorld}, PlayerCount: {playerCount}, PlayerNull: {player == null}, IsLoading: {DiscordRPCMod._isLoading}");
                    switch (currentState)
                    {
                        case "MainMenu":
                            Log.Out("[DiscordRPCMod]: Detected Main Menu state");
                            UpdateMainMenuPresence(playerCount);
                            break;
                        case "Loading":
                            Log.Out("[DiscordRPCMod]: Detected Loading state");
                            UpdateConnectingPresence(playerCount);
                            break;
                        case "Dead":
                            Log.Out("[DiscordRPCMod]: Detected Dead state");
                            UpdateDeadPresence(player, playerCount);
                            break;
                        case "InGame":
                            Log.Out("[DiscordRPCMod]: Detected In-Game state");
                            UpdateInGamePresence(player, playerCount);
                            break;
                    }
                    DiscordRPCMod._lastState = currentState;
                    DiscordRPCMod._lastUpdateTime = Time.time;
                    _lastZombieKills = _zombieKills; // อัปเดตค่าเก่า
                }
            }
            catch (Exception e)
            {
                Log.Error($"[DiscordRPCMod]: Error updating Discord RPC - {e.Message}");
            }
        }

        private void UpdateMainMenuPresence(int playerCount)
        {
            Button[] buttons = null;
            if (_config.MainMenuButton != null)
            {
                if (!_config.MainMenuButton.MultiplayerOnly || playerCount > 1)
                {
                    buttons = new Button[]
                    {
                        new Button
                        {
                            Label = _config.MainMenuButton.Label,
                            Url = _config.MainMenuButton.Url
                        }
                    };
                    //Log.Out("[DiscordRPCMod]: Main Menu button added - Label: " + _config.MainMenuButton.Label + ", URL: " + _config.MainMenuButton.Url);
                }
                else
                {
                    Log.Out("[DiscordRPCMod]: Main Menu button skipped due to MultiplayerOnly restriction and playerCount <= 1");
                }
            }
            else
            {
                Log.Out("[DiscordRPCMod]: Main Menu button not set because _config.MainMenuButton is null");
            }

            _client.SetPresence(new RichPresence
            {
                Details = _config.Language == "Thai" ? "อยู่ในเมนูหลัก" : "In Main Menu",
                State = _config.Language == "Thai" ? "กำลังสำรวจวันสิ้นโลก" : "Browsing the Apocalypse",
                Timestamps = new Timestamps { Start = DateTime.UtcNow },
                Assets = new DiscordRPC.Assets
                {
                    LargeImageKey = "7dtd_icon",
                    LargeImageText = "7 Days To Die - xorbit256 Mods",
                    SmallImageKey = "main_menu_icon",
                    SmallImageText = "In Main Menu"
                },
                Buttons = buttons
            });
            Log.Out("[DiscordRPCMod]: Main Menu presence set");
        }

        private void UpdateConnectingPresence(int playerCount)
        {
            string serverName = GetServerName();
            Button[] buttons = _config.InGameButton != null && (!_config.InGameButton.MultiplayerOnly || playerCount > 1)
                ? new Button[] { new Button { Label = _config.InGameButton.Label, Url = _config.InGameButton.Url } }
                : null;

            _client.SetPresence(new RichPresence
            {
                Details = _config.Language == "Thai" ? "กำลังโหลดข้อมูล..." : "Loading Game...",
                State = _config.Language == "Thai" ? $"กำลังเข้าร่วมเซิร์ฟเวอร์ {serverName}" : $"Joining Server {serverName}",
                Timestamps = new Timestamps { Start = DateTime.UtcNow },
                Assets = new DiscordRPC.Assets 
                { 
                    LargeImageKey = "7dtd_icon",
                    LargeImageText = "7 Days To Die - xorbit256 Mods",
                    SmallImageKey = "loading_icon", 
                    SmallImageText = "Connecting To Server..." 
                },
                Buttons = buttons
            });
            Log.Out("[DiscordRPCMod]: Loading presence set");
        }

        private void UpdateDeadPresence(EntityPlayerLocal player, int playerCount)
        {
            string serverName = GetServerName();
            int dayCount = GetDayCount();
            Button[] buttons = _config.InGameButton != null && (!_config.InGameButton.MultiplayerOnly || playerCount > 1)
                ? new Button[] { new Button { Label = _config.InGameButton.Label, Url = _config.InGameButton.Url } }
                : null;

            _client.SetPresence(new RichPresence
            {
                Details = _config.Language == "Thai" ? "ตายแล้ว" : "Dead",
                State = _config.Language == "Thai" ? $"ตายบน {serverName} | วันที่: {dayCount}" : $"Died on {serverName} | Day: {dayCount}",
                Timestamps = new Timestamps { Start = DateTime.UtcNow },
                Assets = new DiscordRPC.Assets 
                {
                    LargeImageKey = "7dtd_icon",
                    LargeImageText = "7 Days To Die - xorbit256 Mods",
                    SmallImageKey = "dead_icon", 
                    SmallImageText = "Player Dead" 
                },
                Buttons = buttons
            });
            Log.Out("[DiscordRPCMod]: Dead presence set");
        }

        private void UpdateInGamePresence(EntityPlayerLocal player, int playerCount)
        {
            string playerName = player.EntityName ?? "Unknown Player";
            string serverName = GetServerName();
            string mapName = GetMapName();
            int dayCount = GetDayCount();
            string playerList = playerCount > 1 ? $" | Players: {string.Join(", ", GameManager.Instance.World.Players.list.Select(p => p.EntityName.Substring(0, 3)))}" : "";
            string bloodMoonStatus = IsBloodMoon() ? (_config.Language == "Thai" ? " | คืนพระจันทร์สีเลือด!" : " | Blood Moon!") : "";
            string details = _config.Language == "Thai" ? string.Format(_detailsFormatThai, playerName, serverName) : string.Format(_detailsFormat, playerName, serverName);
            string zombieKillsText = _config.ShowZombieKills ? $" | Killed: {_zombieKills}" : "";
            string levelText = _config.ShowLevel ? $" | Level: {player.Progression.Level}" : "";
            Button[] buttons = _config.InGameButton != null && (!_config.InGameButton.MultiplayerOnly || playerCount > 1)
                ? new Button[] { new Button { Label = _config.InGameButton.Label, Url = _config.InGameButton.Url } }
                : null;

            _client.SetPresence(new RichPresence
            {
                Details = $"{details}{zombieKillsText}{levelText}",
                State = $"{playerCount}/{_cachedMaxSlots} Players | Day: {dayCount} | Map: {mapName}{bloodMoonStatus}{playerList}",
                Timestamps = new Timestamps { Start = DateTime.UtcNow },
                Assets = new DiscordRPC.Assets 
                {
                    LargeImageKey = "7dtd_icon",
                    LargeImageText = "7 Days To Die - xorbit256 Mods",
                    SmallImageKey = "ingame_icon", 
                    SmallImageText = "In Game Playing" 
                },
                Buttons = buttons
            });
            Log.Out("[DiscordRPCMod]: In-Game presence set");
        }

        private static string GetServerName()
        {
            try
            {
                return GamePrefs.GetString(EnumGamePrefs.ServerName) ?? "Unknown Server";
            }
            catch (Exception e)
            {
                Log.Error($"[DiscordRPCMod]: Error getting server name - {e.Message}");
                return "Unknown Server";
            }
        }

        private static string GetMapName()
        {
            try
            {
                string mapName = GamePrefs.GetString(EnumGamePrefs.GameWorld);
                if (string.IsNullOrEmpty(mapName))
                {
                    mapName = GamePrefs.GetString(EnumGamePrefs.WorldGenSeed) ?? "Unknown Map";
                }
                return mapName;
            }
            catch (Exception e)
            {
                Log.Error($"[DiscordRPCMod]: Error getting map name - {e.Message}");
                return "Unknown Map";
            }
        }

        public void Dispose()
        {
            if (_client != null)
            {
                _client.Dispose();
                Log.Out("[DiscordRPCMod]: Discord RPC Disposed");
            }
        }
    }
}