# ReSpawn

> A lightweight, offline-first Windows app that unifies your game library and automatically tracks playtime — silently, natively, no internet required.

![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D4?style=flat-square&logo=windows)
![Stack](https://img.shields.io/badge/Stack-C%23%20%C2%B7%20.NET%208%20%C2%B7%20WPF-512BD4?style=flat-square&logo=dotnet)
![Version](https://img.shields.io/badge/Version-1.0.0-blue?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-yellow?style=flat-square)

---

## What is ReSpawn?

ReSpawn is a native Windows desktop app for PC gamers tired of fragmented playtime across multiple launchers. Add any game by selecting its `.exe` — ReSpawn tracks your playtime automatically in the background, regardless of how or where you launch it.

**Target Users:**
- Gamers using Steam, Epic, Riot, Xbox App, or direct shortcuts
- Players who want accurate unified playtime in one place
- Privacy-conscious users who don't want data sent to any server

---

## Features

### 🎮 Game Library
- Add games via `.exe` picker or drag & drop
- Auto icon extraction from executable
- Custom icon support
- Edit or remove games anytime
- Real-time search across your library

### ⏱️ Playtime Tracking
- Background process monitor — 1 second polling
- Tracks playtime regardless of how the game was launched
- 60s minimum session threshold
- 12-hour session cap (hibernate protection)
- Session saved balloon notification

### 🖥️ System Tray
- Closes to tray — tracker keeps running
- Tray menu: Open / Exit
- Power button for immediate full exit

### 🎨 UI
- Dark modern interface
- Green dot = Now Playing
- Red dot = File Not Found
- Total games counter + last played dates

### 🔒 Privacy
- 100% offline — zero network requests
- All data stored locally in `%AppData%\ReSpawn\`
- No telemetry, no accounts, no API keys

---

## Tech Stack

| Layer | Technology |
|---|---|
| Language | C# 12 |
| Runtime | .NET 8 LTS |
| UI Framework | WPF (MVVM) |
| Storage | Local JSON via System.Text.Json |
| System Tray | Hardcodet.NotifyIcon.Wpf 2.0.1 |
| Icon Extraction | System.Drawing.Icon |
| Testing | xUnit + Moq |

---

## Installation

1. Download `ReSpawn.exe` from [Releases](https://github.com/harshitsinghal11/ReSpawn/releases)
2. Double-click to run — no installation required
3. No .NET runtime needed — fully self-contained

---

## File Structure

```
ReSpawn/
├── Models/          # Game, GameSession
├── Services/        # GameService, ProcessMonitor, IconExtractor
├── ViewModels/      # MainViewModel, GameTileViewModel, Add/EditGameViewModel
├── Views/           # MainWindow, HomeView, Add/EditGameDialog
├── Controls/        # GameTileControl
└── Helpers/         # AppDataHelper, TimeFormatter, RelayCommand, etc.

ReSpawn.Tests/
├── GameServiceTests.cs
└── TimeFormatterTests.cs
```

---

## Author

**Harshit Singhal** — Built for gamers who just want to play, not manage launchers.

*ReSpawn — Track everything. Launch anything.*
