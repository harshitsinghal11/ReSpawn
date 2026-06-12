# ReSpawn

> A lightweight, offline-first Windows desktop app that unifies your game library and automatically tracks playtime — silently, natively, no internet required.

![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D4?style=flat-square&logo=windows)
![Stack](https://img.shields.io/badge/Stack-C%23%20·%20.NET%208%20·%20WPF-512BD4?style=flat-square&logo=dotnet)
![Version](https://img.shields.io/badge/Version-1.1.0-blue?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-yellow?style=flat-square)
![Build](https://img.shields.io/badge/Build-Passing-brightgreen?style=flat-square)
![Tests](https://img.shields.io/badge/Tests-12%20Passed-brightgreen?style=flat-square)

---

## What is ReSpawn?

ReSpawn is a native Windows desktop application built for PC gamers tired of fragmented playtime across multiple launchers. Add any game by selecting its `.exe`, dropping a shortcut, or dragging a file — ReSpawn silently tracks your playtime in the background, regardless of how or where you launch it.

No accounts. No internet. No bloat. Just your games and your time.

**Built for:**
- Gamers juggling Steam, Epic, Riot, Xbox App, or custom shortcuts
- Players who want accurate, unified playtime in one place
- Privacy-conscious users who want zero data sent anywhere
- Minimalists who want a fast launcher without the overhead

---

## Table of Contents

- [Features](#features)
- [Installation](#installation)
- [How It Works](#how-it-works)
- [Tech Stack](#tech-stack)
- [File Structure](#file-structure)
- [Author](#author)

---

## Features

### 🎮 Game Library
- Add games via `.exe` file picker, drag & drop, or `.lnk` / `.url` shortcuts
- Auto icon extraction from executables
- Custom icon support — set any image as cover art
- Edit game name, path, and icon anytime
- Remove games with confirmation
- Duplicate process name detection with warning
- Real-time search bar across your entire library
- Total games counter in header

### ⏱️ Playtime Tracking
- Background process monitor with 1-second polling
- Tracks sessions regardless of which launcher opened the game
- 60-second minimum session threshold — ignores accidental launches
- 12-hour session cap — protects against hibernate/sleep corruption
- Orphaned session cleanup on every startup
- Balloon notification when a session is saved

### 🖥️ System Tray
- Window closes to system tray — tracker keeps running silently
- Tray context menu: Open ReSpawn / Exit
- Double-click tray icon to restore window
- Auto-minimizes to tray when a game launches
- Power button in header for immediate full exit
- Optional "Run at Startup" setting to automatically launch ReSpawn with Windows

### 🎨 UI & UX
- Clean dark interface with modern card layout
- 🟢 Green dot = Game is currently running
- 🔴 Red dot = Executable not found at saved path
- Last played shown as relative time — Today, Yesterday, 3 days ago
- Enter key to confirm Add Game dialog
- F5 keyboard shortcut to refresh library
- Dynamic Add Game button — grey when inactive, red when ready
- Play button overlay on game cards for faster game launching
- Hover-based quick launch interaction for improved usability

### 🔒 Privacy & Security
- 100% offline — zero network requests, ever
- No telemetry, analytics, or crash reporting
- All data stored locally in `%AppData%\ReSpawn\`
- No administrator privileges required
- Single instance enforcement — prevents duplicate processes

---

## Installation

1. Download `ReSpawn.exe` from [**Latest Release**](https://github.com/harshitsinghal11/ReSpawn/releases)
2. Double-click to run — no installation or setup required
3. No .NET runtime needed — fully self-contained single executable

> **Requirements:** Windows 10 or Windows 11 (x64)

---

## How It Works

ReSpawn monitors the Windows process list every second. The moment a registered game's process appears — whether launched from ReSpawn, Steam, Epic, a desktop shortcut, or anywhere else — the tracker starts timing. When the process closes, the session is automatically saved with its full timestamp and duration.

```
Game Launched (any source)
        ↓
ProcessMonitor detects process name
        ↓
Session timer starts → UI updates live
        ↓
Process closes → session saved to games.json
        ↓
Playtime updated → balloon notification shown
```

All data lives in `%AppData%\ReSpawn\games.json` with atomic writes and automatic backups to prevent corruption.

---

## Tech Stack

| Layer | Technology | Purpose |
|---|---|---|
| Language | C# 12 | Type-safe, performant, first-class Windows tooling |
| Runtime | .NET 8 LTS | Self-contained publish, long-term support |
| UI Framework | WPF + MVVM | Native Windows rendering, data-bound UI |
| Storage | System.Text.Json | Zero-dependency local JSON persistence |
| System Tray | Hardcodet.NotifyIcon.Wpf 2.0.1 | XAML-integrated tray icon |
| Icon Extraction | System.Drawing.Icon | Windows-native `.exe` icon extraction |
| Process Monitoring | System.Diagnostics.Process | Direct Windows process list access |
| Testing | xUnit + Moq | Unit tests for core services |

---

## File Structure

```
ReSpawn/
│
├── Assets/              # Icons and images
├── Models/              # Game.cs, GameSession.cs
├── Services/            # GameService, ProcessMonitor, IconExtractor
├── ViewModels/          # MainViewModel, GameTileViewModel, Add/EditGameViewModel
├── Views/               # MainWindow, HomeView, AddGameDialog, EditGameDialog
├── Controls/            # GameTileControl
└── Helpers/             # AppDataHelper, TimeFormatter, RelayCommand,
                         # AtomicFileWriter, PathValidator, ShortcutResolver

ReSpawn.Tests/
├── GameServiceTests.cs
└── TimeFormatterTests.cs
```

### Runtime Data — `%AppData%\ReSpawn\`

```
%AppData%\ReSpawn\
├── games.json          # Main data file
├── games.json.bak      # Auto-backup
└── icons\              # Extracted game icons
    └── {game-id}.png
```

---

## Author

**Harshit Singhal**

Built with ❤️ for gamers who just want to play, not manage launchers.

---

*ReSpawn — Track everything. Launch anything.*
