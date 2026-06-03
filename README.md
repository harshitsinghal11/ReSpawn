# ReSpawn

> A lightweight, offline-first Windows desktop application that unifies your game library and automatically tracks playtime across every launcher — silently, natively, and without needing the internet.

![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D4?style=flat-square&logo=windows)
![Stack](https://img.shields.io/badge/Stack-C%23%20%C2%B7%20.NET%208%20%C2%B7%20WPF-512BD4?style=flat-square&logo=dotnet)
![Storage](https://img.shields.io/badge/Storage-Local%20JSON-green?style=flat-square)
![Version](https://img.shields.io/badge/Version-0.2.0-blue?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-yellow?style=flat-square)

---

## Current Version

| Field | Value |
|---|---|
| Version | `1.0.0` |
| Stage | Active Development |
| Target Platform | Windows 10 / Windows 11 (x64) |
| Runtime | .NET 8 Self-Contained |
| Last Updated | June 2026 |

---

## About / Summary

**ReSpawn** is a fast, native Windows desktop application built for PC gamers who are tired of fragmented playtime across multiple launchers. Whether you play on Steam, Epic Games, Riot Client, Xbox App, or directly from a desktop shortcut — ReSpawn tracks it all from one place.

ReSpawn works in two modes simultaneously:

- **Game Launcher** — A clean, minimal home screen where users register games by selecting their `.exe` file. Games can be launched with a double-click, regardless of which platform they belong to. Icons are automatically extracted from the executable and displayed on each tile.

- **Background Playtime Tracker** — A silent background service that monitors Windows processes every second. The moment a registered game's process appears — whether launched from ReSpawn, Steam, Epic, or a desktop shortcut — the tracker starts a session timer. When the process closes, the session is saved automatically with full timestamp and duration data.

Because ReSpawn tracks by **process name** rather than launch source, it is completely platform-agnostic. It requires no API keys, no accounts, no internet connection, and no integration with any third-party service.

All data lives locally in a single `games.json` file stored in `%AppData%\ReSpawn\`. The application runs invisibly in the system tray, consuming under 30 MB of memory at idle.

**Target Users:**
- PC gamers who use more than one game launcher
- Players who want accurate, unified playtime without trusting Steam's numbers
- Privacy-conscious users who don't want their play habits sent to any server
- Minimalist users who want a fast launcher without the overhead of Steam or Epic

---

## Table of Contents

1. [Current Version](#current-version)
2. [About / Summary](#about--summary)
3. [Problem Statement](#problem-statement)
4. [Core Features](#core-features)
5. [Tech Stack](#tech-stack)
6. [File Structure](#file-structure)
7. [Future Plans](#future-plans)

---

## Problem Statement

Modern PC gamers are fragmented across at least 3–4 launchers. Steam, Epic Games, Riot Client, and Xbox App each track playtime only for games launched through themselves. Any game opened from a desktop shortcut, a custom install path, or a launcher like ReSpawn is completely invisible to all of them.

This creates four compounding problems:

**1. Fragmented playtime.**
A player with 200 hours in a Steam game, 150 hours in an Epic game, and 80 hours in a Riot game has no single number showing 430 total hours of gaming. Each platform shows its own isolated slice.

**2. Invisible sessions.**
Games launched outside their native launcher — via desktop shortcut, file manager, or any third-party tool — are never tracked. The hours simply disappear.

**3. Bloated launchers running in background.**
Steam, Epic, Riot, and Xbox App all run persistent background services that consume RAM and CPU even when the user isn't gaming. These services exist primarily for store and social features, not just launching games.

**4. No unified library.**
To see all owned games, a user must open multiple apps. There is no single interface that shows the full collection with unified statistics.

**ReSpawn solves all four** by acting as a lightweight, always-on background monitor that is completely independent of any launcher. It doesn't care how a game was launched — it watches the Windows process list and records playtime the moment a registered process is alive.

---

## Core Features

### 🎮 Game Management
- Add games via `.exe` file picker with auto-fill of name and process name
- Drag & Drop `.exe` files directly onto the home screen to add instantly
- Edit game name, executable path, and custom icon
- Remove game with confirmation dialog
- Duplicate process name detection with warning
- File Not Found warning badge on broken tiles

### 📚 Library View
- Responsive game grid with icon tiles
- Auto icon extraction from `.exe` files with fallback placeholder
- Custom icon support — set any PNG/JPG as game icon
- Playtime display in `Xh Ym` format
- Last played date shown as relative string (`Today`, `Yesterday`, `3 days ago`)
- Total Games counter in header
- Real-time search bar filtering game library
- Empty state screen when no games added

### ⏱️ Playtime Tracking
- Background process monitor polling every 1 second
- Real-time green dot indicator when game is running
- Sessions saved automatically when game closes
- 60 second minimum session threshold — short accidental launches ignored
- Maximum session cap of 12 hours — protects against hibernate/sleep corruption
- Orphaned session cleanup on every startup
- Session saved balloon notification after each tracked session

### 🖥️ System Tray
- Closing window hides to system tray — tracker keeps running
- One-time balloon tip on first minimize
- Tray context menu: Open ReSpawn / Exit
- Double-click tray icon to restore window
- Power button in header for full immediate exit

### ⌨️ UX & Keyboard
- Double-click tile to launch game
- Right-click context menu: Launch, Edit, Remove
- F5 to refresh library
- Enter key to confirm Add Game dialog
- Dynamic Add Game button — grey when disabled, red when ready

### 🔒 Privacy & Security
- Fully offline — zero network requests ever
- No telemetry, analytics, or crash reporting
- All data stored locally in `%AppData%\ReSpawn\`
- No administrator privileges required

---

## Tech Stack

| Layer | Technology | Version | Reasoning |
|---|---|---|---|
| **Language** | C# | 12 | Type-safe, performant, first-class Windows tooling |
| **Runtime** | .NET | 8 LTS | Long-term support, self-contained publish, minimal footprint |
| **UI Framework** | WPF | .NET 8 | Native Windows rendering, XAML-based, ideal for data-bound UIs |
| **UI Pattern** | MVVM | — | Clean separation of UI and logic; live data binding |
| **Process Monitoring** | `System.Diagnostics.Process` | Built-in | Direct Windows API; most reliable cross-game method |
| **Icon Extraction** | `System.Drawing.Icon` | Built-in | `Icon.ExtractAssociatedIcon()` — standard Windows icon API |
| **JSON Storage** | `System.Text.Json` | Built-in | Zero-dependency, high-performance serialization in .NET 8 |
| **System Tray** | `Hardcodet.NotifyIcon.Wpf` | 2.0.1 | Standard WPF tray icon library, XAML-integrated |
| **Background Threading** | `System.Threading.Timer` | Built-in | Non-UI thread timer; keeps UI thread completely free |
| **Unique IDs** | `System.Guid` | Built-in | `Guid.NewGuid()` for all game and session identifiers |
| **Build / Publish** | `dotnet publish` | .NET 8 CLI | Single-file self-contained publish |
| **Testing** | xUnit + Moq | Latest | Unit testing for GameService and ProcessMonitor logic |
| **IDE** | Visual Studio 2026 | Community | Full WPF designer, XAML IntelliSense, integrated debugger |
| **Version Control** | Git + GitHub | — | Standard; enables releases and changelog management |

---

## File Structure

```
ReSpawn/
│
├── ReSpawn.sln
│
└── ReSpawn/
    │
    ├── ReSpawn.csproj
    ├── App.xaml
    ├── App.xaml.cs
    ├── Constants.cs
    │
    ├── Assets/
    │   ├── respawn.ico
    │   ├── tray-icon.ico
    │   └── default-icon.png
    │
    ├── Models/
    │   ├── Game.cs
    │   └── GameSession.cs
    │
    ├── Services/
    │   ├── GameService.cs
    │   ├── ProcessMonitor.cs
    │   └── IconExtractor.cs
    │
    ├── ViewModels/
    │   ├── MainViewModel.cs
    │   ├── GameTileViewModel.cs
    │   ├── AddGameViewModel.cs
    │   └── EditGameViewModel.cs
    │
    ├── Views/
    │   ├── MainWindow.xaml
    │   ├── MainWindow.xaml.cs
    │   ├── HomeView.xaml
    │   ├── HomeView.xaml.cs
    │   ├── AddGameDialog.xaml
    │   ├── AddGameDialog.xaml.cs
    │   ├── EditGameDialog.xaml
    │   └── EditGameDialog.xaml.cs
    │
    ├── Controls/
    │   ├── GameTileControl.xaml
    │   └── GameTileControl.xaml.cs
    │
    └── Helpers/
        ├── AppDataHelper.cs
        ├── AtomicFileWriter.cs
        ├── PathValidator.cs
        ├── RelayCommand.cs
        ├── StringToImageSourceConverter.cs
        └── TimeFormatter.cs

ReSpawn.Tests/
    ├── GameServiceTests.cs
    ├── TimeFormatterTests.cs
    └── TestData/
```

### AppData Folder — Runtime

```
%AppData%\ReSpawn\
    ├── games.json
    ├── games.json.bak
    └── icons\
        ├── {game-uuid-1}.png
        ├── {game-uuid-2}.png
        └── ...
```

---

## Installation

1. Download `ReSpawn.exe` from the [Latest Release](https://github.com/harshitsinghal11/ReSpawn/releases)
2. Double-click to run — no installation required
3. No .NET runtime needed — fully self-contained

---

## Author

**Harshit Singhal**
Built with ❤️ for gamers who just want to play, not manage launchers.

---

*ReSpawn — Track everything. Launch anything.*
