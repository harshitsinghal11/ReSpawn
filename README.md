# ReSpawn

> A privacy-first Windows game tracker that unifies playtime across every launcher.

<p align="center">
  <img src="/ReSpawn/Assets/demo.gif" alt="ReSpawn Demo">
</p>

Track your gaming time in one place, whether you launch games from Steam, Epic Games, Riot Client, Xbox App, Battle.net, GOG, desktop shortcuts, or directly from an executable.
No accounts. No subscriptions. No telemetry. No internet connection required.
Built for gamers who want accurate playtime tracking without being locked into a specific platform.

![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D4?style=flat-square&logo=windows)
![Stack](https://img.shields.io/badge/Stack-C%23%20·%20.NET%208%20·%20WPF-512BD4?style=flat-square&logo=dotnet)
![Version](https://img.shields.io/badge/Version-1.2.0-blue?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-yellow?style=flat-square)

---

## Why ReSpawn?

Modern PC gaming is fragmented.
Your playtime is spread across multiple launchers, stores, and standalone games. Steam tracks Steam games. Epic tracks Epic games. Everything else gets lost.
ReSpawn solves that problem by monitoring game processes directly on Windows and automatically recording playtime regardless of where the game was launched.

## Screenshots

<p align="center">
  <img src="/ReSpawn/Assets/home.png" width="900">
</p>

<p align="center">
  <img src="/ReSpawn/Assets/add-game.png" width="32%">
  <img src="/ReSpawn/Assets/edit-game.png" width="32%">
  <img src="/ReSpawn/Assets/ready-to-launch.png" width="32%">
</p>
### With ReSpawn, You Can

- Track all your games in one library
- Automatically record playtime across launchers
- Launch games from a single place
- Keep complete ownership of your data
- Use the application completely offline
- Avoid accounts, subscriptions, and cloud lock-in

---

## Key Benefits

### 🎮 One Unified Library

See your entire gaming collection and playtime in one place instead of jumping between launchers.

### ⏱ Accurate Automatic Tracking

Track gameplay whether a game is launched from Steam, Epic Games, Riot Client, Xbox App, a desktop shortcut, or directly from an executable.

### 🔒 Privacy First

No telemetry. No analytics. No tracking. No data collection.
Everything stays on your PC.

### ⚡ Lightweight & Native

Built with C# and .NET 8 using WPF for a fast, responsive Windows experience.
Uses less than 30 MB of memory while idle.

### 🌐 Fully Offline

ReSpawn never requires an internet connection and never sends data anywhere.

---

## Features

### 🎮 Game Library

- Add games using executable selection
- Drag & drop support
- Import `.lnk` and `.url` shortcuts
- Automatic icon extraction
- Custom cover artwork support
- Edit game details anytime
- Duplicate game detection
- Real-time search and filtering
- Total game count and playtime statistics

### ⏱ Playtime Tracking

- Automatic game detection
- Background process monitoring
- Live playtime updates
- Session tracking and recording
- Automatic session saving
- Startup recovery protection
- Orphaned session cleanup
- Minimum session threshold to prevent accidental launches
- Session validation safeguards

### 🖥 System Tray Integration

- Minimize to tray
- Continue tracking while hidden
- Startup with Windows
- Quick-access tray menu
- Single-instance protection

### 🎨 User Experience

- Clean dark interface
- Responsive game cards
- Running game indicators
- Missing executable warnings
- Relative last-played timestamps
- Keyboard shortcuts
- Context menu actions
- Smooth UI animations

---

## Supported Launchers

ReSpawn works independently of launchers and tracks games by monitoring running processes.
Compatible with:
- Steam
- Epic Games Launcher
- Riot Client
- Xbox App
- Battle.net
- GOG Galaxy
- EA App
- Ubisoft Connect
- Standalone Games
- Custom Launchers
- Desktop Shortcuts

If the game process starts, ReSpawn can track it.

---

## How It Works

```text
Launch Game
      ↓
Process Detected
      ↓
Playtime Tracking Starts
      ↓
Game Closed
      ↓
Session Saved
      ↓
Statistics Updated
```

ReSpawn monitors registered game processes in the background.
The launcher used does not matter.
When the game starts, tracking begins automatically. When the game closes, the session is saved and total playtime is updated instantly.

---

## Privacy

Privacy is a core design principle.

### ReSpawn Never

❌ Sends telemetry
❌ Collects analytics
❌ Requires user accounts
❌ Uploads your data
❌ Requires internet access

### ReSpawn Always

✅ Stores data locally
✅ Works completely offline
✅ Gives you full control over your information
✅ Keeps your game library private
All application data is stored locally:

```text
%AppData%\ReSpawn\
├── games.json
├── games.json.bak
└── icons\
```

---

## Installation

1. Download the latest release from the Releases page.
2. Run `ReSpawn.exe`.
3. Add your games.
4. Start tracking.

### Requirements

- Windows 10 (64-bit)
- Windows 11 (64-bit)

No installer required.
No .NET runtime required.

---

## Technology Stack

| Component | Technology |
|------------|------------|
| Language | C# 12 |
| Runtime | .NET 8 |
| UI Framework | WPF + MVVM |
| Storage | System.Text.Json |
| Testing | xUnit + Moq |
| Process Monitoring | System.Diagnostics.Process |
| System Tray | Hardcodet.NotifyIcon.Wpf |
| Registry Integration | Microsoft.Win32.Registry |

---

## Roadmap

Planned features and improvements:

- Session history view
- Detailed statistics dashboard
- Playtime charts
- Game collections and categories
- Data import/export
- Optional cloud backup
- Achievement tracking
- Enhanced analytics

---

## Contributing

Contributions, bug reports, and feature suggestions are welcome.
Feel free to open an issue or submit a pull request.

---

## Author

**Harshit Singhal**

---

### ReSpawn

**Track everything. Launch anything.**