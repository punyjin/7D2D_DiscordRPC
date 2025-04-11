# 📘 README | เลือกภาษา • Choose Language

- 🇹🇭 [ภาษาไทย (Thai)](#ภาษาไทย-thai)
- 🇺🇸 [English](#english)


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

## 🇹🇭 ภาษาไทย (Thai)

### 🎮 7 Days to Die - ม็อด Discord Rich Presence

อัปเกรดสถานะ Discord ของคุณให้แสดงข้อมูลจากเกม 7 Days to Die แบบเรียลไทม์

#### ✨ ฟีเจอร์
- แสดงชื่อแผนที่ ชื่อผู้เล่น และชื่อเซิร์ฟเวอร์
- แสดงวันในเกม การรอดจาก Blood Moon และสถานะของเกม (เมนูหลัก, กำลังเข้าเกม, อยู่ในเกม)
- แสดงจำนวนซอมบี้ที่ฆ่า และเลเวลของผู้เล่น
- ปุ่ม RPC ตั้งค่าได้ ทั้งในเมนูหลักและระหว่างเล่นเกม
- รองรับภาษาไทยและอังกฤษ
- ปรับแต่งได้ผ่านไฟล์ `Config.xml`

## ⚙️ ตัวอย่างการตั้งค่าไฟล์ (`Config.xml`)
```xml
<Config>
    <Language>English</Language> <!-- English or Thai -->
    
    <MainMenuButton enabled="true" multiplayerOnly="false"> <!-- ปุ่มหน้าเมนูหลัก -->
        <Label>View Mod</Label> <!-- ข้อความแบบกำหนดเองของปุ่ม -->
        <Url>https://github.com/punyjin</Url> 
    </MainMenuButton>

    <!-- ค่า multiplayerOnly นั้น เป็นการตั้งค่าว่าจะแสดง RPC ส่วนของปุ่ม เฉพาะโหมดเล่นหลายคนเท่านั้น หากเล่นคนเดียวจะไม่แสดง -->

    <InGameButton enabled="true" multiplayerOnly="true"> <!-- ปุ่มในาสถานะ Ingame --> 
        <Label>Join Server</Label> 
        <Url>https://www.youtube.com/shorts/41iWg91yFv0</Url> <!-- ลิ้งค์สำหรับปุ่ม -->
﻿﻿<!--<Url>steam://connect/your.server.ip:port</Url>--> <!-- ส่วนนี้ยังไม่ได้รับการทดสอบ -->
    </InGameButton>
    
    <ShowZombieKills>true</ShowZombieKills>  <!-- ปิด หรือ เปิด การแสดงผลจำนวนซอมบี้ที่ฆ่า (True / False) --> 
    <ShowLevel>true</ShowLevel>  <!-- ปิด หรือ เปิด การแสดงผลเลเวลผู้เล่น (True / False) --> 
</Config>
```

#### 💡 วิธีการทำงาน
ม็อดนี้ใช้ [HarmonyLib](https://github.com/pardeike/Harmony) เพื่อดัดแปลงเกมและดึงข้อมูลสถานะเกมไปแสดงผลบน Discord โดยผ่าน [DiscordRPC](https://github.com/discord/discord-rpc)

#### 📁 ไลบรารีที่ใช้
- HarmonyLib (0Harmony)
- DiscordRPC
- Assembly-CSharp
- UnityEngine
- LobLibrary

#### 🚀 วิธีติดตั้ง
1. แตกไฟล์ม็อดไว้ที่โฟลเดอร์: ``7DaysToDie/Mods/DiscordRPC/``
2. เปิด Discord และรันเกม
3. สถานะเกมจะแสดงบนโปรไฟล์ Discord ของคุณโดยอัตโนมัติ

#### 👤 ผู้พัฒนา
**KazenoNeko**  
GitHub: [https://github.com/punyjin](https://github.com/punyjin)

---

## 📄 License

Open-source under the MIT License. Contributions welcome!

