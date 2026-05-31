# ReSpawn

> A lightweight, offline-first Windows desktop application that unifies your game library and automatically tracks playtime across every launcher — silently, natively, and without needing the internet.

![Platform](https://img.shields.io/badge/Platform-Windows%2010%2F11-0078D4?style=flat-square&logo=windows)
![Stack](https://img.shields.io/badge/Stack-C%23%20%C2%B7%20.NET%208%20%C2%B7%20WPF-512BD4?style=flat-square&logo=dotnet)
![Storage](https://img.shields.io/badge/Storage-Local%20JSON-green?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-yellow?style=flat-square)

---

## Current Version

| Field | Value |
|---|---|
| Version | `0.1.0-alpha` (Pre-Development) |
| Stage | Planning / Architecture |
| Target Platform | Windows 10 / Windows 11 (x64) |
| Runtime | .NET 8 Self-Contained |
| Last Updated | May 31, 2026 |

---

## About / Summary

**ReSpawn** is a fast, native Windows desktop application that solves a problem every multi-platform gamer faces: playtime scattered across Steam, Epic Games, Riot Client, Xbox App, and games launched directly from shortcuts — with no single place to see it all.

ReSpawn works in two modes simultaneously:

- **Game Launcher** — A clean, minimal home screen where users register games by selecting their `.exe` file. From there, any game can be launched with a single click, regardless of what platform it belongs to.

- **Background Playtime Tracker** — A silent background service that monitors Windows processes every 5 seconds. The moment a registered game's process appears — whether launched from ReSpawn, Steam, Epic, or a desktop shortcut — the tracker starts a session timer. When the process closes, the session is saved automatically.

Because ReSpawn tracks by **process name** rather than launch source, it is completely platform-agnostic. It requires no API keys, no accounts, no internet connection, and no integration with any third-party service.

All data lives locally in a single `games.json` file stored in `%AppData%\ReSpawn\`. The application is designed to start with Windows, run invisibly in the system tray, and consume fewer than 25 MB of memory at idle.

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
4. [Functional Requirements](#functional-requirements)
5. [Non-Functional Requirements](#non-functional-requirements)
6. [Tech Stack](#tech-stack)
7. [File Structure](#file-structure)
8. [Future Plans](#future-plans)

---

## Problem Statement

Modern PC gamers are fragmented across at least 3–4 launchers. Steam, Epic Games, Riot Client, and Xbox App each track playtime only for games launched through themselves. Any game opened from a desktop shortcut, a custom install path, or a launcher like ReSpawn is completely invisible to all of them.

This creates four compounding problems:

**1. Fragmented playtime.**
A player with 200 hours in PUBG (Steam), 150 hours in Fortnite (Epic), and 80 hours in Valorant (Riot) has no single number showing 430 total hours of gaming. Each platform shows its own isolated slice.

**2. Invisible sessions.**
Games launched outside their native launcher — via desktop shortcut, file manager, or any third-party tool — are never tracked. The hours simply disappear.

**3. Bloated launchers running in background.**
Steam, Epic, Riot, and Xbox App all run persistent background services that consume RAM and CPU even when the user isn't gaming. These services exist primarily for store and social features, not just launching games.

**4. No unified library.**
To see all owned games, a user must open multiple apps. There is no single interface that shows the full collection with unified statistics.

**ReSpawn solves all four** by acting as a lightweight, always-on background monitor that is completely independent of any launcher. It doesn't care how a game was launched — it watches the Windows process list and records playtime the moment a registered process is alive.

---

## Functional Requirements

### FR-01 — Game Registration

- The user can add any game by selecting its `.exe` file through a standard Windows file picker dialog (filtered to `*.exe`).
- Upon selecting the file, the application automatically extracts:
  - **Game Name** — derived from the filename, user-editable before saving.
  - **Process Name** — the filename without the `.exe` extension, user-editable for games that spawn a secondary process.
  - **Executable Path** — the full absolute path, stored as-is.
  - **Icon** — extracted from the `.exe` using the Windows icon API and cached locally as a `.png`.
- The user can review and edit all extracted fields in a confirmation dialog before the game is saved.

### FR-02 — Game Library Display

- All registered games are displayed on a home screen as a responsive grid of tiles.
- Each tile shows:
  - Game icon (falls back to a default placeholder if extraction failed or file is missing).
  - Game name.
  - Total playtime formatted as `Xh Ym` (e.g., `12h 30m`).
  - Last played date as a relative string (`Today`, `Yesterday`, `3 days ago`, or a full date).
  - A pulsing green **"Now Playing"** badge while the game's process is detected as running.
- An empty state screen is shown when no games are registered.

### FR-03 — Game Launch

- Any game in the library can be launched by double-clicking its tile.
- Launch is executed via `Process.Start(exePath)`.
- If the `.exe` file no longer exists at the stored path, a clear error message is shown with an option to re-link the executable.

### FR-04 — Game Management

- The user can **edit** a game's name, process name, and icon via a right-click context menu.
- The user can **remove** a game from the library via the context menu, with a confirmation dialog.
- Removing a game deletes all its session history and its cached icon file.

### FR-05 — Automatic Process Monitoring

- A background thread polls `Process.GetProcesses()` every **5 seconds** (configurable).
- Each registered game's `processName` is matched against the live process list (case-insensitive).
- If a match is found and the game was not already marked as playing:
  - Mark `isPlaying = true`.
  - Record `sessionStart = DateTime.UtcNow`.
- If a previously matched process is no longer found:
  - Calculate `durationSeconds = (DateTime.UtcNow - sessionStart).TotalSeconds`.
  - If duration is **less than 60 seconds**, discard the session silently.
  - If duration is **60 seconds or more**, save the session and update totals.

### FR-06 — Session Recording

- Each saved session contains: a UUID, UTC start timestamp, UTC end timestamp, and duration in seconds.
- Sessions are appended to the game's `sessions[]` array in `games.json`.
- `totalPlaytimeSeconds` is incremented by the session duration.
- `lastPlayed` is updated to the session end timestamp.
- `isPlaying` is set back to `false`.

### FR-07 — Crash Recovery

- On every application startup, the service scans for games where `isPlaying == true`.
- These represent sessions that were never closed due to a crash or force-quit.
- Orphaned sessions are discarded (not saved) and `isPlaying` is reset to `false`.
- The user is not interrupted — cleanup is silent.

### FR-08 — System Tray Operation

- Closing the main window minimizes the application to the system tray instead of quitting.
- The tray icon provides a context menu with:
  - **Open ReSpawn** — restores the main window.
  - **Now Playing: [Game Name]** or **No game running** — dynamic live status.
  - **Exit** — fully quits the application and stops the background tracker.
- Double-clicking the tray icon restores the main window.
- The background tracker continues running while the window is hidden.

### FR-09 — Data Persistence

- All game and session data is stored in `%AppData%\ReSpawn\games.json`.
- Writes are atomic: the app writes to a temp file and then swaps it in place to prevent data corruption on crash mid-save.
- If `games.json` is missing on startup, it is auto-created with an empty game list (new install case).
- If `games.json` is corrupt, the bad file is backed up as `games.json.bak` and a fresh empty file is created.

### FR-10 — Playtime Formatting

- Total playtime is always displayed in `Xh Ym` format (e.g., `0h 05m`, `1h 00m`, `45h 30m`).
- Last played is displayed as a relative string at short distances and an absolute date beyond 7 days.
- All internal timestamps are UTC; display converts to local time.

---

## Non-Functional Requirements

### Performance

- Application startup time must be **under 1 second** from launch to interactive home screen on standard hardware (SSD, 8GB RAM).
- Memory usage at idle (window minimized to tray, tracker running) must stay **below 30 MB**.
- The process monitor poll cycle must complete in **under 50ms** to avoid UI thread interference.
- Writing to `games.json` must complete in **under 10ms** to not block the background thread.
- UI frame rate must remain at 60fps during all animations (tile hover, badge pulse, window open/close).

### Reliability

- Session data must never be lost due to normal app closure, OS shutdown, or application crash.
- The atomic write pattern (temp file swap) must be used for every `games.json` save operation.
- Orphaned session cleanup must run on every startup without exception.
- The background tracker must self-recover from transient `Process.GetProcesses()` failures (e.g., a single failed poll should be retried on the next cycle, not crash the app).
- The application must handle `games.json` access failures (file locked by antivirus, permission denied) gracefully without crashing.

### Usability

- The Add Game flow must take no more than **3 user interactions** to complete (open dialog → select file → confirm).
- All primary actions (add, launch, edit, remove) must be reachable within **2 clicks** from the home screen.
- Error states (file not found, icon extraction failed, JSON corrupt) must surface clear, plain-language messages — never raw exception text.
- The UI must be fully operable with a mouse only; keyboard navigation is a V2 enhancement.
- Playtime and last played information must be readable at a glance without tooltips.

### Security & Privacy

- No network requests are made at any time — the application is fully offline.
- No telemetry, analytics, or crash reporting is sent anywhere.
- All data remains on the user's local machine under their own `%AppData%` folder.
- The application does not require administrator privileges to run.
- The application does not modify any game files or registry entries beyond its own app data.

### Maintainability

- The codebase must follow the MVVM pattern strictly — no business logic in code-behind files.
- All configurable values (poll interval, minimum session seconds, max session cap) must live in a single `Constants.cs` file.
- `GameService` must be the single point of truth for all `games.json` read/write operations.
- `ProcessMonitor` must fire events rather than directly mutating UI state — the ViewModel subscribes.
- Every public method in `GameService` and `ProcessMonitor` must have an XML doc comment.

### Scalability (Local)

- The application must perform without degradation with up to **500 registered games**.
- The application must perform without degradation with up to **10,000 session records** per game.
- `games.json` deserialization must remain under 100ms even at 500 games × 1,000 sessions each.

### Compatibility

- Must run on **Windows 10 (1903+)** and **Windows 11** — both x64 only.
- Must run as a standard user account (no UAC elevation required).
- Must support both HiDPI (125%, 150%, 200%) and standard (100%) display scaling.
- Published as a self-contained `.exe` — no .NET runtime installation required from the user.

---

## Tech Stack

| Layer | Technology | Version | Reasoning |
|---|---|---|---|
| **Language** | C# | 12 | Type-safe, performant, first-class Windows tooling support |
| **Runtime** | .NET | 8 LTS | Long-term support, self-contained publish, minimal footprint |
| **UI Framework** | WPF (Windows Presentation Foundation) | .NET 8 | Native Windows rendering, GPU-accelerated, XAML-based, ideal for data-bound UIs |
| **UI Pattern** | MVVM | — | Clean separation of UI and logic; enables live data binding without code-behind coupling |
| **Process Monitoring** | `System.Diagnostics.Process` | Built-in | Direct Windows API access; `Process.GetProcesses()` is the most reliable cross-game method |
| **Icon Extraction** | `System.Drawing.Icon` | Built-in | `Icon.ExtractAssociatedIcon()` is the standard Windows API for reading `.exe` embedded icons |
| **JSON Storage** | `System.Text.Json` | Built-in | Zero-dependency, high-performance JSON serialization built into .NET 8 |
| **System Tray** | `Hardcodet.NotifyIcon.Wpf` | 1.1.0 | The standard WPF tray icon library; well-maintained, XAML-integrated |
| **Background Threading** | `System.Threading.Timer` | Built-in | Non-UI thread timer; polling on a background thread keeps the UI thread completely free |
| **Unique IDs** | `System.Guid` | Built-in | `Guid.NewGuid()` for all game and session identifiers — no collisions, no sequences |
| **Build / Publish** | `dotnet publish` | .NET 8 CLI | Single-file self-contained publish (`--self-contained -p:PublishSingleFile=true`) |
| **Installer** | NSIS (Nullsoft Scriptable Install System) | 3.x | Lightweight, widely trusted Windows installer generator; produces a standard `.exe` setup file |
| **IDE** | Visual Studio 2022 | 17.x | Full WPF designer, XAML IntelliSense, integrated debugger |
| **Version Control** | Git + GitHub | — | Standard; enables releases, issue tracking, and changelog management |
| **Testing** | xUnit + Moq | Latest | Unit testing for `GameService` and `ProcessMonitor` logic in isolation |

### Why WPF over Electron / React

| Metric | WPF + C# | Electron + React |
|---|---|---|
| Idle memory usage | ~20 MB | ~150 MB |
| Cold startup time | < 0.5 seconds | 2–4 seconds |
| Windows process API | Direct, native | Via Node.js child_process shim |
| Icon extraction | Built into .NET | Requires npm package |
| Published build size | ~50 MB (self-contained) | ~200 MB+ |
| UI rendering | Native GPU, 60fps | Chromium-based renderer |
| Offline capability | Native, no runtime needed | Bundled Chromium always present |

---

## File Structure

```
ReSpawn/
│
├── ReSpawn.sln                          ← Visual Studio solution file
│
└── ReSpawn/                             ← Main WPF project
    │
    ├── ReSpawn.csproj                   ← Project file (.NET 8, OutputType=WinExe)
    │
    ├── App.xaml                         ← Application entry point, global resources, tray icon
    ├── App.xaml.cs                      ← Startup logic, shutdown handling, DI bootstrap
    │
    ├── Constants.cs                     ← All magic numbers: PollIntervalMs, MinSessionSeconds,
    │                                       MaxSessionHours, AppDataFolderName
    │
    ├── Assets/
    │   ├── ReSpawn.ico                  ← Application window icon
    │   ├── tray-icon.ico                ← System tray icon (16x16, 32x32 multi-res)
    │   └── default-icon.png             ← Fallback game icon when extraction fails
    │
    ├── Models/
    │   ├── Game.cs                      ← Game data model (maps 1:1 to JSON schema)
    │   └── GameSession.cs               ← Session data model (id, start, end, durationSeconds)
    │
    ├── Services/
    │   ├── GameService.cs               ← Single source of truth for games.json CRUD
    │   │                                   LoadGames(), SaveGames(), AddGame(),
    │   │                                   UpdateGame(), RemoveGame(), AppendSession()
    │   │
    │   ├── ProcessMonitor.cs            ← Background polling service
    │   │                                   Raises GameStarted / GameStopped events
    │   │                                   Handles orphan cleanup on startup
    │   │
    │   └── IconExtractor.cs             ← Extracts .exe icon via Icon.ExtractAssociatedIcon()
    │                                       Saves to %AppData%\ReSpawn\icons\{gameId}.png
    │                                       Returns fallback path on any failure
    │
    ├── ViewModels/
    │   ├── MainViewModel.cs             ← Drives HomeView: game list, add/remove commands
    │   ├── GameTileViewModel.cs         ← Per-tile: name, icon, playtime, isPlaying binding
    │   ├── AddGameViewModel.cs          ← Add Game dialog: file selection, preview, validation
    │   └── EditGameViewModel.cs         ← Edit Game dialog: pre-populated, save/cancel
    │
    ├── Views/
    │   ├── MainWindow.xaml              ← Custom frameless shell window with drag region
    │   ├── MainWindow.xaml.cs           ← Close → minimize to tray override
    │   ├── HomeView.xaml                ← WrapPanel game grid, empty state, header
    │   ├── HomeView.xaml.cs
    │   ├── AddGameDialog.xaml           ← Modal dialog for adding a game
    │   ├── AddGameDialog.xaml.cs
    │   ├── EditGameDialog.xaml          ← Modal dialog for editing a game
    │   └── EditGameDialog.xaml.cs
    │
    ├── Controls/
    │   ├── GameTileControl.xaml         ← Reusable UserControl: icon, name, playtime, badge
    │   └── GameTileControl.xaml.cs
    │
    ├── Helpers/
    │   ├── TimeFormatter.cs             ← FormatPlaytime(long seconds) → "12h 30m"
    │   │                                   FormatLastPlayed(DateTime?) → "Today", "3 days ago"
    │   ├── RelayCommand.cs              ← ICommand implementation for MVVM bindings
    │   ├── PathValidator.cs             ← Checks if exePath still exists on disk
    │   └── AtomicFileWriter.cs          ← Write temp → rename pattern for safe JSON saves
    │
    └── Tests/                           ← Separate xUnit test project (ReSpawn.Tests)
        ├── GameServiceTests.cs
        ├── ProcessMonitorTests.cs
        ├── TimeFormatterTests.cs
        └── TestData/
            ├── valid_games.json
            └── corrupt_games.json
```

### AppData Folder — Runtime (auto-created on first launch)

```
%AppData%\ReSpawn\
    ├── games.json                       ← Live local database
    ├── games.json.bak                   ← Auto-backup of last known good state before corruption recovery
    └── icons\
        ├── {game-uuid-1}.png            ← Cached icon per game, named by game UUID
        ├── {game-uuid-2}.png
        └── ...
```

---


---

## Future Plans

### Version 2 — Library & UX Enhancements

- **Search bar** — real-time filter across game names; clears with Escape
- **Sort options** — Most Played, Last Played, Alphabetical (A-Z / Z-A), Recently Added
- **Favorites / Pinning** — pin games to the top of the grid; persisted in `games.json` via `isFavorite: bool`
- **Custom cover art** — allow users to replace the extracted icon with any image file (JPEG, PNG, WebP); resize and crop to 2:3 portrait aspect for a "game shelf" aesthetic
- **Bulk import** — select a folder; ReSpawn scans recursively for `.exe` files and presents a checklist of found games to add in one action
- **Keyboard navigation** — arrow keys to move between tiles, Enter to launch, Delete to remove
- **Personal notes** — per-game text notes stored in `games.json`; accessible via right-click

### Version 3 — Stats Dashboard & Platform Integrations

- **Statistics view** — dedicated screen with:
  - Daily / weekly / monthly bar charts (using `LiveCharts2` for WPF)
  - Top 5 most-played games of the week
  - Total gaming hours this month vs last month
  - Session length distribution (histogram)
- **Weekly summary report** — auto-generated PDF or CSV: `Week of [date] — X hours across Y games`
- **Steam library auto-import** — read Steam's `libraryfolders.vdf` to discover installed games; import with one click
- **Epic Games detection** — parse `%ProgramData%\Epic\EpicGamesLauncher\Data\Manifests\` for installed game manifests
- **Riot Games detection** — detect Valorant and League of Legends via known install paths and process names
- **Xbox / Game Pass detection** — scan `%ProgramFiles%\WindowsApps\` for installed Game Pass titles

### Version 4 — Personalization & Power Features

- **Theme engine** — Dark / Light mode toggle; custom accent color picker; saved to user preferences JSON
- **Achievement system** — local milestone badges: "10 Hours Played", "Night Owl" (played past 2am), "Century" (100h in one game)
- **Goals & reminders** — set a daily or weekly playtime limit per game; desktop notification when approaching the limit
- **Session notes** — optional per-session journal entry: "Finally beat the final boss"; shown in a session history detail view
- **Export data** — full `games.json` export as CSV or plain text summary for sharing or backup
- **Multi-monitor awareness** — remember which monitor the window was last on; restore to the same monitor

### Long-Term Scaling Opportunities

- **SQLite migration** — replace `games.json` with a local SQLite database (`Microsoft.Data.Sqlite`) once session count grows beyond ~50,000 total records; `games.json` becomes the export format
- **WinUI 3 migration** — migrate from WPF to WinUI 3 for Fluent Design, Mica/Acrylic material backgrounds, and Windows 11 native aesthetic while keeping the same MVVM architecture
- **Optional cloud backup** — encrypted, opt-in backup of `games.json` to a user-chosen destination (OneDrive folder, local network share); no server-side component
- **Plugin / extension API** — expose a local named-pipe or file-based interface so community tools can read session data without modifying the app directly

---

*ReSpawn — Built for gamers who just want to play, not manage launchers.*
