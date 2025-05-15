using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace _7D2D_DecayMod
{
    public class DiscordRPCMod : IModApi
    {
        //Duscird RPC
        public static bool _inited = false;
        public static DiscordRPCManager _rpcManager;
        public static bool _isLoading = false;
        public static float _lastUpdateTime = 0f;
        public static float _updateCooldown = 10f;

        //State & Config
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

                var harmony = new Harmony(GetType().ToString());
                harmony.PatchAll(Assembly.GetExecutingAssembly());

                _rpcManager = new DiscordRPCManager("1257747591702249502", _config);
                _rpcManager.UpdatePresence(null);
            }
        }

        [HarmonyPatch(typeof(BackgroundMusicMono), "Start")]
        public class MainMenuMusic_Patch
        {
            static void PostFix(BackgroundMusicMono _modInstance)
            {
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
                    _rpcManager.UpdatePresence(null);
                    Log.Out("[DiscordRPCMod]: Non-local player disconnected, ignoring");
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
}
