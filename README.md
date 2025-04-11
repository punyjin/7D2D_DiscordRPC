# 📘 README | เลือกภาษา • Choose Language

- 🇹🇭 [ภาษาไทย (Thai)](READMETH.md)
- 🇺🇸 [English](README.md)


## 🇺🇸 English Language
# 🎮 7 Days to Die - Discord Rich Presence Mod
Bring your game to life on Discord with this mod that displays real-time in-game details for 7 Days to Die.

## 📁 Library References
- HarmonyLib (0Harmony)
- DiscordRPC
- Assembly-CSharp
- UnityEngine
- LobLibrary
  
## ✨ Features

- Displays map name, player name, server name
- Shows in-game day, blood moon survival, and status (Main Menu, Joining, In-Game)
- Shows total zombie kills and player level
- Configurable buttons in RPC (Main Menu and In-Game)
- Supports both English and Thai language
- Fully customizable via `Config.xml`

---

## ⚙️ Configuration Example (`Config.xml`)
```xml
<Config>
    <Language>English</Language> <!-- English or Thai -->
    
    <MainMenuButton enabled="true" multiplayerOnly="false"> <!-- Main Menu RPC Button -->
        <Label>View Mod</Label> <!-- Custom Text for Button -->
        <Url>https://github.com/punyjin</Url> 
    </MainMenuButton>

    <!-- multiplayerOnly is require multiplayer to show rpc Button -->

    <InGameButton enabled="true" multiplayerOnly="true"> <!-- In Game RPC Button --> 
        <Label>Join Server</Label> 
        <Url>https://www.youtube.com/shorts/41iWg91yFv0</Url> <!-- URL Link Button -->
﻿﻿    <!--<Url>steam://connect/your.server.ip:port</Url>--> <!-- Didn't Test -->
    </InGameButton>
    
    <ShowZombieKills>true</ShowZombieKills>  <!-- Turn On / Off ShowZombieKills (True / False) --> 
    <ShowLevel>true</ShowLevel>  <!-- Turn On / Off ShowPlayerLevel (True / False) --> 
</Config>
```

---
## 💡 How It Works

This mod uses [HarmonyLib](https://github.com/pardeike/Harmony) to patch the game and inject code that communicates with [DiscordRPC](https://github.com/discord/discord-rpc), so your in-game activity is reflected live on your Discord profile.

---

## 📁 Dependencies

- HarmonyLib (0Harmony)
- DiscordRPC
- Assembly-CSharp
- UnityEngine
- LobLibrary

---

## 🚀 Installation

1. Extract this mod into your Mods folder: ``7DaysToDie/Mods/DiscordRPC/``
2. Start Discord, then launch the game.
3. Your game status should now appear on your Discord profile.

---

## 👤 Author

**KazenoNeko**  
GitHub: [https://github.com/punyjin](https://github.com/punyjin)

---

## 📄 License

Open-source under the MIT License. Contributions welcome!


