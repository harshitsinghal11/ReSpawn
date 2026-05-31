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
8. [Development Phases](#development-phases)
   - [Phase 1 — Project Setup](#phase-1---project-setup)
   - [Phase 2 — Data Layer](#phase-2---data-layer)
   - [Phase 3 — Game Launcher UI](#phase-3---game-launcher-ui)
   - [Phase 4 — Background Process Tracker](#phase-4---background-process-tracker)
   - [Phase 5 — Icon Extraction Service](#phase-5---icon-extraction-service)
   - [Phase 6 — System Tray Integration](#phase-6---system-tray-integration)
   - [Phase 7 — Polish, Edge Cases & Release](#phase-7---polish-edge-cases--release)
9. [Future Plans](#future-plans)

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

## Development Phases

---

### Phase 1 - Project Setup

> Goal: A working, runnable WPF shell with no features — just the skeleton, configuration, and folder structure committed and verified.

**Environment & Tooling**

- [ ] Install Visual Studio 2022 (Community or higher) with the `.NET desktop development` workload
- [ ] Install .NET 8 SDK — verify with `dotnet --version`
- [ ] Install Git — configure user name and email
- [ ] Create GitHub repository: `ReSpawn` (public or private)
- [ ] Add `.gitignore` for Visual Studio / C# projects (`github/gitignore` → VisualStudio)
- [ ] Add `MIT LICENSE` file to repo root

**Solution & Project Bootstrap**

- [ ] Create new Visual Studio solution: `ReSpawn.sln`
- [ ] Add WPF App project: `ReSpawn`, target `net8.0-windows`, `OutputType = WinExe`
- [ ] Add xUnit test project: `ReSpawn.Tests`, target `net8.0`
- [ ] Add project reference: `ReSpawn.Tests` → `ReSpawn`
- [ ] Install NuGet package: `Hardcodet.NotifyIcon.Wpf` (1.1.0) into `ReSpawn`
- [ ] Install NuGet packages into `ReSpawn.Tests`: `xunit`, `xunit.runner.visualstudio`, `Moq`

**Folder Structure**

- [ ] Create all folders from the File Structure section: `Assets/`, `Models/`, `Services/`, `ViewModels/`, `Views/`, `Controls/`, `Helpers/`, `Tests/TestData/`
- [ ] Add placeholder `.gitkeep` files in empty folders so they commit to Git

**Constants & Configuration**

- [ ] Create `Constants.cs` with initial values:
  ```csharp
  public static class Constants
  {
      public const int    PollIntervalMs    = 5000;
      public const int    MinSessionSeconds = 60;
      public const int    MaxSessionHours   = 12;
      public const string AppDataFolder     = "ReSpawn";
      public const string GamesFileName     = "games.json";
      public const string IconsFolder       = "icons";
  }
  ```

**Assets**

- [ ] Add placeholder `ReSpawn.ico` (use any 256x256 icon for now)
- [ ] Add placeholder `tray-icon.ico` (16x16 minimum)
- [ ] Add `default-icon.png` (128x128 generic game controller placeholder)
- [ ] Set all assets' Build Action to `Resource` in project properties

**AppData Path Helper**

- [ ] Create `Helpers/AppDataHelper.cs`:
  - [ ] `GetAppDataPath()` → `%AppData%\ReSpawn\`
  - [ ] `GetIconsPath()` → `%AppData%\ReSpawn\icons\`
  - [ ] `GetGamesFilePath()` → `%AppData%\ReSpawn\games.json`
  - [ ] `EnsureDirectoriesExist()` — creates all folders if missing (call on startup)

**Base Window**

- [ ] Build `MainWindow.xaml` — basic frameless window (no native chrome):
  - [ ] `WindowStyle="None"` + `AllowsTransparency="False"` + custom title bar strip
  - [ ] Drag region: `MouseLeftButtonDown` → `DragMove()`
  - [ ] Custom minimize `[—]`, maximize `[□]`, close `[×]` buttons
  - [ ] Override `OnClosing` → hide to tray (logic wired in Phase 6)

**Verify**

- [ ] Run the application — blank window opens, no crash
- [ ] Run the test project — 0 tests, 0 failures
- [ ] Commit: `"chore: initial project scaffold"`

---

### Phase 2 - Data Layer

> Goal: A fully tested `GameService` that reads, writes, and manages `games.json` with all edge cases handled. No UI involved.

**Models**

- [ ] Create `Models/GameSession.cs`:
  ```csharp
  public class GameSession
  {
      public string Id                { get; set; } = Guid.NewGuid().ToString();
      public DateTime Start           { get; set; }
      public DateTime End             { get; set; }
      public long DurationSeconds     { get; set; }
  }
  ```
- [ ] Create `Models/Game.cs`:
  ```csharp
  public class Game
  {
      public string           Id                   { get; set; } = Guid.NewGuid().ToString();
      public string           Name                 { get; set; } = string.Empty;
      public string           ExePath              { get; set; } = string.Empty;
      public string           ProcessName          { get; set; } = string.Empty;
      public string           IconPath             { get; set; } = string.Empty;
      public long             TotalPlaytimeSeconds { get; set; } = 0;
      public DateTime?        LastPlayed           { get; set; }
      public bool             IsPlaying            { get; set; } = false;
      public List<GameSession> Sessions            { get; set; } = new();
  }
  ```

**Helpers**

- [ ] Create `Helpers/AtomicFileWriter.cs`:
  - [ ] `WriteAllText(string path, string content)` — writes to `path.tmp`, then `File.Replace(path.tmp, path, path.bak)` for atomic swap
- [ ] Create `Helpers/TimeFormatter.cs`:
  - [ ] `FormatPlaytime(long totalSeconds)` → `"0h 05m"`, `"1h 00m"`, `"45h 30m"`
  - [ ] `FormatLastPlayed(DateTime? lastPlayed)` → `"Never"`, `"Today"`, `"Yesterday"`, `"3 days ago"`, `"Dec 15, 2025"` (beyond 7 days)
  - [ ] All methods are `static` and pure (no side effects)

**GameService**

- [ ] Create `Services/GameService.cs`:
  - [ ] `List<Game> LoadGames()` — reads `games.json`; if missing, returns empty list; if corrupt, backs up and returns empty list with a flag
  - [ ] `void SaveGames(List<Game> games)` — serializes with `JsonSerializerOptions { WriteIndented = true }`, calls `AtomicFileWriter.WriteAllText()`
  - [ ] `void AddGame(Game game)` — assigns `Guid.NewGuid()` if `game.Id` is empty, loads, appends, saves
  - [ ] `void UpdateGame(string id, Action<Game> mutate)` — loads, finds by `Id`, applies `mutate`, saves; throws `KeyNotFoundException` if not found
  - [ ] `void RemoveGame(string id)` — loads, removes matching entry, deletes `iconPath` file if it exists, saves
  - [ ] `void AppendSession(string gameId, GameSession session)` — loads, finds game, appends session, increments `TotalPlaytimeSeconds`, updates `LastPlayed`, sets `IsPlaying = false`, saves

**Unit Tests — GameService**

- [ ] `LoadGames_WhenFileNotFound_ReturnsEmptyList()`
- [ ] `LoadGames_WhenFileIsCorrupt_ReturnsEmptyListAndCreatesBackup()`
- [ ] `AddGame_AssignsGuidAndPersists()`
- [ ] `AddGame_DoesNotOverwriteExistingGuid()`
- [ ] `RemoveGame_RemovesCorrectEntry()`
- [ ] `RemoveGame_WithUnknownId_ThrowsKeyNotFoundException()`
- [ ] `AppendSession_UpdatesTotalPlaytimeAndLastPlayed()`
- [ ] `AppendSession_AppendsToSessionsArray()`
- [ ] `SaveGames_WritesValidJson()`
- [ ] `SaveGames_IsAtomic_DoesNotLeavePartialFile()`

**Unit Tests — TimeFormatter**

- [ ] `FormatPlaytime_ZeroSeconds_ReturnsZeroHoursZeroMinutes()`
- [ ] `FormatPlaytime_90Seconds_ReturnsZeroHoursOneMinute()`
- [ ] `FormatPlaytime_LargeValue_ReturnsCorrectHoursAndMinutes()`
- [ ] `FormatLastPlayed_Null_ReturnsNever()`
- [ ] `FormatLastPlayed_Today_ReturnsToday()`
- [ ] `FormatLastPlayed_Yesterday_ReturnsYesterday()`
- [ ] `FormatLastPlayed_OldDate_ReturnsFormattedDate()`

**Verify**

- [ ] All unit tests pass
- [ ] Manually inspect `games.json` written to `%AppData%\ReSpawn\` — confirm valid JSON
- [ ] Commit: `"feat: data layer — GameService, models, formatters, atomic writes"`

---

### Phase 3 - Game Launcher UI

> Goal: Users can see their game library, add games, launch them, edit, and remove them. No tracking yet.

**ViewModels**

- [ ] Create `Helpers/RelayCommand.cs` — standard `ICommand` wrapper:
  - [ ] Constructor: `RelayCommand(Action execute, Func<bool>? canExecute = null)`
  - [ ] `RelayCommand<T>` generic variant for parameterized commands
- [ ] Create `ViewModels/GameTileViewModel.cs`:
  - [ ] Exposes: `Id`, `Name`, `IconPath`, `FormattedPlaytime`, `FormattedLastPlayed`, `IsPlaying`
  - [ ] Implements `INotifyPropertyChanged`
  - [ ] `IsPlaying` setter triggers `OnPropertyChanged("StatusText")` and `OnPropertyChanged("BadgeVisible")`
- [ ] Create `ViewModels/MainViewModel.cs`:
  - [ ] `ObservableCollection<GameTileViewModel> Games` — bound to the tile grid
  - [ ] `ICommand AddGameCommand` — opens `AddGameDialog`
  - [ ] `ICommand LaunchGameCommand` — `Process.Start(game.ExePath)`; shows error if file missing
  - [ ] `ICommand EditGameCommand` — opens `EditGameDialog`
  - [ ] `ICommand RemoveGameCommand` — shows confirmation, calls `GameService.RemoveGame()`
  - [ ] `bool IsEmpty` — `true` when `Games.Count == 0`, drives empty state visibility
  - [ ] `LoadGamesFromDisk()` — called on startup; populates `Games` from `GameService.LoadGames()`
- [ ] Create `ViewModels/AddGameViewModel.cs`:
  - [ ] `ICommand BrowseCommand` — opens `OpenFileDialog(Filter = "Executables|*.exe")`
  - [ ] After file selection: auto-populate `Name`, `ProcessName`, trigger icon extraction preview
  - [ ] `ICommand ConfirmCommand` — validates fields, calls `GameService.AddGame()`, closes dialog
  - [ ] `ICommand CancelCommand` — discards, closes dialog
  - [ ] Input validation: `Name` and `ProcessName` must be non-empty; `ExePath` must exist on disk
- [ ] Create `ViewModels/EditGameViewModel.cs`:
  - [ ] Same shape as `AddGameViewModel` but pre-populated with existing `Game` data
  - [ ] `ConfirmCommand` calls `GameService.UpdateGame()` instead of `AddGame()`

**Controls**

- [ ] Build `Controls/GameTileControl.xaml`:
  - [ ] 160×200px card layout
  - [ ] Top 60%: game icon centered (fallback to `default-icon.png`)
  - [ ] Bottom 40%: name (bold, truncated with ellipsis), playtime, last played / "Now Playing" badge
  - [ ] Hover state: subtle scale transform (1.0 → 1.03) + drop shadow elevation — WPF `Trigger` on `IsMouseOver`
  - [ ] `IsPlaying = true` state: green pulsing dot `●` with a WPF `DoubleAnimation` on opacity (1.0 ↔ 0.3, 1s infinite)
  - [ ] Context menu: Launch, Edit, Remove (with separator before Remove)
  - [ ] Double-click fires `LaunchGameCommand`

**Views**

- [ ] Build `Views/HomeView.xaml`:
  - [ ] Header bar: "My Library" title + "+ Add Game" button (right-aligned)
  - [ ] `WrapPanel` inside `ScrollViewer` — tiles wrap naturally as window resizes
  - [ ] Empty state overlay: icon + "No games yet" message + "Add your first game" button
  - [ ] Empty state visibility bound to `MainViewModel.IsEmpty`
- [ ] Build `Views/AddGameDialog.xaml`:
  - [ ] Browse button + selected path display (read-only text box)
  - [ ] Extracted icon preview (64×64)
  - [ ] Editable `Name` field
  - [ ] Editable `ProcessName` field with helper tooltip: "This is the process name Windows uses. Usually the filename without .exe."
  - [ ] Add / Cancel buttons; Add disabled until validation passes
- [ ] Build `Views/EditGameDialog.xaml` — identical structure to Add, pre-populated

**Launch Error Handling**

- [ ] `Helpers/PathValidator.cs` — `bool Exists(string exePath)` wraps `File.Exists()`
- [ ] In `MainViewModel.LaunchGameCommand`: if path invalid, show `MessageBox` with "Game file not found at [path]. Would you like to re-link it?" — Yes opens `EditGameDialog`

**Verify**

- [ ] Add a game → tile appears on home screen
- [ ] Double-click tile → game process launches
- [ ] Right-click → Remove → game disappears
- [ ] Right-click → Edit → save new name → tile updates
- [ ] 0 games → empty state shown
- [ ] Commit: `"feat: game launcher UI — home screen, add/edit/remove, launch"`

---

### Phase 4 - Background Process Tracker

> Goal: Games are tracked automatically the moment their process is detected, regardless of launch source.

**ProcessMonitor Service**

- [ ] Create `Services/ProcessMonitor.cs`:
  - [ ] Constructor: accepts `GameService` instance (injected)
  - [ ] `void Start()` — initializes `System.Threading.Timer` with `Constants.PollIntervalMs`
  - [ ] `void Stop()` — disposes timer, saves any in-flight sessions cleanly
  - [ ] `Dictionary<string, DateTime> _activeSessions` — key: `processName`, value: `sessionStart` UTC
  - [ ] `void OnTick(object? state)` — the poll callback:
    1. Call `Process.GetProcesses()` — get array of all running process names (lowercased)
    2. Load current games list from `GameService.LoadGames()`
    3. For each game: compare `game.ProcessName.ToLower()` against the process name set
    4. **IDLE → PLAYING**: process found, game not in `_activeSessions` → add to dict, call `GameService.UpdateGame()` to set `IsPlaying = true`, raise `GameStarted` event
    5. **PLAYING → IDLE**: game in `_activeSessions`, process not found → compute duration, call `HandleSessionEnd()`
  - [ ] `void HandleSessionEnd(Game game, DateTime sessionStart)`:
    - [ ] `durationSeconds = (DateTime.UtcNow - sessionStart).TotalSeconds`
    - [ ] Remove from `_activeSessions`
    - [ ] If `durationSeconds < Constants.MinSessionSeconds` → log discard, call `GameService.UpdateGame()` to clear `IsPlaying`, return
    - [ ] If `durationSeconds > Constants.MaxSessionHours * 3600` → cap to max, log warning
    - [ ] Build `GameSession` with `Guid.NewGuid()`, start, end, duration
    - [ ] Call `GameService.AppendSession(game.Id, session)`
    - [ ] Raise `GameStopped` event with the completed session
  - [ ] `event EventHandler<GameStatusEventArgs> GameStarted`
  - [ ] `event EventHandler<GameStatusEventArgs> GameStopped`

**Orphan Cleanup**

- [ ] In `ProcessMonitor.Start()`, before the timer begins:
  - [ ] Load all games
  - [ ] Find any with `IsPlaying == true`
  - [ ] For each: call `GameService.UpdateGame(id, g => g.IsPlaying = false)`
  - [ ] Log each cleanup: `[Startup] Cleared orphaned session for {game.Name}`

**ViewModel Integration**

- [ ] In `MainViewModel`: subscribe to `ProcessMonitor.GameStarted` and `ProcessMonitor.GameStopped`
- [ ] On `GameStarted`: find matching `GameTileViewModel` by game ID, set `IsPlaying = true` (dispatched to UI thread via `Application.Current.Dispatcher.Invoke`)
- [ ] On `GameStopped`: set `IsPlaying = false`, update `FormattedPlaytime` and `FormattedLastPlayed`

**Unit Tests — ProcessMonitor**

- [ ] `OnTick_WhenProcessFound_StartsSession()`
- [ ] `OnTick_WhenProcessAlreadyTracked_DoesNotDuplicate()`
- [ ] `OnTick_WhenProcessDisappears_SavesSession()`
- [ ] `OnTick_WhenSessionUnder60s_DiscardsSession()`
- [ ] `OnTick_WhenSessionOverMaxHours_CapsAtMaxHours()`
- [ ] `Start_CleansOrphanedSessions()`
- [ ] `HandleSessionEnd_RaisesGameStoppedEvent()`

**Verify**

- [ ] Start a registered game from Steam → ReSpawn tile shows "Now Playing"
- [ ] Close the game → tile returns to normal, `games.json` contains a new session entry
- [ ] Start and immediately close game (< 60s) → no session saved, playtime unchanged
- [ ] Force-kill ReSpawn while game is running → reopen → `isPlaying` is `false`, no bad session
- [ ] Commit: `"feat: background process tracker with session recording and orphan cleanup"`

---

### Phase 5 - Icon Extraction Service

> Goal: Every game shows its real icon automatically. The Add Game flow never blocks or fails due to icon issues.

- [ ] Create `Services/IconExtractor.cs`:
  - [ ] `string Extract(string exePath, string gameId)`:
    1. Build output path: `Path.Combine(AppDataHelper.GetIconsPath(), $"{gameId}.png")`
    2. Call `Icon.ExtractAssociatedIcon(exePath)` — requires `using System.Drawing`
    3. Call `icon.ToBitmap()` → `bitmap.Save(outputPath, ImageFormat.Png)`
    4. Return `outputPath` (relative to AppData)
    5. Wrap in `try/catch(Exception)` — on any failure, return `"Assets/default-icon.png"` fallback path
    6. Log extraction failure to `Debug.WriteLine`
  - [ ] `void DeleteIcon(string iconPath)` — safe delete (catch if file not found)
  - [ ] `bool IconExists(string iconPath)` — wraps `File.Exists()`
- [ ] Wire `IconExtractor` into `AddGameViewModel.BrowseCommand` — extract preview icon immediately after `.exe` selection, before user confirms
- [ ] Wire `IconExtractor` into `GameService.RemoveGame()` — delete icon file when game is removed
- [ ] In `GameTileControl.xaml`: bind `Image.Source` to `IconPath`; add value converter `StringToImageSourceConverter` that falls back to `default-icon.png` if the file path resolves to nothing
- [ ] Create `Helpers/StringToImageSourceConverter.cs` — `IValueConverter` for WPF image binding with fallback

**Verify**

- [ ] Add PUBG → tile shows PUBG's real icon
- [ ] Add a game with no embedded icon → tile shows `default-icon.png`, no crash
- [ ] Remove a game → confirm its icon file is deleted from `%AppData%\ReSpawn\icons\`
- [ ] Commit: `"feat: icon extraction service with fallback and cleanup"`

---

### Phase 6 - System Tray Integration

> Goal: The app runs silently in the background. Window close hides to tray; tracker keeps running.

- [ ] Add `<tb:TaskbarIcon>` resource to `App.xaml` using `Hardcodet.NotifyIcon.Wpf`
- [ ] Set tray icon source to `Assets/tray-icon.ico`
- [ ] Build tray context menu in XAML with `WPF MenuItem` bindings:
  - [ ] **Open ReSpawn** → `Application.Current.MainWindow.Show(); MainWindow.Activate();`
  - [ ] Separator
  - [ ] **[Status Line]** — bound to `MainViewModel.TrayStatusText`:
    - [ ] `"● Now Playing: PUBG"` when a game is active
    - [ ] `"No game running"` when idle
    - [ ] This item is non-clickable (used as a display label)
  - [ ] Separator
  - [ ] **Exit** → `Application.Current.Shutdown()`
- [ ] Override `MainWindow.OnClosing`:
  - [ ] Set `e.Cancel = true` to prevent actual close
  - [ ] Call `this.Hide()`
  - [ ] Show a one-time balloon tip: "ReSpawn is still running. Right-click the tray icon to exit." (show only on first minimize, controlled by a `bool _shownTrayHint` flag)
- [ ] Wire tray icon `TrayMouseDoubleClick` → `MainWindow.Show(); Activate()`
- [ ] Add `MainViewModel.TrayStatusText` property:
  - [ ] Subscribe to `ProcessMonitor.GameStarted` → update to `"● Now Playing: {gameName}"`
  - [ ] Subscribe to `ProcessMonitor.GameStopped` → update to `"No game running"`
- [ ] Ensure `ProcessMonitor` is **not stopped** when window hides — it must keep running
- [ ] On `Application.Exit` (triggered by tray "Exit"): call `ProcessMonitor.Stop()` before shutdown to flush any in-flight session

**Verify**

- [ ] Click window close → window hides, tray icon appears, balloon shows once
- [ ] Double-click tray icon → window restores
- [ ] Launch a game → tray icon tooltip and menu status line updates to "Now Playing"
- [ ] Click tray Exit → app fully closes, session saved if game was running
- [ ] Commit: `"feat: system tray integration — hide to tray, live status, clean shutdown"`

---

### Phase 7 - Polish, Edge Cases & Release

> Goal: Production-quality build. All edge cases handled. Installer ready. Zero known crashes.

**Edge Case Hardening**

- [ ] **Broken exe path**: `PathValidator` check in `MainViewModel`; tile shows warning badge + "File not found" tooltip; right-click offers "Re-link executable"
- [ ] **Duplicate processName**: In `AddGameViewModel.ConfirmCommand` — check if any existing game shares `ProcessName` (case-insensitive); show warning dialog, require explicit confirmation to proceed
- [ ] **Corrupt `games.json`**: Covered in Phase 2; add integration test using `TestData/corrupt_games.json`
- [ ] **Hibernate/sleep cap**: In `HandleSessionEnd`, `durationSeconds > Constants.MaxSessionHours * 3600` → cap and log
- [ ] **Multiple instances of same process**: `Process.GetProcessesByName()` returns an array — presence of ≥ 1 instance counts as running; quantity does not affect session count
- [ ] **Icon file deleted manually**: `StringToImageSourceConverter` falls back to `default-icon.png` at render time
- [ ] **`games.json` write fails** (disk full, permissions): `AtomicFileWriter` wraps in `try/catch`, logs error with details, shows non-blocking error toast

**UI Polish**

- [ ] Smooth tile entrance animation: tiles fade in on home screen load (`OpacityAnimation` 0→1 staggered per tile)
- [ ] Hover elevation: scale 1.0→1.03 + shadow depth 2→6 on `GameTileControl` hover
- [ ] Session saved toast notification: `Hardcodet.NotifyIcon.Wpf` balloon → `"PUBG — 1h 35m recorded"`
- [ ] Loading skeleton: show 6 placeholder grey shimmer tiles while `games.json` loads on startup
- [ ] Window min-size constraint: `MinWidth="800" MinHeight="500"` — tiles always visible

**End-to-End Testing Checklist**

- [ ] Add game from Steam install dir → tile appears with correct icon, name, process name
- [ ] Launch game from ReSpawn → session tracked
- [ ] Launch same game from Steam → session still tracked by ReSpawn
- [ ] Close game → session appears in `games.json`, playtime on tile updates
- [ ] Short session (< 60s) → no session entry added
- [ ] Force-kill ReSpawn mid-game → reopen → tile shows correct playtime, no phantom session
- [ ] Remove game → tile gone, session history gone, icon file deleted
- [ ] Re-add same game → starts fresh with 0 playtime
- [ ] Corrupt `games.json` manually → reopen → backup created, fresh empty library shown
- [ ] Move game `.exe` to different path → tile shows "File not found" warning
- [ ] Open app with 50+ games → startup under 1s, no lag scrolling the grid

**Performance Profiling**

- [ ] Profile memory at idle (tray only): confirm < 30 MB
- [ ] Profile poll tick duration: confirm < 50ms for 50 registered games
- [ ] Profile `games.json` load: confirm < 100ms for 500 games

**Build & Distribution**

- [ ] Update `AssemblyInfo.cs` or `.csproj` with version `0.1.0`, product name `ReSpawn`, copyright
- [ ] Test self-contained single-file publish:
  ```
  dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
  ```
- [ ] Verify published `.exe` runs on a clean Windows machine with no .NET installed
- [ ] Write NSIS `.nsi` installer script:
  - [ ] Install to `%ProgramFiles%\ReSpawn\`
  - [ ] Add Start Menu shortcut
  - [ ] Add Desktop shortcut (optional, prompted)
  - [ ] Register Windows startup entry (optional, prompted)
  - [ ] Uninstaller that removes `%ProgramFiles%\ReSpawn\` (but not `%AppData%\ReSpawn\` — preserve user data)
- [ ] Build installer → test install and uninstall on a clean VM
- [ ] Tag release `v0.1.0` in Git
- [ ] Create GitHub Release with attached installer `.exe` and changelog

**Commit:** `"release: v0.1.0 — MVP production build"`

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
